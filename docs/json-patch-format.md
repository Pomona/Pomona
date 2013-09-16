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

Changing the `info` property (an object):
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

Replace the whole `info` property:
```json
{
    "!info" : { "foo" : "unknown", "bar" : "hello" }
}
```

Remove Joe:
```json
{
    "-people" : [{ "@id" : 1 }]
}
```

Change Peter:
```json
{
    "*people" : [{ "@id" : 2, "name" : "Peter Pan" }]
}
```

Add Nancy to `people`:
```json
{
    "+people" : [{ "name" : "Nancy" }]
}
```

As adding is the default operation for arrays, the following will also add Nancy:
```json
{
    "people" : [{ "name" : "Nancy" }]
}
```

Replace the whole 'people' property:
```json
{
    "!people" : [{ "name" : "Peter Pan" }]
}
```
