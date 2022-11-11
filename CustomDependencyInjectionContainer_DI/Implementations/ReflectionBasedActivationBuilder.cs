using CustomDependencyInjectionContainer_DI.Abstractions;
using System.Reflection;

namespace CustomDependencyInjectionContainer_DI.Implementations;
public class ReflectionBasedActivationBuilder : ActivationBuilder
{
    protected override Func<IScope, object> BuildActivationInternal(TypeBasedServiceDescriptor descriptor, ConstructorInfo ctor, ParameterInfo[] args)
    {
        return x =>
        {
            var argsForCreation = new object[args.Length];
            for (int i = 0; i < args.Length; i++)
                argsForCreation[i] = x.Resolve(args[i].ParameterType);
            return ctor.Invoke(argsForCreation);
        };
    }
}
