<!--Title:Generated client-->
<!--Url:generated_client-->

Pomona can generate a .NET compatible client for our service if desired.

A client NuGet package can be generated and downloaded by accessing the
`/client.nupkg` url on the service.

Alternatively the DLL can be downloaded directly from `/{ClientAssembly}.dll`,
where `{ClientAssembly}` must be replaced with the configured name of the
client dll (see below for this).

## Controlling client generation

### Metadata

By overriding the `ClientMetadata` property of a `ITypeMappingFilter`,
we can change the assembly name, interface name, type name and namespace
of the client.

<[sample:client-metadata-override]>

### Merged assembly

By returning `true` from the `GenerateIndependentClient()` method of
`ITypeMappingFilter` we can generate an assembly with `Pomona.Common.dll`
merged.

Note that the resulting assembly will still have a dependency on
`Newtonsoft.Json`.

## LINQ support

As the `IQueryable` interface by nature is a leaky abstraction, supporting
all queries is not practical.

Common use cases should however be supported, although sometimes with a
bit of `Expression` acrobatics.

### Simple queries

Simple non-aggregate queries must be structured in a way described in
the example below:

<[sample:client-linq-simple-query-structure]>

### Group by queries

Aggregate queries must be structured be structured in a way described in
the example below:

<[sample:client-linq-aggregate-query-structure]>

### Supported expressions

A few of the supported methods are listed here

* Operators: + - / * && ||
* String
  - .Length
  - .StartsWith(string value)
  - .Contains(string value)
  - .ToLower()
  - .ToUpper()
* IEnumerable<T>
  - .Where(Func<,> predicate)
  - .Select(Func<,> selector)
  - .Sum()
  - .Count()

A more complete list can be found in `QueryFunctionMapping.cs` of the Pomona
source code.
