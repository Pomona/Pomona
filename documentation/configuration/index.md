<!--Title:Configuration and bootstrapping-->
<!--Url:configuration-->

Pomona has adopted the philosophy of "convention over configuration" using a
two-layered system consisting of

* <[linkto:configuration/rules;title=The fluent rule layer]>, for per-type and property overrides
* <[linkto:configuration/convention;title=The convention layer]>, for global rules

The convention layer should be used to define rules that is common to all your
resources, and the fluent layer for exceptions to these rules.
