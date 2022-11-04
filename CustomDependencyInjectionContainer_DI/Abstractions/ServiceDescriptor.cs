using CustomDependencyInjectionContainer_DI.Enums;

namespace CustomDependencyInjectionContainer_DI.Abstractions;
public abstract class ServiceDescriptor
{
    public Type ServiceType { get; init; }
    public Lifetime Lifetime { get; init; }
}
