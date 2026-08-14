using Lefty.Schematron.Cli.Xml;
using McMaster.Extensions.CommandLineUtils;

namespace Lefty.Schematron.Cli;

/// <summary />
[Command( "xml", Description = "XML commands" )]
[Subcommand( typeof( EvaluateCommand ) )]
[Subcommand( typeof( FormatCommand ) )]
[Subcommand( typeof( ValidateCommand ) )]
public class XmlCommand
{
    /// <summary />
    public int OnExecute( CommandLineApplication app )
    {
        app.ShowHelp();
        return 1;
    }
}