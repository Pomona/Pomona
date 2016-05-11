<!--Title:Changing conventions-->
<!--Url:convention-->

When starting up Pomona gets its mapping conventions from `ITypeMappingFilter`.

By default the `DefaultTypeMappingFilter` is used, which defines a set of sane defaults.

If you want to alter these conventions using `TypeMappingFilterBase` as a base class
is recommended, as implementing the whole `ITypeMappingFilter` interface would be a lot of work.

For some examples see code below:

<[sample:crazy-conventions]>

For further information see the `ITypeMappingFilter` interface definition. Most of the methods
should hopefully be self-explanatory.
