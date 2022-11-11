using CustomDependencyInjectionContainer_DI.Implementations;
using System.Reflection;

namespace CustomDependencyInjectionContainer_DI.Abstractions;
public abstract class ActivationBuilder
{
    public Func<IScope, object> BuildActivation(TypeBasedServiceDescriptor descriptor)
    {
        var ctor = descriptor.ImplementationType.GetConstructors(BindingFlags.Public
            | BindingFlags.Instance).Single();
        var args = ctor.GetParameters();
        return BuildActivationInternal(descriptor,ctor,args);
    }
    protected abstract Func<IScope, object> BuildActivationInternal(TypeBasedServiceDescriptor descriptor, ConstructorInfo ctor, ParameterInfo[] args);
}
