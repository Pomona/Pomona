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
