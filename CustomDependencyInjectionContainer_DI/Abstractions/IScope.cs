namespace CustomDependencyInjectionContainer_DI.Abstractions;
public interface IScope :IDisposable,IAsyncDisposable
{
    public object Resolve(Type serviceType);
}
