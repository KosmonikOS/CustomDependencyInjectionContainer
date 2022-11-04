namespace CustomDependencyInjectionContainer_DI.Abstractions;
public interface IScope
{
    public object Resolve(Type serviceType);
}
