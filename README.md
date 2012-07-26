Goal: To create an easier way to map stuff to a REST API.

This is a few days' work, in other words a hard-coded thing for testing concepts.

Roadmap for first release
=========================
* Add tests for serialization and deserialization on client
* Create IPomonaDataSource, for retrieval of data. DONE
* Create PomonaSession and PomonaSessionFactory that will bind everything together
* Write correct metadata for generated client dll (AssemblyInfo etc..)
* Implement support for value types (which is always expanded, and don't have URI). 70% DONE.
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



Bugs and necesarry improvements
===============================

* Uri's is now generated from native type names in nancy module, should rather get name from TransformedType

* Expand path doesnt work properly on types with merged URIs. For example if Critter #125 is a MusicalCritter,
  http://localhost:2211/critter/125?expand=critter.weapons is expected to expand weapons.
  It doesn't, instead this is needed: http://localhost:2211/critter/125?expand=musicalcritter.weapons,
  which is not the way we want it to work!

* IPomonaDataSource needs to be improved. Must be passed some sort of query..
  Maybe also remove generics stuff here? Don't know if there's really any advantage to having generics..
  Maybe there should be multiple IPomonaDataSource (like repositories).

Cryptic notes about compressed JSON-like binary format
======================================================

CODE

STRING : LENGTH ARRAY[LENGTH]

NEW_STRING STRING
NEW_DOUBLE ARRAY[8]
NEW_VARINT VARINT

* Use indexed skiplist for compression?
