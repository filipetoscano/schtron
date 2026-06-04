schtron
==========================================================================

[![CI](https://github.com/filipetoscano/schtron/workflows/CI/badge.svg)](https://github.com/filipetoscano/schtron/actions)
[![NuGet](https://img.shields.io/nuget/vpre/Lefty.Schematron.Cli.svg?label=NuGet)](https://www.nuget.org/packages/Lefty.Schematron.Cli/)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)

Swiss-knife for Schematron operations, using .NET.


Running
--------------------------------------------------------------------------

```
> schtron
Swiss-knife for Schematron operations

Usage: schtron [command] [options]

Options:
  --version     Show version information.
  -?|-h|--help  Show help information.

Commands:
  eval          Evaluates an XML file using XSL transform
  format        Formats an XML file
  pfx           Creates a self-signed PFX file
  sign          Signs a Schematron file
  transform     Transforms a Schematron file to XSL v2/v3 transforms
  validate      Validates a Schematron file
  verify        Verifies digital signature on a Schematron file
  version       Emits version information of embedded libraries

Run 'schtron [command] -?|-h|--help' for more information about a command.
```


Pre-requisites
--------------------------------------------------------------------------

* [.NET 10](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)


Installing
--------------------------------------------------------------------------

To install the tool as a repository tool, run the following command in
the root of your repository:

```bash
> dotnet tool install --local Lefty.Schematron.Cli --create-manifest-if-needed
```

If you'd rather have the tool installed globally in your machine, run
the following command:

```bash
> dotnet tool install --global Lefty.Schematron.Cli
```


Licensing
-------------------------------------------------------------------------------

The present tool is available under [MIT](https://opensource.org/licenses/MIT)/

This software would not be possible without the following two libraries:

* [schxslt1](https://codeberg.org/SchXslt/schxslt) is Copyright (c) David Maus (MIT License) 
* [schxslt2](https://codeberg.org/SchXslt/schxslt2) is Copyright (c) David Maus (MIT License)

The source code for the above XSL transforms are bundled into the 
`Lefty.Schematron` assembly as embedded resources.
