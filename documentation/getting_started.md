<!--Title:Getting started-->
<!--Url:getting_started-->

Here we will guide you through the steps needed to create a very basic Pomona
service.

This example is also available in the Pomona repository in the
`samples/Pomona.Samples.DustyBoxes1` folder of the
[Pomona source](https://github.com/Pomona/Pomona).

## Installing

We start by creating a new C# console project and install the [Pomona NuGet package]
(http://www.nuget.org/packages/Pomona/) from the standard NuGet feed.

```
PM> Install-Package Pomona
```

*Note: We make a command line project so we can run this as a self-hosted Nancy host. In a real life example we would probably use another type of project.*

### The domain model

Now that we have a empty project we need some domain model to expose.

<[sample:dusty-game-console]>

*Every resource should have some id, which will be used as part of the URL.*

### Setting up a module

Then we need to create a module,

<[sample:dusty-module]>

..and also a corresponding configuration class.

<[sample:dusty-configuration]>

The configuration above have two properties overridden.

* `FluentRuleObjects` returns a list containing one rule object `DustyRules`,
  which will be scanned for mapping rules.
* `SourceTypes` returns a list of the domain objects to be mapped.

### Handling resource requests

In the `Map` method of the `DustyRules` we defined that `GameConsoleHandler` will
handle requests for the `GameConsole` resource.

Now we need to actually implement this handler class:

<[sample:dusty-game-console-handler]>

For exposing a read-only collection of resources we just need to define one method
returning an `IQueryable<GameConsole>`.

Usually you would proabably use a LINQ provider here, like the one in [NHibernate](http://nhibernate.info/) or [Entity Framework](http://www.asp.net/entity-framework). But for now we'll just use an in-memory array.

### Running the service

At this point our project should contain the following classes:

* `GameConsole` - Our domain object
* `GameConsoleHandler` - Handler for `GameConsole`
* `DustyConfiguration` - Configuration for Pomona module
* `DustyModule` - The Pomona module
* `Program` - The console app startup class

As mentioned earlier we will be using Nancy self host for this example. For this
the package [Nancy.Hosting.Self](http://www.nuget.org/packages/Nancy.Hosting.Self)
must be installed from the NuGet gallery.

```
PM> Install-Package Nancy.Hosting.Self
```

We also need to modify our `Progam.cs` file to actually bootstrap and fire up
a local http server.

<[sample:dusty-program]>

Now our example should be able to compile, run and be explored at [http://localhost:1337](http://localhost:1337).

```git
$ curl http://localhost:1337 -H "Accept: application/json"
{
  "game-consoles": "http://localhost:1337/game-consoles"
}
```

```git
C:\>curl http://localhost:1337/game-consoles -H "Accept: application/json"
{
  "_type": "__result__",
  "totalCount": -1,
  "items": [
    {
      "_uri": "http://localhost:1337/game-consoles/a2600",
      "id": "a2600",
      "name": "Atari 2600"
    },
    {
      "_uri": "http://localhost:1337/game-consoles/gameboy",
      "id": "gameboy",
      "name": "Game Boy"
    },
    {
      "_uri": "http://localhost:1337/game-consoles/nes",
      "id": "nes",
      "name": "Nintendo Entertainment System"
    }
  ],
  "skip": 0,
  "previous": null,
  "next": null
}
```

And there you have it, **congratulations**, you have just completed our little
tutorial. Read the following chapters of the documentation for further experimentation,
