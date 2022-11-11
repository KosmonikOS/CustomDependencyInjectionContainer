namespace CustomDependencyInjectionContainer_DI.Abstractions;
public interface IContainerBuilder
{
    public IContainer Build();
    public IContainerBuilder Add(ServiceDescriptor descriptor);
    public IContainerBuilder UseReflection();
}
