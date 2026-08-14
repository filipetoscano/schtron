using Lefty.Schematron.Cli.Sch;
using McMaster.Extensions.CommandLineUtils;

namespace Lefty.Schematron.Cli;

/// <summary />
[Command( "sch", Description = "Schematron commands" )]
[Subcommand( typeof( SignCommand ) )]
[Subcommand( typeof( TransformCommand ) )]
[Subcommand( typeof( ValidateCommand ) )]
[Subcommand( typeof( VerifyCommand ) )]
public class SchematronCommand
{
    /// <summary />
    public int OnExecute( CommandLineApplication app )
    {
        app.ShowHelp();
        return 1;
    }
}
