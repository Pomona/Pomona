<!--Title:Data source-->
<!--Url:datasource-->

As an alternative to creating handlers for each specific resource type it is also
possible to implement an `IPomonaDataSource`.

A data source will be used when registered for in the IoC container.

`IPomonaDataSource` has the methods `Patch<T>`, `Post<T>` and `Query<T>`, where `T` is the
resource type for the current request.

<[sample:simple-data-source]>
