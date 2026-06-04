using McMaster.Extensions.CommandLineUtils;
using Spectre.Console;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Xml;

namespace Lefty.Schematron.Cli;

/// <summary />
[Command( "format", Description = "Formats an XML file" )]
public class FormatCommand
{
    /// <summary />
    public FormatCommand()
    {
    }


    /// <summary />
    [Argument( 0, Description = "Input file" )]
    [Required]
    [FileExists]
    public string? InputFile { get; set; }

    /// <summary />
    [Option( "-c|--remove-comments", CommandOptionType.NoValue, Description = "When set, removes all comments" )]
    public bool RemoveComments { get; set; }

    /// <summary />
    [Option( "-n|--indent-count", CommandOptionType.SingleValue, Description = "Indent count" )]
    public int IndentCount { get; set; } = 2;

    /// <summary />
    [Option( "-o|--output-file", CommandOptionType.SingleValue, Description = "Output file" )]
    public string? OutputFile { get; set; }


    /// <summary />
    public int OnExecute()
    {
        /*
         * Load document
         */
        var doc = new XmlDocument();
        doc.Load( this.InputFile! );


        /*
         * Decline to format if digitally signed, since it risks breaking
         * the signature.
         */
        if ( doc.SelectSingleNode( " //ds:Signature ", Ns.Manager ) != null )
        {
            AnsiConsole.MarkupLine( "[red]err[/]: decline to format XML which is digitally signed" );
            return 2;
        }


        /*
         * Remove comments
         */
        if ( this.RemoveComments == true )
        {
            while ( true )
            {
                var c = (XmlComment?) doc.SelectSingleNode( " //comment() " );

                if ( c == null )
                    break;

                c.ParentNode!.RemoveChild( c );
            }
        }


        /*
         * 
         */
        Stream ostream;

        if ( this.OutputFile == null )
            ostream = Console.OpenStandardOutput();
        else
            ostream = File.Create( this.OutputFile );

        var xw = XmlWriter.Create( ostream, new XmlWriterSettings()
        {
            CloseOutput = true,
            Indent = true,
            IndentChars = new string( ' ', this.IndentCount ),
            NewLineChars = "\n",
            Encoding = Encoding.UTF8,
        } );

        doc.Save( xw );


        /*
         * 
         */
        if ( this.OutputFile != null )
            AnsiConsole.MarkupLineInterpolated( $"[green]ok[/]: wrote formatted file {this.OutputFile}" );

        return 0;
    }
}