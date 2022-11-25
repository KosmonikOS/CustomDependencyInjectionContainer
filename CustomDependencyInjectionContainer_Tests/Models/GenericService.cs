namespace CustomDependencyInjectionContainer_Tests.Models;

public class GenericService<T> : IGenericService<T> { }
public class AnotherGenericService<T> : IGenericService<T> { }
public interface IGenericService<T> { }