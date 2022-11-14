using CustomDependencyInjectionContainer_DI.Abstractions;

namespace CustomDependencyInjectionContainer_DI.Implementations;
public class MultipleServiceDescriptor : ServiceDescriptor
{
    public ServiceDescriptor[] Descriptors { get; init; }
}