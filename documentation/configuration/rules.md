<!--Title:Fluent rules-->
<!--Url:rules-->

The mapping configuration for a specific type can be defined as a method in
any class.

<[sample:misc-fluent-rules-example]>

Any object containing fluent rules must be returned from the `FluentRuleObjects`
property of your configuration class to be considered:

Pomona will scan fluent rule objects and look for methods taking one argument
of type `ITypeMappingConfigurator<T>`. A rule methods will be applied to `T`
and all subtypes of `T`.

<[sample:misc-config-fluent-rule-objects-overrides]>

# Type options

Type options are defined through the `ITypeMappingConfigurator<T>` interface.

Rules specified using `ITypeMappingConfigurator<T>` can either be chained, or split
up in multiple declarations.

## Ignoring properties

A property can be ignored and hidden from the exposed resource by using the `Exclude` method.

<[sample:misc-exclude-property-fluent-rule]>

## Selecting handler

A handler class can be chosen for a resource type by using the `HandledBy<T>` method.

<[sample:misc-handled-by-fluent-rule]>

You can find out more about this in <[linkto:handlers]>

## Treating type as a value object

We can treat a complex type as a value object, as opposed to a resource.
A value object can be a part of resource or another value object, and has no URI.

<[sample:misc-value-object-fluent-rule]>

You can learn more about value types in <[linkto:type_system]>

## Custom construction of object

When Pomona needs to instantiate a resource or value object type during deserialization,
it uses the empty constructor by default if available.

If we want to instantiate the object using a non-empty constructor or a method taking parameters,
we can use the `ConstructedUsing` method.

<[sample:misc-constructed-using-fluent-rule]>

The `ConstructedUsing` method accepts an `Expression<Func<IConstructorControl<TDeclaringType>, TDeclaringType>>`
argument.

The `IConstructorControl<T>` parameter can then be used to bind properties with
any corresponding parameters.

## Constructor context

If we need to access some service while instantiating a type we can use the `Context<T>()`
method of `IConstructorControl<T>`.

<[sample:misc-contextful-construction-fluent-rule]>

## Specifying plural name of type

Pomona will try to derive the plural name of a type by using a few simple rules,
which works for a majority of English words.

This value is used to generate the URI for a root resource collection type.

<[sample:misc-plural-name-fluent-rule]>

# Property options

TODO!
