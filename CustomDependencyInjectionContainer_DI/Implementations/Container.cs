using System.Collections;
using CustomDependencyInjectionContainer_DI.Abstractions;
using CustomDependencyInjectionContainer_DI.Enums;
using System.Collections.Concurrent;

namespace CustomDependencyInjectionContainer_DI.Implementations;
public class Container : IContainer
{
    private readonly ConcurrentDictionary<Type, ServiceDescriptor> _descriptors = new();
    private readonly ConcurrentDictionary<ServiceDescriptor, Func<IScope, object>> _activators = new();
    private readonly ActivationBuilder _activationBuilder;
    private readonly Scope _rootScope;
    private bool _disposed = false;
    public Container(IEnumerable<ServiceDescriptor> sDescriptors, ActivationBuilder activationBuilder)
    {
        foreach (var dg in sDescriptors.GroupBy(x => x.ServiceType))
        {
            var descriptors = dg.ToArray();
            if (descriptors.Length == 1)
                this._descriptors[dg.Key] = descriptors[0];
            else
            {
                this._descriptors[dg.Key] = new MultipleServiceDescriptor()
                {
                    Descriptors = descriptors,
                    Lifetime = Lifetime.Transient,
                    ServiceType = dg.Key
                };
            }
        }
        _rootScope = new Scope(this);
        this._activationBuilder = activationBuilder;
    }
    public IScope CreateScope()
    {
        if (_disposed)
            throw new InvalidOperationException("Scope has been already disposed");
        return new Scope(this);
    }
    public void Dispose()
    {
        _rootScope.Dispose();
        _disposed = true;
    }
    public ValueTask DisposeAsync()
    {
        var task = _rootScope.DisposeAsync();
        _disposed = true;
        return task;
    }
    private ServiceDescriptor FindDescriptor(Type serviceType)
    {
        if (_descriptors.TryGetValue(serviceType, out var descriptor))
            return descriptor;
        if (serviceType.IsAssignableTo(typeof(IEnumerable)) && serviceType.IsGenericType
             && serviceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
        {
            var nestedServiceType = serviceType.GetGenericArguments()[0];
            return nestedServiceType.IsGenericType
                ? BuildMultipleGenericDescriptor(nestedServiceType, serviceType)
                : BuildMultipleDescriptor(nestedServiceType, serviceType);
        }
        if (serviceType.IsConstructedGenericType)
            return BuildGenericDescriptor(serviceType);
        throw new InvalidOperationException($"Unable to find {serviceType}");
    }
    private ServiceDescriptor BuildMultipleGenericDescriptor(Type serviceType, Type collectionType)
    {
        var generic = serviceType.GetGenericTypeDefinition();
        var gArgs = serviceType.GetGenericArguments();
        if (FindDescriptor(generic) is MultipleServiceDescriptor md)
        {
            return _descriptors.GetOrAdd(collectionType, new FactoryBasedServiceDescriptor()
            {
                ServiceType = serviceType,
                Lifetime = Lifetime.Transient,
                Factory = s =>
                {
                    var scope = (Scope)s;
                    var services = Array.CreateInstance(serviceType, md.Descriptors.Length);
                    for (var i = 0; i < services.Length; i++)
                    {
                        var gDescriptor = MakeDescriptorGeneric(md.Descriptors[i], gArgs);
                        services.SetValue(scope.ResolveDescriptor(gDescriptor), i);
                    }
                    return services;
                }
            });
        }
        throw new InvalidOperationException($"{serviceType} has multiple implementations , cannot choose one");
    }
    private ServiceDescriptor BuildMultipleDescriptor(Type serviceType, Type collectionType)
    {
        if (FindDescriptor(serviceType) is MultipleServiceDescriptor md)
        {
            return _descriptors.GetOrAdd(collectionType, new FactoryBasedServiceDescriptor()
            {
                ServiceType = serviceType,
                Lifetime = Lifetime.Transient,
                Factory = s =>
                {
                    var scope = (Scope)s;
                    var services = Array.CreateInstance(serviceType, md.Descriptors.Length);
                    for (var i = 0; i < services.Length; i++)
                        services.SetValue(scope.ResolveDescriptor(md.Descriptors[i]), i);
                    return services;
                }
            });
        }
        throw new InvalidOperationException($"{serviceType} has multiple implementations , cannot choose one");
    }
    private ServiceDescriptor MakeDescriptorGeneric(ServiceDescriptor descriptor, Type[] args)
    {
        if (descriptor is TypeBasedServiceDescriptor tb)
        {
            var concreteImplementation = tb.ImplementationType.MakeGenericType(args);
            return new TypeBasedServiceDescriptor()
            {
                Lifetime = descriptor.Lifetime,
                ImplementationType = concreteImplementation,
                ServiceType = descriptor.ServiceType
            };
        }
        throw new InvalidOperationException($"Generic {descriptor.ServiceType} can only be registered with implementation type");
    }
    private ServiceDescriptor BuildGenericDescriptor(Type serviceType)
    {
        var generic = serviceType.GetGenericTypeDefinition();
        var gDescriptor = FindDescriptor(generic);
        if (gDescriptor is TypeBasedServiceDescriptor tb)
        {
            var gArgs = serviceType.GetGenericArguments();
            var concreteImplementation = tb.ImplementationType
                .MakeGenericType(gArgs);
            return _descriptors.GetOrAdd(serviceType, new TypeBasedServiceDescriptor()
            {
                Lifetime = gDescriptor.Lifetime,
                ImplementationType = concreteImplementation,
                ServiceType = serviceType
            });
        }
        throw new InvalidOperationException($"Generic {serviceType} can only be registered with implementation type");
    }
    private object CreateInstance(ServiceDescriptor descriptor, IScope scope)
    {
        return _activators.GetOrAdd(descriptor, x => CreateActivator(descriptor))(scope);
    }
    private Func<IScope, object> CreateActivator(ServiceDescriptor descriptor)
    {
        if (descriptor is InstanceBasedServiceDescriptor id)
            return _ => id.Instance;
        if (descriptor is FactoryBasedServiceDescriptor fd)
            return fd.Factory;
        var td = (TypeBasedServiceDescriptor)descriptor;
        return _activationBuilder.BuildActivation(td);
    }

    private class Scope : IScope
    {
        private readonly Container _container;
        private readonly ConcurrentDictionary<ServiceDescriptor, object> _scopedServices = new();
        private readonly ConcurrentStack<object> _disposables = new();
        private bool _disposed = false;
        public Scope(Container container)
        {
            this._container = container;
        }
        public object Resolve(Type serviceType)
        {
            if (_disposed)
                throw new InvalidOperationException("Container has been already disposed");
            var descriptor = _container.FindDescriptor(serviceType);
            return ResolveDescriptor(descriptor);
        }
        public void Dispose()
        {
            foreach (var disposable in _disposables)
            {
                if (disposable is IDisposable d)
                    d.Dispose();
                else if (disposable is IAsyncDisposable ad)
                    ad.DisposeAsync().GetAwaiter().GetResult();
            }
            _disposed = true;
        }
        public async ValueTask DisposeAsync()
        {
            foreach (var disposable in _disposables)
            {
                if (disposable is IAsyncDisposable ad)
                    await ad.DisposeAsync();
                else if (disposable is IDisposable d)
                    d.Dispose();
            }
            _disposed = true;
        }
        internal object ResolveDescriptor(ServiceDescriptor descriptor)
        {
            if (descriptor.Lifetime == Lifetime.Transient)
                return CreateDisposableInstance(descriptor);
            if (descriptor.Lifetime == Lifetime.Scoped || this == _container._rootScope)
                return _scopedServices.GetOrAdd(descriptor,
                    x => CreateDisposableInstance(descriptor));
            return _container._rootScope.ResolveDescriptor(descriptor);
        }
        private object CreateDisposableInstance(ServiceDescriptor descriptor)
        {
            var instance = _container.CreateInstance(descriptor, this);
            if (instance is IDisposable || instance is IAsyncDisposable)
                _disposables.Push(instance);
            return instance;
        }
    }
}
