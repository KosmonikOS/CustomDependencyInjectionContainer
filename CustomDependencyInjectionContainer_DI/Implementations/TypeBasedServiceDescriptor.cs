using CustomDependencyInjectionContainer_DI.Abstractions;

namespace CustomDependencyInjectionContainer_DI.Implementations;
public class TypeBasedServiceDescriptor : ServiceDescriptor
{
    public Type ImplementationType { get; init; }
}
