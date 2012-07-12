Goal: To create an easier way to map stuff to a REST API.

This is a few days' work, in other words a hard-coded thing for testing concepts.

Roadmap for first release
=========================
* Add tests for serialization and deserialization on client
* Create IPomonaDataSource, for retrieval of data
* Create PomonaController(?Hub??) that will bind everything together
* Implement simple query mechanism (Linq? relinq? something simpler?)

Future tasks
============
* Implement another layer of abstraction for serializing and deserializing, to support more types than Json. Will look at ServiceStack for this.
* Implement JS client lib. Maybe two types, one based on KnockoutJs? That would be cool.
* Implement html media type for friendly browseing.
