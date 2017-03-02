<!--Title:Queries-->
<!--Url:queries-->

Resource collections exposed from an `IQueryable<T>` source can be queried
using the a standard set of query parameters.

## Query parameters

* `$filter`: predicate expression
* `$groupby`: group by expression
* `$select`: selector expression(s)
* `$orderby`: ordering expression(s)
* `$skip`: number of items to skip
* `$top`: number of items to return
* `$projection`: projection, can be one of the following
* `$expand`: property paths to expand

### $filter

The `$filter` query parameter can be used to filter the results. The filter
is applied before `groupby` and `$select`.

```git
$ curl 'http://localhost:1337/game-consoles?$filter=name%20eq%20%27Game%20Boy%27' -H "Accept: application/json"
{
  "_type": "__result__",
  "totalCount": -1,
  "items": [
    {
      "_uri": "http://localhost:1337/game-consoles/gameboy",
      "id": "gameboy",
      "name": "Game Boy"
    }
  ],
  "skip": 0,
  "previous": null,
  "next": null
}
```

### $select

The `$select` query parameter can be used for limiting the amount of data to retrieve,
or for applying simple transformations.

It accepts a comma separated list of expressions and names, of the pattern

`expr1 as name1,expr2 as name2`

The `as name` part is optional if the expression is a simple property reference, in which
case the expression property name will be used as the name.

The results will take the form of JSON objects with properties corresponding to the list
of expressions in `$select`.

```git
$ curl 'http://localhost:1337/game-consoles?$select=id,toupper(name)%20as%20n' -H "Accept: application/json"
{
  "_type": "__result__",
  "totalCount": -1,
  "items": [
    {
      "id": "gameboy",
      "n": "GAME BOY"
    },
    {
      "id": "nes",
      "n": "NINTENDO ENTERTAINMENT SYSTEM"
    }
  ],
  "skip": 0,
  "previous": null,
  "next": null
}
```

When selecting just one value we can avoid wrapping it in an object by using the
special form `expr as this`.

```git
$ curl 'http://localhost:1337/game-consoles?$select=toupper(name)%20as%20this' -H "Accept: application/json"
{
  "_type": "__result__",
  "totalCount": -1,
  "items": [
    "ATARI 2600",
    "GAME BOY",
    "NINTENDO ENTERTAINMENT SYSTEM"
  ],
  "skip": 0,
  "previous": null,
  "next": null
}
```

### $projection

The `$projection` query parameter controls what projection will be applied
to the query results.

* `first`: First resource of result set, 404 if not found
* `first`: Last resource of result set, 404 if not found
* `single`: First and only resource of result set. 404 if not found, 400 if several found
* `firstordefault`: First resource of result set, null
* `lastordefault`: First resource of result set, null
* `max`: Max value (only valid when `$select` expression returns a comparable value)
* `min`: Min value (only valid when `$select` expression returns a comparable value)
* `count`: Number of results
* `sum`: Sum of results (only valid when `$select` expression returns a summable value)

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
