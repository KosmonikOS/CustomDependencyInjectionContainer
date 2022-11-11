using CustomDependencyInjectionContainer_DI.Abstractions;
using System.Linq.Expressions;
using System.Reflection;

namespace CustomDependencyInjectionContainer_DI.Implementations;
public class LambdaBasedActivationBuilder : ActivationBuilder
{
    private static readonly MethodInfo ResolveMethod = typeof(IScope).GetMethod("Resolve");
    protected override Func<IScope, object> BuildActivationInternal(TypeBasedServiceDescriptor descriptor, ConstructorInfo ctor, ParameterInfo[] args)
    {
        var param = Expression.Parameter(typeof(IScope));
        var resolvedArgs = args.Select(x =>
            Expression.Convert(Expression.Call(param, ResolveMethod,
                Expression.Constant(x.ParameterType)), x.ParameterType));
        var creation = Expression.New(ctor, resolvedArgs);
        return Expression.Lambda<Func<IScope, object>>(creation, param).Compile();
    }
}
