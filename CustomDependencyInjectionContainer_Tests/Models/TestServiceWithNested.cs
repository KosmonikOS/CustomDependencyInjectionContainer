namespace CustomDependencyInjectionContainer_Tests.Models;

public interface ITestServiceWithNested
{
    public ITestService Nested { get; set; }
}

public class TestServiceWithNested : ITestServiceWithNested
{
    public TestServiceWithNested(ITestService service)
    {
        Nested = service;
    }
    public ITestService Nested { get; set; }
}