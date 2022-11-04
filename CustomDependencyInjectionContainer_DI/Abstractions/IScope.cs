namespace CustomDependencyInjectionContainer_DI.Interfaces;
public interface IScope
{
    public object Resolve(Type serviceType);
}
