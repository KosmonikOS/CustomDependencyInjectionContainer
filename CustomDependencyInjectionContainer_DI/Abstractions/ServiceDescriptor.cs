using CustomDependencyInjectionContainer_DI.Enums;
using CustomDependencyInjectionContainer_DI.Implementations;

namespace CustomDependencyInjectionContainer_DI.Abstractions;
public abstract class ServiceDescriptor
{
    public Type ServiceType { get; init; }
    public Lifetime Lifetime { get; init; }

    public virtual MultipleServiceDescriptor ConvertToMultiple()
    {
        return new MultipleServiceDescriptor()
        {
            ServiceType = this.ServiceType,
            Lifetime = Lifetime.Transient,
            Descriptors = new[] { this }
        };
    }
}
