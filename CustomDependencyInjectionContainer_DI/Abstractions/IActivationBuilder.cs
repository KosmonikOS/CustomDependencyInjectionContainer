using CustomDependencyInjectionContainer_DI.Implementations;

namespace CustomDependencyInjectionContainer_DI.Abstractions;

public interface IActivationBuilder
{
    public Func<IScope, object> BuildActivation(TypeBasedServiceDescriptor descriptor);
}