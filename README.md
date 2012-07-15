Goal: To create an easier way to map stuff to a REST API.

This is a few days' work, in other words a hard-coded thing for testing concepts.

Roadmap for first release
=========================
* Add tests for serialization and deserialization on client
* Create IPomonaDataSource, for retrieval of data
* Create PomonaSession and PomonaSessionFactory that will bind everything together
* Implement simple query mechanism (Linq? relinq? something simpler?)
  * Property equals something, look at how this is done
  * Make it possible for data source to implement its own query syntax

Future tasks
============
* Implement another layer of abstraction for serializing and deserializing, to support more types than Json. Will look at ServiceStack for this.
* Implement JS client lib. Maybe two types, one based on KnockoutJs? That would be cool.
* Implement html media type for friendly browseing.
* Implement batch fetching on client side
* Batch query support, for example by encapsulating an array of http operations in a JSON array


Bugs
====
* Uri's is now generated from native type names in nancy module, should rather get name from TransformedType

Cryptic notes about compressed JSON-like binary format
======================================================

CODE

STRING : LENGTH ARRAY[LENGTH]

NEW_STRING STRING
NEW_DOUBLE ARRAY[8]
NEW_VARINT VARINT

* Use indexed skiplist for compression?
