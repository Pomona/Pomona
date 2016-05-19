<!--Title:Concurrency control using ETags-->
<!--Url:etags-->

Pomona supports optimistic concurrency control by using a combination of
[ETags](https://en.wikipedia.org/wiki/HTTP_ETag) and the `If-Match` header.

For this to work we need to specify what property to use as the ETag.

<[sample:etag-fluent-rule]>

Every time a `PATCH` request is made to this resource with the `If-Match`
header set to the previously seen `ETag` header it will compare the header value
with the current ETag value. If the ETag has changed Pomona will abort the
request with a `412` [Precondition Failed](https://tools.ietf.org/html/rfc7232#section-4.2)
status code.

When using a data layer with [optimistic concurrency](https://en.wikipedia.org/wiki/Optimistic_concurrency_control)
control we should also throw a `ResourcePreconditionFailedException` if a conflict is detected during update.
