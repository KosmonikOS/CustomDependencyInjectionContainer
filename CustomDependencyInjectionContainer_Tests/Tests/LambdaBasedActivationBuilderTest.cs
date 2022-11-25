using CustomDependencyInjectionContainer_DI.Abstractions;
using CustomDependencyInjectionContainer_DI.Enums;
using CustomDependencyInjectionContainer_DI.Implementations;
using CustomDependencyInjectionContainer_Tests.Models;
using Moq;
using Xunit;

namespace CustomDependencyInjectionContainer_Tests.Tests;

public class LambdaBasedActivationBuilderTest
{
    private readonly LambdaBasedActivationBuilder _sut = new();
    private readonly Mock<IScope> _scope = new();

    [Fact]
    public void BuildActivation_ShouldReturnFunc_WithNoNestedServices()
    {
        var descriptor = new TypeBasedServiceDescriptor()
        {
            ServiceType = typeof(ITestService),
            ImplementationType = typeof(TestService),
            Lifetime = Lifetime.Transient
        };
        _scope.Setup(x=>x.Resolve(It.IsAny<Type>())).Verifiable();
        var func = _sut.BuildActivation(descriptor);
        var obj = func(_scope.Object);
        _scope.Verify(x=>x.Resolve(It.IsAny<Type>()),Times.Never);
        Assert.NotNull(obj);
        Assert.IsType<TestService>(obj);
    }

    [Fact]
    public void BuildActivation_Should_ReturnFunc_WithNestedServices()
    {
        var descriptor = new TypeBasedServiceDescriptor()
        {
            ServiceType = typeof(ITestServiceWithNested),
            ImplementationType = typeof(TestServiceWithNested),
            Lifetime = Lifetime.Transient
        };
        _scope.Setup(x => x.Resolve(typeof(ITestService)))
            .Returns(new TestService());
        var func = _sut.BuildActivation(descriptor);
        var obj = (ITestServiceWithNested)func(_scope.Object);
        Assert.NotNull(obj);
        Assert.IsType<TestServiceWithNested>(obj);
        Assert.NotNull(obj.Nested);
        Assert.IsType<TestService>(obj.Nested);
    }
}