using CustomDependencyInjectionContainer_DI.Abstractions;

namespace CustomDependencyInjectionContainer_DI.Implementations;
public class ContainerBuilder : IContainerBuilder
{
    private readonly List<ServiceDescriptor> descriptors = new();
    private ActivationBuilder activationBuilder = new LambdaBasedActivationBuilder();
    public IContainerBuilder Add(ServiceDescriptor descriptor)
    {
        descriptors.Add(descriptor);
        return this;
    }
    public IContainer Build()
    {
        return new Container(descriptors,activationBuilder);
    }
    public IContainerBuilder UseReflection()
    {
        activationBuilder = new ReflectionBasedActivationBuilder();
        return this;
    }
}
