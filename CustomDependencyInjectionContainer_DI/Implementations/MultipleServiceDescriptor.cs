using CustomDependencyInjectionContainer_DI.Abstractions;
using CustomDependencyInjectionContainer_DI.Enums;

namespace CustomDependencyInjectionContainer_DI.Implementations;
public class MultipleServiceDescriptor : ServiceDescriptor
{
    public ServiceDescriptor[] Descriptors { get; init; }
    public override MultipleServiceDescriptor ConvertToMultiple() => this;
}