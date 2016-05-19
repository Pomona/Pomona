<!--Title:Contributors guide-->
<!--Url:internals-->

# Building Pomona

You can build Pomona using Visual Studio 2015 or Mono (4.2.3+).

Only standard .NET 4.5.1 assemblies need to be installed, all other
assemblies is included in the repository or will be restored by nuget.

# Design

Pomona was built from the bottom up. If some architectural decisions
might seem arbitrary, it's probably because they are.

## Architecture

<[img:content/diagrams/pomona_architecture.png;Pomona architecture diagram]>

## Request lifecycle

<[img:content/diagrams/request_code_path.png;Code path of request]>

## Routing

During early stages of Pomona development the intention was to let Nancy
handle the routing. Unfortunately we ran into some corner cases related to
child resource sets which was hard to fix.

As a result Pomona now has its own routing logic, inside the `Pomona.Routing`
namespace. The routes are organized as a tree with the `Route` class as the
base node type.

### Route node types

* `DataSourceRootRoute` - Default root `/` route, listing all root resources
* `DataSourceCollectionRoute` - Route to collection of a root resource type, like `/customers`
* `GetByIdRoute` - Route to an identified item in a collection, like `/customers/{id}`
* `ResourcePropertyRoute` - Route to a child resource collection bound to a property
   of another resource, like `/customers/{id}/contacts`

## Queries

### Expression parsing

`QueryExpressionParser` will orchestrate the conversion of an expression as
a string to a LINQ expression tree in the following way:

The lexing and parsing of an expression is performed using an ANTLR 3
compatible grammar named `PomonaQueryParser`. This grammar will create an initial
[abstract syntax tree](https://en.wikipedia.org/wiki/Abstract_syntax_tree) .

The tree will then be transformed in `PomonaQueryTreeParser` to an intermediate form,
with more semantic information and error checking.

Finally `NodeTreeToExpressionConverter` will convert the intermediate tree to a LINQ
`Expression`.


# Ideas for future changes

## Root resource

For a long time the plan has been to abandon the `IPomonaDataSource` interface, and
instead rely on having a root resource with navigation properties to all root resources.
