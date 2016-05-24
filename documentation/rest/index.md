<!--Title:Using exposed service-->
<!--Url:rest-->


# JSON hypermedia format

Pomona exposes your web service using a JSON format that follows certain
conventions.

## Resource metadata

For a resource the following properties has a special, reserved meaning:

* `_uri`: The canonical URL of the resource
* `_type`: Type of the resource

```javascript
{
  "_uri": "http://some-service/users/123",
  "_type": "User",
  "name": "bob",
  "is_active": true
}
```

## Links

Links to other resources are wrapped inside a JSON object with a `_ref` property,
which makes it possible to detect them programmatically.

They can also contain a `type` hint for the referenced resource.

```javascript
{
  "_type": "User",
  "_ref": "http://some-service/users/123"
}
```

## Resource collections

Resource collections are presented as JSON objects with an `items` that holds the
contained resources in a JSON array.

Properties:
* `_type`: Will be set to `__result__`
* `totalCount`: Total count of all pages when `$totalcount` query parameter is set to `true`. Otherwise -1
* `skip`: Number of elements to skip, specified using the `$skip` query parameter
* `next`: Next page, `null` if current page is the last
* `previous`: Previous page, `null` if current page is the first

```javascript
{
  "_type": "__result__",
  "totalCount": -1,
  "items": [
    {
      "_uri": "http://localhost:1337/game-consoles/a2600",
      "id": "a2600",
      "name": "Atari 2600"
    },
    {
      "_uri": "http://localhost:1337/game-consoles/gameboy",
      "id": "gameboy",
      "name": "Game Boy"
    }
  ],
  "skip": 0,
  "previous": null,
  "next": "http://localhost:1337/game-consoles?$top=2&$skip=2"
}
```

## Boxed value types

Because JSON has a limited number of primitive types, some values will be
boxed.

```javascript
{
  "_type": "Int64",
  "value": 23
}
```

# Patching

Pomona uses a custom PATCH format, which feels intuitive and straight-forward
for simple cases, and that also can support more advanced scenarios.

### Property operators:

* `!` - Replaces a property (object or array). When setting to a value type this is always the default operation, making the `!` optional.
* `*` - Patches the existing object of a property (object or array).
* `-` - Removes a property.

### Array operators:

* `-@` - Indicates that an object item is removed from array using specificied identifier.
* `*@` - Indicates that an object item is to be located in array using specified identifier, and then patched.
* Default operation is to add something to an array.

### Escaping:

Property names starting with `-`, `*`, `!`, `@` and `^` has to be escaped by putting `^` in front of it.

### Examples

Original JSON document:

```javascript
{
    "info": { "foo": "fighter", "crow" : "bar" },
    "people" : [
        {
            "id" : 1,
            "name" : "Joe",
            "pets" : [
                { "race" : "Cat", "name" : "Wendy", "color" : "Black" },
                { "race" : "Dog", "name" : "Nana", "color" : "Brown" }
            ]
        },
        { "id" : 2, "name" : "Peter" }
    ],
    "attributes" : {
        "goat" : "eat",
        "fish" : "swim",
        "-MUST_BE_ESCAPED-" : "nada"
    }
}
```

Changing the `info` property (an object):

```javascript
{
    "*info" : { "foo" : "miauu" }
}
```

As changing members that are not arrays is the default operation, this also changes the `info` property:

```javascript
{
    "info" : { "foo" : "miauu" }
}
```

Replace the whole `info` property:

```javascript
{
    "!info" : { "foo" : "unknown", "bar" : "hello" }
}
```

Removing the fish property from attributes:
```javascript
{
    "attributes" : { "-fish" }
}
```

Removing the fish property from attributes (property value is ignored):
```javascript
{
    "attributes" : { "-fish" : {} }
}
```

Replacing the strangely named "-MUST_BE_ESCAPED-" property from attributes requires escaping:
```javascript
{
    "attributes" : { "!^-MUST_BE_ESCAPED-" : "REPLACED!" }
}
```

Remove Joe from the `people` array:
```javascript
{
    "people" : [{ "-@id" : 1 }]
}
```

Change Peter in the `people` array:
```javascript
{
    "people" : [{ "*@id" : 2, "name" : "Peter Pan" }]
}
```

Another way to change Peter in the `people` array (before renaming him to "Peter Pan" as per the above example):
```javascript
{
    "people" : [{ "*@name" : "Peter", "name" : "Peter Pan" }]
}
```

Add Nancy to the `people` array:
```javascript
{
    "people" : [{ "name" : "Nancy" }]
}
```

Replace the whole `people` property:
```javascript
{
    "!people" : [{ "name" : "Peter Pan" }]
}
```

Changing the color of the pet Wendy from black to red, adding the mouse Kipper and deleting Karl:
```javascript
{
    "*people" : [
        {
            "@id" : 1,
            "pets" : [
                { "race" : "Mouse", "name" : "Kipper", "color" : "Gray" },
                { "*@name" : "Wendy", "color" : "Red" },
                { "-@name" : "Karl" }
            ]
        }
    ]
}
```
