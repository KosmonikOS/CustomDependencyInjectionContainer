namespace CustomDependencyInjectionContainer_DI.Abstractions;
public interface IContainerBuilder
{
    public IContainer Build();
    public void Add(ServiceDescriptor descriptor);
}
