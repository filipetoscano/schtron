`Lefty.Schematron`
==========================================================================

[![CI](https://github.com/filipetoscano/schtron/workflows/CI/badge.svg)](https://github.com/filipetoscano/schtron/actions)
[![NuGet](https://img.shields.io/nuget/vpre/Lefty.Schematron.svg?label=NuGet)](https://www.nuget.org/packages/Lefty.Schematron/)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)

Swiss-knife for Schematron operations, using .NET.


Functionality
--------------------------------------------------------------------------

* Validate a Schematron file
* Transform a Schematron file to XSL v2 and v3 transformations
* Evaluate an XML file using XSL transforms


Installing via NuGet
--------------------------------------------------------------------------

Package is published in the [NuGet](https://www.nuget.org/packages/Lefty.Schematron/) gallery.

From the command-line:

```
> dotnet add package Lefty.Schematron
```

From within Visual Studio using Package Manager Console:

```
PM> Install-Package Lefty.Schematron
```


Getting started
--------------------------------------------------------------------------

In the startup of your application, configure the DI container as follows:

```csharp
using Lefty.Schematron;

builder.Services.AddTransient<ISchematronService,SchematronService>();
```

You can then use the injected `ISchematronService` instance.


Pre-requisites
--------------------------------------------------------------------------

* [.NET 10](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)


Licensing
-------------------------------------------------------------------------------

Library available under [MIT](https://opensource.org/licenses/MIT)/

This software would not be possible without the following two libraries:

* [schxslt1](https://codeberg.org/SchXslt/schxslt) is Copyright (c) David Maus (MIT License) 
* [schxslt2](https://codeberg.org/SchXslt/schxslt2) is Copyright (c) David Maus (MIT License)

The source code for the above XSL transforms are bundled into the assembly
as embedded resources.
