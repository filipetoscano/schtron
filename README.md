schtron
===============================================================================

[![CI](https://github.com/filipetoscano/schtron/workflows/CI/badge.svg)](https://github.com/filipetoscano/schtron/actions)
[![NuGet](https://img.shields.io/nuget/vpre/Lefty.Schematron.Cli.svg?label=NuGet)](https://www.nuget.org/packages/Lefty.Schematron.Cli/)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)

Tool/library to convert Schematron into XSLT.


dotnet Tool
-------------------------------------------------------------------------------

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

Once installed, it becomes available as the `schtron` tool:

```bash
> schtron -?
1.0.0+5ef757699522f071d996a118efe9a146e5424d5f

Schematron

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

Run 'schtron [command] -?|-h|--help' for more information about a command
```


Running locally
-------------------------------------------------------------------------------

```
cd tools/Lefty.Schematron.Cli
dotnet restore
dotnet build
```

Run the `transform` command as follows:

```
dotnet run -- transform input.sch --format=xslt2 --output-file=output2.xslt
dotnet run -- transform input.sch --format=xslt3 --output-file=output3.xslt
```

Run the `eval` command as follows:

```
dotnet run -- eval document.xml output2.xslt
dotnet run -- eval document.xml output3.xslt
```


Licensing
-------------------------------------------------------------------------------

The present software is available under [MIT](https://opensource.org/licenses/MIT)/

This software would not be possible without the following two libraries:

* [schxslt1](https://codeberg.org/SchXslt/schxslt) is Copyright (c) David Maus (MIT License) 
* [schxslt2](https://codeberg.org/SchXslt/schxslt2) is Copyright (c) David Maus (MIT License)

The source code for the above XSL transforms are bundled into the 
`Lefty.Schematron` assembly as embedded resources.
