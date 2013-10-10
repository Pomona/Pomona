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
    ]
}
```
Operators:
* `!` - Replaces a property (object or array).
* `-` - Removes an identified item from an array. Invalid on objects.
* `*` - Modifies a property (object or array).
* `+` - Adds an item to an array. Invalid on objects. Also the default operation on arrays, making it optional.
* `@` - Identification prefix used to indicate that the property name that follows and its assigned value is the key and value that identifies the item in the array that should be performed an action on.

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

Remove Joe from the `people` array:
```json
{
    "-people" : [{ "@id" : 1 }]
}
```

Change Peter in the `people` array:
```json
{
    "*people" : [{ "@id" : 2, "name" : "Peter Pan" }]
}
```

Another way to change Peter in the `people` array (before renaming him to "Peter Pan" as per the above example):
```json
{
    "*people" : [{ "@name" : "Peter", "name" : "Peter Pan" }]
}
```

Add Nancy to the `people` array:
```json
{
    "+people" : [{ "name" : "Nancy" }]
}
```

As adding is the default operation for arrays, the following will also add Nancy to the `people` array:
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

Changing the color of the pet Wendy from black to red and adding the mouse Kipper:
```json
{
    "*people" : [
        {
            "@id" : 1,
            "*pets" : [
                { "@name" : "Wendy", "color" : "Red" }
            ],
            "+pets" : [
                { "race" : "Mouse", "name" : "Kipper", "color" : "Gray" }
            ]
        }
    ]
}
```
