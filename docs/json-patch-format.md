JSON PATCH Specification
====================================

Example document:
```json
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
Property operators:
* `!` - Replaces a property (object or array). When setting to a value type this is always the default operation, making the `!` optional.
* `*` - Patches the existing object of a property (object or array).
* `-` - Removes a property.

Array operators:
* `-@` - Indicates that an object item is removed from array using specificied identifier.
* `*@` - Indicates that an object item is to be located in array using specified identifier, and then patched.
* Default operation is to add something to an array.

Escaping:
* Property names starting with `-`, `*`, `!`, `@` and `^` has to be escaped by putting `^` in front of it.

Changing the `info` property (an object):
```json
{
    "*info" : { "foo" : "miauu" }
}
```

As changing members that are not arrays is the default operation, this also changes the `info` property:
```json
{
    "info" : { "foo" : "miauu" }
}
```

Replace the whole `info` property:
```json
{
    "!info" : { "foo" : "unknown", "bar" : "hello" }
}
```

Removing the fish property from attributes:
```json
{
    "attributes" : { "-fish" }
}
```

Removing the fish property from attributes (property value is ignored):
```json
{
    "attributes" : { "-fish" : {} }
}
```

Replacing the strangely named "-MUST_BE_ESCAPED-" property from attributes requires escaping:
```json
{
    "attributes" : { "!^-MUST_BE_ESCAPED-" : "REPLACED!" }
}
```

Remove Joe from the `people` array:
```json
{
    "people" : [{ "-@id" : 1 }]
}
```

Change Peter in the `people` array:
```json
{
    "people" : [{ "*@id" : 2, "name" : "Peter Pan" }]
}
```

Another way to change Peter in the `people` array (before renaming him to "Peter Pan" as per the above example):
```json
{
    "people" : [{ "*@name" : "Peter", "name" : "Peter Pan" }]
}
```

Add Nancy to the `people` array:
```json
{
    "people" : [{ "name" : "Nancy" }]
}
```

Replace the whole `people` property:
```json
{
    "!people" : [{ "name" : "Peter Pan" }]
}
```

Changing the color of the pet Wendy from black to red, adding the mouse Kipper and deleting Karl:
```json
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
