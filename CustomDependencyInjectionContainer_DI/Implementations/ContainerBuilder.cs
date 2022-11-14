using CustomDependencyInjectionContainer_DI.Abstractions;

namespace CustomDependencyInjectionContainer_DI.Implementations;
public class ContainerBuilder : IContainerBuilder
{
    private readonly List<ServiceDescriptor> _descriptors = new();
    private ActivationBuilder _activationBuilder = new LambdaBasedActivationBuilder();
    public IContainerBuilder Add(ServiceDescriptor descriptor)
    {
        _descriptors.Add(descriptor);
        return this;
    }
    public IContainer Build()
    {
        return new Container(_descriptors,_activationBuilder);
    }
    public IContainerBuilder UseReflection()
    {
        _activationBuilder = new ReflectionBasedActivationBuilder();
        return this;
    }
}
