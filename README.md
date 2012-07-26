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

* Special handling for nullable types in JSON schema generation needs to be added.

* IPomonaDataSource needs to be improved. Must be passed some sort of query..
  Maybe also remove generics stuff here? Don't know if there's really any advantage to having generics..
  Maybe there should be multiple IPomonaDataSource (like repositories).

Brainstorm area
===============

* Could make query mechanism pluggable, through some sort of IHttpQueryTranslator.
  Then we could provide a simple default implementation.

* An exotic side-project could be to implement an IHttpQueryTranslator that uses relinq
  to serialize and convert LINQ expressions, which then can be executed on Nhibernate or other ORM.
  This does however seem a bit dangerous with regards to security.

Idea: Automated batching of queries to decrease N+1 performance problems
========================================================================

The classic N+1 problem will appear when looping through a list, where we at each step
access a reference to another object, which will then be loaded.

Although pomona supports sending a list of expanded paths to deal with these problems,
it would be sorta cool if this could be detected runtime and fixed.

This is one way to do it (by example):

We got two simple entity types:

Customer example:
    {
       name: "The first name",
       order: {
           _ref: "http://blah/order/1"
       }
    }

Order example:
    {
        _uri: "http://blah/order/1",
        description: "This is a order",
    }

This can be accomplished by keeping track of what properties have been accessed, and in what order.

Lets say we get a list of 30 customers by some criteria that is supposed to be presented
in a table along with the order description.. And we iterate through the customers in order.

Total query count: 1

customers[0].Order.Description is accessed first, which, as is normal, makes the first order loaded.
We take a note that Order of description has been loaded for first Customer, and double the prefetch
count for the path Customer.Order, from 1 (no prefetching) to 2.

Total query count: 2

customers[1].Order.Description is accessed next time, which loads Order for both customer #1 and customer #2.
Then prefetch count is doubled again from 2 to 4.

customers[2].Order is already prefetched, so nothing needs to be loaded for that.

Total query count: 3

The customers[3].Order is accessed, which loads Order for customer #3,#4,#5,#6. The prefetch count is
doubled again.

Total query count: 4

And so on..:

Query 5: Order #7, #8, #9, #10, #11, #12, #13, #14 loaded
Query 6: Order #15, #16, #17, #18, #19, #20, #21, #22, #23, #24 loaded

This gives a total of 6 http requests instead of 26, which means instead of N+1 we have Log2(N)+1 operations.

Cryptic notes about compressed JSON-like binary format
======================================================

CODE

STRING : LENGTH ARRAY[LENGTH]

NEW_STRING STRING
NEW_DOUBLE ARRAY[8]
NEW_VARINT VARINT

* Use indexed skiplist for compression?
