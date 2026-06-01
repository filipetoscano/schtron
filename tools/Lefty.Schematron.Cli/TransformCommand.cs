using McMaster.Extensions.CommandLineUtils;
using Spectre.Console;
using System.ComponentModel.DataAnnotations;

namespace Lefty.Schematron.Cli;

/// <summary />
[Command( "transform", Description = "Transforms a Schematron file to XSL v2/v3 transforms" )]
public class TransformCommand
{
    /// <summary />
    public TransformCommand()
    {
    }


    /// <summary />
    [Argument( 0, Description = "Input schematron file" )]
    [Required]
    [FileExists]
    public string? InputFile { get; set; }

    /// <summary />
    [Option( "-f|--format", CommandOptionType.SingleValue, Description = "Format (Xslt2, Xslt3)" )]
    public OutputFormat OutputFormat { get; set; } = OutputFormat.Xslt2;

    /// <summary />
    [Option( "-o|--output-file", CommandOptionType.SingleValue, Description = "Output file" )]
    public string? OutputFile { get; set; }


    /// <summary />
    public int OnExecute( CommandLineApplication app )
    {
        /*
         * 
         */
        var input = File.OpenRead( this.InputFile! );


        /*
         * 
         */
        Stream output;

        if ( this.OutputFile == null )
            output = Console.OpenStandardOutput();
        else
            output = File.Create( this.OutputFile );


        /*
         * 
         */
        var ss = new SchematronService();
        ss.Transform( input, output, this.OutputFormat );

        if ( this.OutputFile != null )
            AnsiConsole.MarkupLineInterpolated( $"[green]ok[/]: generated {this.OutputFile}" );

        return 0;
    }
}