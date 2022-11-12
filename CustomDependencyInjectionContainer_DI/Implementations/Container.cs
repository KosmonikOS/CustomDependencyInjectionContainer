using CustomDependencyInjectionContainer_DI.Abstractions;
using CustomDependencyInjectionContainer_DI.Enums;
using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;

namespace CustomDependencyInjectionContainer_DI.Implementations;
public class Container : IContainer
{
    private readonly Dictionary<Type, ServiceDescriptor> descriptors = new();
    private readonly ConcurrentDictionary<ServiceDescriptor, Func<IScope, object>> activators = new();
    private readonly ActivationBuilder activationBuilder;
    private readonly Scope rootScope;
    private bool disposed = false;
    public Container(IEnumerable<ServiceDescriptor> sDescriptors, ActivationBuilder activationBuilder)
    {
        foreach (var dg in sDescriptors.GroupBy(x => x.ServiceType))
        {
            var items = dg.ToArray();
            if (items.Length == 1)
                this.descriptors.Add(dg.Key, items[0]);
            else
            {
                var md = new MultipleServiceDescriptor() { Descriptors = items, ServiceType = dg.Key };
                var genericType = typeof(IEnumerable<>).MakeGenericType(dg.Key);
                this.descriptors.Add(genericType, BuildMultipleDescriptor(genericType, md));
            }
        }
        rootScope = new Scope(this);
        this.activationBuilder = activationBuilder;
    }
    public IScope CreateScope()
    {
        if (disposed)
            throw new InvalidOperationException("Scope has been already disposed");
        return new Scope(this);
    }
    private ServiceDescriptor BuildMultipleDescriptor(Type serviceType, MultipleServiceDescriptor md)
    {
        return new FactoryBasedServiceDescriptor()
        {
            ServiceType = serviceType,
            Lifetime = Lifetime.Transient,
            Factory = s =>
            {
                var scope = (Scope)s;
                var services = Array.CreateInstance(md.ServiceType, md.Descriptors.Length);
                for (int i = 0; i < services.Length; i++)
                    services.SetValue(scope.ResolveDescriptor(md.Descriptors[i]), i);
                return services;
            }
        };
    }
    private ServiceDescriptor FindDescriptor(Type serviceType)
    {
        if (!descriptors.TryGetValue(serviceType, out var descriptor))
            throw new InvalidOperationException($"Unable to find {serviceType}");
        return descriptor;
    }
    private object CreateInstance(ServiceDescriptor descriptor, IScope scope)
    {
        return activators.GetOrAdd(descriptor, x => CreateActivator(descriptor))(scope);
    }
    private Func<IScope, object> CreateActivator(ServiceDescriptor descriptor)
    {
        if (descriptor is InstanceBasedServiceDescriptor id)
            return _ => id.Instance;
        if (descriptor is FactoryBasedServiceDescriptor fd)
            return fd.Factory;
        var td = (TypeBasedServiceDescriptor)descriptor;
        return activationBuilder.BuildActivation(td);
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
        internal object ResolveDescriptor(ServiceDescriptor descriptor)
        {
            if (descriptor.Lifetime == Lifetime.Transient)
                return CreateDisposableInstance(descriptor);
            else if (descriptor.Lifetime == Lifetime.Scoped || this == container.rootScope)
                return scopedServices.GetOrAdd(descriptor.ServiceType,
                    x => CreateDisposableInstance(descriptor));
            return container.rootScope.ResolveDescriptor(descriptor);
        }
        private object CreateDisposableInstance(ServiceDescriptor descriptor)
        {
            var instance = container.CreateInstance(descriptor, this);
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
