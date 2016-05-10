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



## Property options
