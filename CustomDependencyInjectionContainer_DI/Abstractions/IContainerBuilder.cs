using CustomDependencyInjectionContainer_DI.Abstractions;

namespace CustomDependencyInjectionContainer_DI.Interfaces;
public interface IContainerBuilder
{
    public IContainer Build();
    public void Add(ServiceDescriptor descriptor);
}
