<!--Title:Resource handlers-->
<!--Url:handlers-->

# What's a handler?

In Pomona a handler is something that handles a HTTP request. It's basically
the same as what is called a "controller" in many other web frameworks.

We can define one or more handler types per method type, either by using the
`HandledBy<T>(..)` method or by overriding the `GetResourceHandlers` convention.

Methods in the handler class are duck-typed, and resolved using their name
and signature.

A handler is instantiated using the IoC container associated with the processed
`NancyContext`.

# Types of handler methods

## Query handlers

The Query handlers are used for GET requests to resource collections. They are required
to return an `IQueryable<T>`.

They must follow the naming pattern `Get{Resources}` or `Query{Resources}`, where
`{Resources}` must be the plural name of the resource type. Or, if the handler class
is only used for one type, it can simply be called `Query`.

<[sample:dusty-game-console-handler]>

Here a GET request to /game-consoles would be routed to the `Query()` method of the
handler type.

## Get single resource by id

A method named `Get{Resource}` or `Get` that takes the id as its first argument and
returns a resource of the correct type will handle requests for resources by id.

Note that by default this method is implemented by using the query handler,
so in many cases manually implementing this method is redundant.

<[sample:misc-get-customer-handler-method]>
