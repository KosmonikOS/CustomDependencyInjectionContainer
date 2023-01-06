<div class="markdown prose w-full break-words dark:prose-invert dark">

# Custom Dependency Injection Container

This is a custom Dependency Injection Container written using .NET 6.

## Features

*   Lightweight and easy to use
*   Supports both reflection-based and lambda-based activation builders

## Activation Builders

The container supports two types of activation builders:

<table>

<thead>

<tr>

<th>Activation Builder</th>

<th>Description</th>

</tr>

</thead>

<tbody>

<tr>

<td>ReflectionBasedActivationBuilder</td>

<td>Uses reflection to resolve services.</td>

</tr>

<tr>

<td>LambdaBasedActivationBuilder</td>

<td>Uses LINQ Lambda Expressions to resolve services. This is the default activation builder and can be faster than reflection in certain cases.</td>

</tr>

</tbody>

</table>

## Usage

<pre>
<div class="p-4 overflow-y-auto">
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
</div>
</div>
</pre>

## Note

This is a personal project and is not intended for production use. It is provided as an example of a custom dependency inject container and is not actively maintained.

</div>
