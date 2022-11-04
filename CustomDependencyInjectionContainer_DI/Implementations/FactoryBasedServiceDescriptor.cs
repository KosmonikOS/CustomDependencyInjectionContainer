using CustomDependencyInjectionContainer_DI.Abstractions;

namespace CustomDependencyInjectionContainer_DI.Implementations;
public class FactoryBasedServiceDescriptor : ServiceDescriptor
{
    public Func<IScope, object> Factory { get; init; }
}
