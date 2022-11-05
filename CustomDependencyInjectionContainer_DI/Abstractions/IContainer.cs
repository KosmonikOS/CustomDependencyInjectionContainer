namespace CustomDependencyInjectionContainer_DI.Abstractions;
public interface IContainer :IDisposable,IAsyncDisposable
{
    public IScope CreateScope();
}
