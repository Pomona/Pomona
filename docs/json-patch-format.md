JSON PATCH Specification
====================================

Example document:
```json
{
    "info": { "foo": "fighter", "crow" : "bar" },
    "people" : [
	{ "id" : 1, "name" : "Joe" },
        { "id" : 2, "name" : "Peter" }
    ]
}
```

Changing 'info' (an object):
```json
{
    "*info" : { "foo" : "miauu" }
}
```

Changing members that are not arrays is the default operation, so this does the same thing:
```json
{
    "info" : { "foo" : "miauu" }
}
```

We want to replace the 'info' property:
```json
{
    "!info" : { "foo" : "unknown", "bar" : "hello" }
}
```

We want to remove Joe:
```json
{
    "-people" : [{ "@id" : 1 }]
}
```

We want to change Peter:
```json
{
    "*people" : [{ "@id" : 2, "name" : "Peter Pan" }]
}
```

We want to add nancy:
```json
{
    "+people" : [{ "name" : "Nancy" }]
}
```

Adding is the default operation for arrays, so the following will also add Nancy:
```json
{
    "people" : [{ "name" : "Nancy" }]
}
```

We want to replace the whole 'people' property:
```json
{
    "!people" : [{ "name" : "Peter Pan" }]
}
```
