using CustomDependencyInjectionContainer_DI.Abstractions;
using CustomDependencyInjectionContainer_DI.Enums;
using System.Collections.Concurrent;
using System.Reflection;

namespace CustomDependencyInjectionContainer_DI.Implementations;
public class Container : IContainer
{
    private readonly Dictionary<Type, ServiceDescriptor> descriptors;
    private readonly ConcurrentDictionary<Type, Func<IScope, object>> activators = new();
    private readonly Scope rootScope;
    private bool disposed = false;
    public Container(List<ServiceDescriptor> descriptors)
    {
        this.descriptors = descriptors.ToDictionary(x => x.ServiceType);
        rootScope = new Scope(this);
    }
    public IScope CreateScope()
    {
        if (disposed)
            throw new InvalidOperationException("Scope has been already disposed");
        return new Scope(this);
    }
    private ServiceDescriptor FindDescriptor(Type serviceType)
    {
        var descriptor = descriptors[serviceType];
        if (descriptor == null)
            throw new InvalidOperationException($"Unable to find {serviceType}");
        return descriptor;

    }
    private object CreateInstance(Type serviceType, IScope scope)
    {
        return activators.GetOrAdd(serviceType,
            x => CreateActivator(serviceType))(scope);
    }
    private Func<IScope, object> CreateActivator(Type serviceType)
    {
        var descriptor = FindDescriptor(serviceType);
        if (descriptor is InstanceBasedServiceDescriptor id)
            return _ => id.Instance;
        if (descriptor is FactoryBasedServiceDescriptor fd)
            return fd.Factory;
        var td = (TypeBasedServiceDescriptor)descriptor;
        var ctor = td.ImplementationType.GetConstructors(BindingFlags.Public
            | BindingFlags.Instance).Single();
        var args = ctor.GetParameters();
        return x =>
        {
            var argsForCreation = new object[args.Length];
            for (int i = 0; i < args.Length; i++)
                argsForCreation[i] = CreateInstance(args[i].ParameterType, x);
            return ctor.Invoke(argsForCreation);
        };
    }
    public void Dispose()
    {
        rootScope.Dispose();
        disposed = true;
    }
    public ValueTask DisposeAsync()
    {
        var task = rootScope.DisposeAsync();
        disposed = true;
        return task;
    }
    private class Scope : IScope
    {
        private readonly Container container;
        private readonly ConcurrentDictionary<Type, object> scopedServices = new();
        private readonly ConcurrentStack<object> disposables = new();
        private bool disposed = false;
        public Scope(Container container)
        {
            this.container = container;
        }
        public object Resolve(Type serviceType)
        {
            if (disposed)
                throw new InvalidOperationException("Container has been already disposed");
            var descriptor = container.FindDescriptor(serviceType);
            return ResolveDescriptor(descriptor);
        }
        private object ResolveDescriptor(ServiceDescriptor descriptor)
        {
            if (descriptor.Lifetime == Lifetime.Transient)
                return CreateDisposableInstance(descriptor.ServiceType);
            else if (descriptor.Lifetime == Lifetime.Scoped || this == container.rootScope)
                return scopedServices.GetOrAdd(descriptor.ServiceType,
                    x => CreateDisposableInstance(descriptor.ServiceType));
            return container.rootScope.ResolveDescriptor(descriptor);
        }
        private object CreateDisposableInstance(Type serviceType)
        {
            var instance = container.CreateInstance(serviceType, this);
            if (instance is IDisposable || instance is IAsyncDisposable)
                disposables.Push(instance);
            return instance;
        }
        public void Dispose()
        {
            foreach (var disposable in disposables)
            {
                if (disposable is IDisposable d)
                    d.Dispose();
                else if (disposable is IAsyncDisposable ad)
                    ad.DisposeAsync().GetAwaiter().GetResult();
            }
            disposed = true;
        }
        public async ValueTask DisposeAsync()
        {
            foreach (var disposable in disposables)
            {
                if (disposable is IAsyncDisposable ad)
                    await ad.DisposeAsync();
                else if (disposable is IDisposable d)
                    d.Dispose();
            }
            disposed = true;
        }
    }
}
