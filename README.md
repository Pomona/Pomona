![Pōmōna](http://pomona.io/content/images/pomona-icon-210.png)

# Pomona

Pomona is a framework built for exposing a domain model in a RESTful and hypermedia-driven manner.

It embraces the concept of convention over configuration, and provides opiniated defaults on how
domain model objects is exposed as HTTP resources.

Pomona was born out of frustrations with the difficulties of exposing a complex business domain model
as a RESTful web service.

## Build status
||Windows (AppVeyor)|Linux (Travis)|
|:--:|:--:|:--:|
|**develop**|[![Build Status](https://ci.appveyor.com/api/projects/status/vj3cw49n499u6046/branch/develop?svg=true)](https://ci.appveyor.com/project/Pomona/pomona/branch/develop)|[![Build Status](https://travis-ci.org/Pomona/Pomona.svg?branch=develop)](https://travis-ci.org/Pomona/Pomona)|
|**master**|[![Build Status](https://ci.appveyor.com/api/projects/status/vj3cw49n499u6046/branch/master?svg=true)](https://ci.appveyor.com/project/Pomona/pomona/branch/master)|[![Build Status](https://travis-ci.org/Pomona/Pomona.svg?branch=master)](https://travis-ci.org/Pomona/Pomona)|
|**future**|[![Build Status](https://ci.appveyor.com/api/projects/status/vj3cw49n499u6046/branch/future?svg=true)](https://ci.appveyor.com/project/Pomona/pomona/branch/future)|[![Build Status](https://travis-ci.org/Pomona/Pomona.svg?branch=future)](https://travis-ci.org/Pomona/Pomona)|


## Highlights

* Customizable conventions
* Fluent style overriding for exceptions to rules
* Autogenerated .NET client
* Advanced query filtering
* Simple LINQ provider

## Documentation

To get started with Pomona [please read the documentation](http://pomona.io).

## Contributing

Do you want to contribute to Pomona? Lovely! Contributions of any kind are
**highly** appreciated! Just make sure you've read and agreed to the
[contribution guidelines](https://github.com/Pomona/Pomona/blob/develop/CONTRIBUTING.md)
before submitting a pull request.

## Project links

* [Homepage](http://pomona.io)
* [GitHub page](https://github.com/okb/pomona)
* [Bug reporting](https://github.com/okb/pomona/issues)
* [![Join the chat at https://gitter.im/Pomona/Pomona](https://badges.gitter.im/Pomona/Pomona.svg)](https://gitter.im/Pomona/Pomona?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

## Aknowledgements

Thanks to

* [JSON.NET](ttp://james.newtonking.com/projects/json-net.aspx) for serialization stuff. h
* [Nancy](http://nancyfx.org/) for hosting the web service.
  I really love Nancy! I can't overstate how good I think it is! &lt;3 &lt;3 &lt;3
  One day I hope Pōmōna will offer a Super-Duper-Happy path just like it.
* [NUnit](http://www.nunit.org/) for testing.
* [Cecil](http://www.mono-project.com/Cecil) for IL generation in client

## Contributions

Pomona has mostly been built by @BeeWarloc (Karsten N. Strand) and @asbjornu (Asbjørn Ulsberg).

A significant portion of the development is funded by OKB AS.
