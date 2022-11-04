using CustomDependencyInjectionContainer_DI.Abstractions;

namespace CustomDependencyInjectionContainer_DI.Implementations;
public class InstanceBasedServiceDescriptor: ServiceDescriptor
{
    public object Instance { get; init; }
}