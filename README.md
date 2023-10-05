![Pōmōna](https://pomona.rest/content/images/pomona-icon-210.png)

# Pomona

Pomona is a framework built for exposing a domain model in a RESTful and
hypermedia-driven manner. It embraces the concept of convention over
configuration, and provides opiniated defaults on how domain model objects is
exposed as HTTP resources.

Pomona was born out of frustrations with the difficulties of exposing a complex
business domain model as a RESTful web service.

|                |        **master**         |       **develop**       |        **future**       |
| -------------: | :-----------------------: | :---------------------: | :---------------------: |
|     **GitHub** | [![GitHub release][1]][2] |            -            |             -           |
|      **NuGet** |      [![NuGet][3]][4]     |    [![NuGet][5]][6]     |             -           |
|     **Travis** |     [![Master][7]][8]     |  [![Develop][9]][10]    |   [![Future][11]][12]   |
|   **AppVeyor** |    [![Master][13]][14]    |  [![Develop][15]][16]   |   [![Future][17]][18]   |
| **Codefactor** |  [![Codefactor][19]][20]  | [![Codefactor][21]][22] | [![Codefactor][23]][24] |

## Table of Contents

1. [Overview](https://github.com/Pomona/Pomona#overview)
2. [Documentation](https://github.com/Pomona/Pomona#documentation)
3. [Contributing](https://github.com/Pomona/Pomona#contributing)
4. [Get in touch](https://github.com/Pomona/Pomona#get-in-touch)
5. [License](https://github.com/Pomona/Pomona#license)
5. [Acknowledgements](https://github.com/Pomona/Pomona#acknowledgements)

## Overview

To illustrate what Pomona does, here's a diagram:

![Pomona - Overview](https://cloud.githubusercontent.com/assets/12283/15649503/564fb484-2672-11e6-8ceb-6998e1c5d8f4.png)

1. Starting on the top left, in the **client**:
    1. A [Linq](https://msdn.microsoft.com/en-us/library/bb397926.aspx)
       statement is written inside a client application against a statically typed
       and auto-generated client library.
    2. The query is run through a [Linq Provider ](https://msdn.microsoft.com/en-us/library/bb546158.aspx)
   and translated to an HTTP query string.
2. Inside the **server**:
    1. The HTTP query string is received by the Pomona Server and parsed back into
       a [Linq Expression Tree](https://msdn.microsoft.com/en-us/library/mt654263.aspx).
    2. The Linq Expression is fed through a (custom) Linq Provider that can execute
       it against any back-end datastore supporting Linq as a query method.
    3. The Data is mapped from [Data Transfer Objects](https://en.wikipedia.org/wiki/Data_transfer_object),
       to database objects, back into DTOs and fed through a JSON serializer.
3. Back inside the **client**:
    1. The JSON is deserialized to statically typed DTOs.
    2. The DTOs are made available to the client with just the data requested
       with the initial `.Select()` statement.

This is in many ways similar to what [Falcor](https://netflix.github.io/falcor/)
does, only in a statically typed way tailored for .NET instead of a
`Promise`-based approach for JavaScript.

## Documentation

To get started with Pomona [please read the documentation](https://pomona.rest).

## Contributing

Do you want to contribute to Pomona? Lovely! Contributions of any kind are
**highly** appreciated! Just make sure you've read and agreed to the
[contribution guidelines](https://github.com/Pomona/Pomona/blob/develop/CONTRIBUTING.md)
before submitting a pull request.

## Get in touch

* [![Join the chat at https://gitter.im/Pomona/Pomona](https://badges.gitter.im/Pomona/Pomona.svg)](https://gitter.im/Pomona/Pomona?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
* [Homepage](https://pomona.rest)
* [GitHub page](https://github.com/Pomona/Pomona)
* [Bug reporting](https://github.com/Pomona/Pomona/issues)

## License

Copyright [Karsten N. Strand](https://github.com/BeeWarloc),
[Asbjørn Ulsberg](https://github.com/asbjornu),
[PayEx](https://github.com/PayEx) and contributors. Pomona is provided as-is
under the [MIT License](https://github.com/Pomona/Pomona/blob/develop/LICENSE).

## Acknowledgements

Thanks to:

* [JSON.NET](http://www.newtonsoft.com/json) for JSON serialization.
* [Nancy](http://nancyfx.org/) for providing a kick-ass HTTP server implementation.
* [NUnit](http://www.nunit.org/) for testing.
* [Cecil](http://www.mono-project.com/Cecil) for IL generation in client.
* [JetBrains](http://jetbrains.com/) for providing the Pomona project with an [Open Source License](https://www.jetbrains.com/support/community/#section=open-source) of [ReSharper](https://www.jetbrains.com/resharper/).


 [1]: https://img.shields.io/github/release/Pomona/Pomona.svg
 [2]: https://github.com/Pomona/Pomona/releases/latest
 [3]: https://img.shields.io/nuget/v/Pomona.svg
 [4]: https://www.nuget.org/packages/Pomona
 [5]: https://img.shields.io/nuget/vpre/Pomona.svg
 [6]: https://www.nuget.org/packages/Pomona
 [7]: https://travis-ci.org/Pomona/Pomona.svg?branch=master
 [8]: https://travis-ci.org/Pomona/Pomona
 [9]: https://travis-ci.org/Pomona/Pomona.svg?branch=develop
[10]: https://travis-ci.org/Pomona/Pomona
[11]: https://travis-ci.org/Pomona/Pomona.svg?branch=future
[12]: https://travis-ci.org/Pomona/Pomona
[13]: https://img.shields.io/appveyor/ci/Pomona/Pomona/master.svg
[14]: https://ci.appveyor.com/project/Pomona/Pomona/branch/master
[15]: https://img.shields.io/appveyor/ci/Pomona/Pomona/develop.svg
[16]: https://ci.appveyor.com/project/Pomona/Pomona/branch/develop
[17]: https://img.shields.io/appveyor/ci/Pomona/Pomona/future.svg
[18]: https://ci.appveyor.com/project/Pomona/Pomona/branch/future
[19]: https://www.codefactor.io/repository/github/pomona/pomona/badge/master
[20]: https://www.codefactor.io/repository/github/pomona/pomona/overview/master
[21]: https://www.codefactor.io/repository/github/pomona/pomona/badge/develop
[22]: https://www.codefactor.io/repository/github/pomona/pomona/overview/develop
[23]: https://www.codefactor.io/repository/github/pomona/pomona/badge/future
[24]: https://www.codefactor.io/repository/github/pomona/pomona/overview/future

