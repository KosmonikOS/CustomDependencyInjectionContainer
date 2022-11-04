using CustomDependencyInjectionContainer_DI.Abstractions;
using CustomDependencyInjectionContainer_DI.Enums;
using CustomDependencyInjectionContainer_DI.Implementations;

namespace CustomDependencyInjectionContainer_DI.Extentions;
public static class ContainerBuilderExtentions
{
    public static IContainerBuilder AddTransient(this IContainerBuilder builder, Type serviceType, Type implementation)
        => builder.AddType(serviceType, implementation, Lifetime.Transient);
    public static IContainerBuilder AddScoped(this IContainerBuilder builder, Type serviceType, Type implementation)
        => builder.AddType(serviceType, implementation, Lifetime.Scoped);
    public static IContainerBuilder AddSingleton(this IContainerBuilder builder, Type serviceType, Type implementation)
        => builder.AddType(serviceType, implementation, Lifetime.Singleton);
    public static IContainerBuilder AddTransient(this IContainerBuilder builder, Type serviceType, Func<IScope, object> factory)
        => builder.AddFactory(serviceType, factory, Lifetime.Transient);
    public static IContainerBuilder AddScoped(this IContainerBuilder builder, Type serviceType, Func<IScope, object> factory)
        => builder.AddFactory(serviceType, factory, Lifetime.Scoped);
    public static IContainerBuilder AddSingleton(this IContainerBuilder builder, Type serviceType, Func<IScope, object> factory)
        => builder.AddFactory(serviceType, factory, Lifetime.Singleton);
    public static IContainerBuilder AddSingleton(this IContainerBuilder builder, Type serviceType, object instance)
        => builder.AddInstance(serviceType, instance);
    public static IContainerBuilder AddTransient<TService, TImplementation>(this IContainerBuilder builder)
        => builder.AddType(typeof(TService), typeof(TImplementation), Lifetime.Transient);
    public static IContainerBuilder AddScoped<TService, TImplementation>(this IContainerBuilder builder)
        => builder.AddType(typeof(TService), typeof(TImplementation), Lifetime.Scoped);
    public static IContainerBuilder AddSingleton<TService, TImplementation>(this IContainerBuilder builder)
        => builder.AddType(typeof(TService), typeof(TImplementation), Lifetime.Singleton);
    private static IContainerBuilder AddType(this IContainerBuilder builder,
        Type serviceType, Type implementation, Lifetime lifetime)
    {
        builder.Add(new TypeBasedServiceDescriptor()
        {
            ImplementationType = implementation,
            ServiceType = serviceType,
            Lifetime = lifetime
        });
        return builder;
    }
    private static IContainerBuilder AddFactory(this IContainerBuilder builder,
        Type serviceType, Func<IScope, object> factory, Lifetime lifetime)
    {
        builder.Add(new FactoryBasedServiceDescriptor()
        {
            ServiceType = serviceType,
            Factory = factory,
            Lifetime = lifetime
        });
        return builder;
    }
    private static IContainerBuilder AddInstance(this IContainerBuilder builder,
        Type serviceType, object instance)
    {
        builder.Add(new InstanceBasedServiceDescriptor()
        {
            ServiceType = serviceType,
            Instance = instance,
            Lifetime = Lifetime.Singleton
        });
        return builder;
    }
}
