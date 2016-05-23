<!--Title:Queries-->
<!--Url:queries-->

Resource collections exposed from an `IQueryable<T>` source can be queried
using the a standard set of query parameters.

## Query parameters

* `$filter`: predicate expression
* `$groupby`: group by expression
* `$select`: selector expression

## Operators

### Boolean
* `and`, `&&`: And (conditional)
* `or`, `||`: Or (conditional)

### Multiplicative
* `mul`, `*`: Multiply
* `div`, `/`: Divide
* `mod`, `%`: Modulus

### Additive
* `add`, `+`: Add
* `sub`, `-`: Subtract

### Relational
* `eq`, `==`: Equals
* `ne`, `!=`: Not equals
* `gt`, `>`: Greater than
* `lt`, `<`: Less than
* `ge`, `>=`: Greater than or equal
* `le`, `<=`: Less than or equal

## Grammar

For the complete grammar look at the ANTLR3 grammar file [PomonaQuery.q](https://raw.githubusercontent.com/Pomona/Pomona/master/app/Pomona/Queries/PomonaQuery.g).
