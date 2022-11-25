using System.Collections;
using CustomDependencyInjectionContainer_DI.Abstractions;
using CustomDependencyInjectionContainer_DI.Enums;
using CustomDependencyInjectionContainer_DI.Implementations;
using CustomDependencyInjectionContainer_Tests.Models;
using Moq;
using Xunit;

namespace CustomDependencyInjectionContainer_Tests.Tests;

public class ContainerAndScopeTest
{
    private readonly Mock<IActivationBuilder> _aBuilder = new();
    [Fact]
    public void Container_CreateScope_ShouldReturnNewScopeConnectedWithContainer()
    {
        var container = new Container(new ServiceDescriptor[] { }, _aBuilder.Object);
        var scope = container.CreateScope();
        Assert.NotNull(scope);
    }
    [Fact]
    public void Scope_Resolve_ShouldReturnService_WithNoNested()
    {
        var descriptor = new TypeBasedServiceDescriptor()
        {
            ServiceType = typeof(ITestService),
            ImplementationType = typeof(TestService),
            Lifetime = Lifetime.Transient
        };
        _aBuilder.Setup(x => x.BuildActivation(descriptor))
            .Returns(_ => new TestService());
        var container = new Container(new ServiceDescriptor[] { descriptor }, _aBuilder.Object);
        var scope = container.CreateScope();
        var service = scope.Resolve(typeof(ITestService));
        Assert.NotNull(service);
        Assert.IsType<TestService>(service);
    }
    [Fact]
    public void Scope_Resolve_ShouldReturnService_WithGeneric()
    {
        var descriptor = new TypeBasedServiceDescriptor()
        {
            ServiceType = typeof(IGenericService<>),
            ImplementationType = typeof(GenericService<>),
            Lifetime = Lifetime.Transient
        };
        _aBuilder.Setup(x => x.BuildActivation(
                It.Is<TypeBasedServiceDescriptor>(x =>
                    x.ImplementationType.GetGenericArguments().Single() == typeof(int))))
            .Returns(_ => new GenericService<int>());
        var container = new Container(new ServiceDescriptor[] { descriptor }, _aBuilder.Object);
        var scope = container.CreateScope();
        var service = scope.Resolve(typeof(IGenericService<int>));
        Assert.NotNull(service);
        Assert.IsType<GenericService<int>>(service);
    }
    [Fact]
    public void Scope_Resolve_ShouldReturnService_WithIEnumerable()
    {
        var descriptors = new ServiceDescriptor[]
        {
            new TypeBasedServiceDescriptor()
            {
                ServiceType = typeof(ITestService),
                ImplementationType = typeof(TestService),
                Lifetime = Lifetime.Transient
            },
            new TypeBasedServiceDescriptor()
            {
            ServiceType = typeof(ITestService),
            ImplementationType = typeof(AnotherTestService),
            Lifetime = Lifetime.Transient
            }
    };
        _aBuilder.Setup(x => x.BuildActivation(It.Is<TypeBasedServiceDescriptor>(
                x => x.ImplementationType == typeof(TestService))))
                .Returns(_ => new TestService());
        _aBuilder.Setup(x => x.BuildActivation(It.Is<TypeBasedServiceDescriptor>(
                x => x.ImplementationType == typeof(AnotherTestService))))
            .Returns(_ => new AnotherTestService());
        var container = new Container(descriptors, _aBuilder.Object);
        var scope = container.CreateScope();
        var services = scope.Resolve(typeof(IEnumerable<ITestService>));
        var collection = (ICollection<object>)services;
        Assert.NotNull(services);
        Assert.IsAssignableFrom<IEnumerable>(services);
        Assert.Equal(2, collection.Count);
        Assert.All(collection, x =>
            Assert.IsAssignableFrom<ITestService>(x));

    }
    [Fact]
    public void Scope_Resolve_ShouldReturnMultipleServices_WithGenericAndIEnumerable()
    {
        var descriptors = new ServiceDescriptor[]
        {
            new TypeBasedServiceDescriptor()
            {
                ServiceType = typeof(IGenericService<>),
                ImplementationType = typeof(GenericService<>),
                Lifetime = Lifetime.Transient
            },
            new TypeBasedServiceDescriptor()
            {
                ServiceType = typeof(IGenericService<>),
                ImplementationType = typeof(AnotherGenericService<>),
                Lifetime = Lifetime.Transient
            }
        };
        _aBuilder.Setup(x => x.BuildActivation(It.Is<TypeBasedServiceDescriptor>(
                x => x.ImplementationType == typeof(GenericService<int>))))
            .Returns(_ => new GenericService<int>());
        _aBuilder.Setup(x => x.BuildActivation(It.Is<TypeBasedServiceDescriptor>(
                x => x.ImplementationType == typeof(AnotherGenericService<int>))))
            .Returns(_ => new AnotherGenericService<int>());
        var container = new Container(descriptors, _aBuilder.Object);
        var scope = container.CreateScope();
        var services = scope.Resolve(typeof(IEnumerable<IGenericService<int>>));
        var collection = (ICollection<object>)services;
        Assert.NotNull(services);
        Assert.IsAssignableFrom<IEnumerable>(services);
        Assert.Equal(2, collection.Count);
        Assert.All(collection, x =>
            Assert.IsAssignableFrom<IGenericService<int>>(x));
    }
    [Fact]
    public void Scope_Resolve_ShouldReturnTwoDifferentTransientServices_FromOneScope()
    {
        var descriptor = new TypeBasedServiceDescriptor()
        {
            ServiceType = typeof(ITestService),
            ImplementationType = typeof(TestService),
            Lifetime = Lifetime.Transient
        };
        _aBuilder.Setup(x => x.BuildActivation(descriptor))
            .Returns(_ => new TestService());
        var container = new Container(new[] { descriptor }, _aBuilder.Object);
        var scope = container.CreateScope();
        var service1 = scope.Resolve(typeof(ITestService));
        var service2 = scope.Resolve(typeof(ITestService));
        Assert.NotEqual(service1, service2);
    }
    [Fact]
    public void Scope_Resolve_ShouldReturnTwoSameScopedServices_FromOneScope()
    {
        var descriptor = new TypeBasedServiceDescriptor()
        {
            ServiceType = typeof(ITestService),
            ImplementationType = typeof(TestService),
            Lifetime = Lifetime.Scoped
        };
        _aBuilder.Setup(x => x.BuildActivation(descriptor))
            .Returns(_ => new TestService());
        var container = new Container(new[] { descriptor }, _aBuilder.Object);
        var scope = container.CreateScope();
        var service1 = scope.Resolve(typeof(ITestService));
        var service2 = scope.Resolve(typeof(ITestService));
        Assert.Equal(service1, service2);
    }
    [Fact]
    public void Scope_Resolve_ShouldReturnTwoDifferentScopedServices_FromTwoScopes()
    {
        var descriptor = new TypeBasedServiceDescriptor()
        {
            ServiceType = typeof(ITestService),
            ImplementationType = typeof(TestService),
            Lifetime = Lifetime.Scoped
        };
        _aBuilder.Setup(x => x.BuildActivation(descriptor))
            .Returns(_ => new TestService());
        var container = new Container(new[] { descriptor }, _aBuilder.Object);
        var scope1 = container.CreateScope();
        var scope2 = container.CreateScope();
        var service1 = scope1.Resolve(typeof(ITestService));
        var service2 = scope2.Resolve(typeof(ITestService));
        Assert.NotEqual(service1, service2);
    }
    [Fact]
    public void Scope_Resolve_ShouldReturnTwoSameSingletonServices_FromTwoScopes()
    {
        var descriptor = new TypeBasedServiceDescriptor()
        {
            ServiceType = typeof(ITestService),
            ImplementationType = typeof(TestService),
            Lifetime = Lifetime.Singleton
        };
        _aBuilder.Setup(x => x.BuildActivation(descriptor))
            .Returns(_ => new TestService());
        var container = new Container(new[] { descriptor }, _aBuilder.Object);
        var scope1 = container.CreateScope();
        var scope2 = container.CreateScope();
        var service1 = scope1.Resolve(typeof(ITestService));
        var service2 = scope2.Resolve(typeof(ITestService));
        Assert.Equal(service1, service2);
    }
}