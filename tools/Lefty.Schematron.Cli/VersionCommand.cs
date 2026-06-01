using McMaster.Extensions.CommandLineUtils;

namespace Lefty.Schematron.Cli;

/// <summary />
[Command( "version", Description = "Emits version information of embedded libraries" )]
public class VersionCommand
{
    /// <summary />
    public VersionCommand()
    {
    }


    /// <summary />
    public int OnExecute()
    {
        Console.WriteLine( "Schxslt1 = {0} (for XSLT2)", DepVersions.Schxslt1 );
        Console.WriteLine( "Schxslt2 = {0} (for XSLT3)", DepVersions.Schxslt2 );

        return 0;
    }
}