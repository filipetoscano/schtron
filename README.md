schtron
===============================================================================

Tool/library to convert Schematron into XSLT.


Tool
-------------------------------------------------------------------------------

```
> schtron -?
1.0.0+5ef757699522f071d996a118efe9a146e5424d5f

Schematron

Usage: schtron [command] [options]

Options:
  --version     Show version information.
  -?|-h|--help  Show help information.

Commands:
  eval          Evaluates an XML file
  transform     Transforms a Schematron file to xslt
  validate      Validates a Schematron file

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

* [schxslt1](https://codeberg.org/SchXslt/schxslt) is Copyright (c) David Maus (MIT License) 
* [schxslt2](https://codeberg.org/dmaus/schxslt2/) is Copyright (c) David Maus (MIT License)
