using McMaster.Extensions.CommandLineUtils;
using Spectre.Console;
using System.ComponentModel.DataAnnotations;

namespace Lefty.Schematron.Cli;

/// <summary />
[Command( "eval", Description = "Evaluates an XML file" )]
public class EvaluateCommand
{
    /// <summary />
    public EvaluateCommand()
    {
    }


    /// <summary />
    [Argument( 0, Description = "Input file" )]
    [Required]
    [FileExists]
    public string? InputFile { get; set; }

    /// <summary />
    [Argument( 1, Description = "Transform file" )]
    [Required]
    [FileExists]
    public string? TransformFile { get; set; }


    /// <summary />
    public int OnExecute()
    {
        /*
         * 
         */
        var input = File.OpenRead( this.InputFile! );
        var transform = File.OpenRead( this.TransformFile! );


        /*
         * 
         */
        var ss = new SchematronService();
        var ot = ss.Evaluate( input, transform );


        /*
         * 
         */
        var faCount = ot.Lines.Where( x => x is FailedAssert ).Count();
        var frCount = ot.Lines.Where( x => x is FiredRule ).Count();

        if ( faCount == 0 )
        {
            AnsiConsole.MarkupLineInterpolated( $"[green]ok[/]: file is valid. {frCount} rules fired" );
            return 0;
        }


        /*
         * 
         */
        var table = new Table();
        table.AddColumn( "Rule" );
        table.AddColumn( "Text" );
        table.SimpleBorder();

        foreach ( var fa in ot.Lines.Where( x => x is FailedAssert )
                                    .OfType<FailedAssert>() )
        {
            table.AddRow( new Text( fa.Id ), new Text( fa.Text ) );
        }

        AnsiConsole.Write( table );
        AnsiConsole.MarkupLineInterpolated( $"[red]err[/]: file is invalid, {faCount} errors found" );

        return 0;
    }
}