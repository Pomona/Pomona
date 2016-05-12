<!--Title:Using exposed service-->
<!--Url:rest-->

Pomona exposes your web service using a JSON format.

# Links

Links to other resources are wrapped inside a JSON object,
which makes it possibly to distinguish them from other JSON strings.

They can also contain a type hint for the referenced resource.

```javascript
{
  "_type": "User",
  "_ref": "http://some-service/users/123"
}
```
