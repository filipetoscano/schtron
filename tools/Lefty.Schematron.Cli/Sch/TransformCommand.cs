using McMaster.Extensions.CommandLineUtils;
using Spectre.Console;
using System.ComponentModel.DataAnnotations;

namespace Lefty.Schematron.Cli.Sch;

/// <summary />
[Command( "transform", Description = "Transforms a Schematron file to XSL v2/v3 transforms" )]
public class TransformCommand
{
    private readonly ISchematronService _ss;


    /// <summary />
    public TransformCommand( ISchematronService ss )
    {
        _ss = ss;
    }


    /// <summary />
    [Argument( 0, Description = "Input schematron file" )]
    [Required]
    [FileExists]
    public string? InputFile { get; set; }

    /// <summary />
    [Option( "-f|--format", CommandOptionType.SingleValue, Description = "Format (Xslt2, Xslt3)" )]
    public OutputFormat OutputFormat { get; set; } = OutputFormat.Xslt3;

    /// <summary />
    [Option( "-o|--output-file", CommandOptionType.SingleValue, Description = "Output file" )]
    public string? OutputFile { get; set; }


    /// <summary />
    public int OnExecute( CommandLineApplication app )
    {
        /*
         * 
         */
        using var input = File.OpenRead( this.InputFile! );


        /*
         * Stdout is left alone: it isn't ours to close.
         */
        if ( this.OutputFile == null )
        {
            _ss.Transform( input, Console.OpenStandardOutput(), this.OutputFormat );

            return 0;
        }


        /*
         * Written beside the target and moved into place only once the transform
         * has succeeded. File.Create truncates on open, so transforming straight
         * into the output would destroy whatever was there before and leave an
         * empty file behind for the next step to trip over.
         *
         * Transform leaves the stream open, so the handle is closed here -- before
         * announcing the file, so it is complete and released by the time the
         * caller is told about it.
         */
        var temp = this.OutputFile + ".tmp";

        try
        {
            using ( var output = File.Create( temp ) )
            {
                _ss.Transform( input, output, this.OutputFormat );
            }

            File.Move( temp, this.OutputFile, overwrite: true );
        }
        catch
        {
            Discard( temp );
            throw;
        }

        AnsiConsole.MarkupLineInterpolated( $"[green]ok[/]: generated {this.OutputFile}" );

        return 0;
    }


    /// <summary>
    /// Removes the half-written temporary, without letting a failure to do so
    /// displace the error which caused it.
    /// </summary>
    private static void Discard( string file )
    {
        try
        {
            File.Delete( file );
        }
        catch ( IOException )
        {
        }
        catch ( UnauthorizedAccessException )
        {
        }
    }
}