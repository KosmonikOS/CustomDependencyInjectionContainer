using CustomDependencyInjectionContainer_DI.Extentions;
using CustomDependencyInjectionContainer_DI.Implementations;

var builder = new ContainerBuilder();
builder.AddSingleton(typeof(IService<>), typeof(Service1<>));
var container = builder.Build();
using (container)
{
    using (var scope1 = container.CreateScope())
    {
        var service1 = scope1.Resolve(typeof(IService<int>));
    }
}

public interface IService<T>
{
    public void Do();
}
public class Service1<T> : IService<T>
{
    public void Do()
    {
        Console.WriteLine();
    }
}