using CustomDependencyInjectionContainer_DI.Abstractions;

namespace CustomDependencyInjectionContainer_DI.Implementations;
public class ContainerBuilder : IContainerBuilder
{
    private readonly List<ServiceDescriptor> descriptors = new();
    private ActivationBuilder activationBuilder = new ReflectionBasedActivationBuilder();
    public void Add(ServiceDescriptor descriptor)
    {
        descriptors.Add(descriptor);
    }
    public IContainer Build()
    {
        return new Container(descriptors,activationBuilder);
    }
}
