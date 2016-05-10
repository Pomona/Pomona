<!--Title:Type rules-->
<!--Url:rules-->

The mapping configuration for a specific type can be defined as a method in
any class.

<[sample:misc-fluent-rules-example]>

Any object containing fluent rules must be returned from the `FluentRuleObjects`
property of your configuration class to be considered:

<[sample:misc-config-fluent-rule-objects-overrides]>

## Type options

Type options are defined through the `ITypeMappingConfigurator<T>` interface.

### Ignoring properties

A property can be ignored and hidden from the exposed resource by using the `Exclude` method.

<[sample:misc-exlude-property-fluent-rule]>

### Selecting handler

A handler class can be chosen for a resource type by using the `HandledBy<T>` method.

<[sample:misc-handled-by-fluent-rule]>

You can find out more about this in <[linkto:handlers]>

## Property options
