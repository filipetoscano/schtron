using McMaster.Extensions.CommandLineUtils;
using Spectre.Console;
using Spectre.Console.Json;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Lefty.Schematron.Cli;

/// <summary />
[Command( "eval", Description = "Evaluates an XML file using XSL transform" )]
public class EvaluateCommand
{
    private readonly ISchematronService _ss;


    /// <summary />
    public EvaluateCommand( ISchematronService ss )
    {
        _ss = ss;
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
    [Option( "-v|--verbose", CommandOptionType.NoValue, Description = "Emit verbose output" )]
    public bool Verbose { get; set; }

    /// <summary />
    [Option( "--json", CommandOptionType.NoValue, Description = "Emit output as JSON" )]
    public bool Json { get; set; }


    /// <summary />
    public int OnExecute()
    {
        /*
         * 
         */
        using var input = File.OpenRead( this.InputFile! );
        using var transform = File.OpenRead( this.TransformFile! );


        /*
         * 
         */
        var ot = _ss.Evaluate( input, transform );


        /*
         * 
         */
        var faCount = ot.Lines.Where( x => x is FailedAssert ).Count();
        var frCount = ot.Lines.Where( x => x is FiredRule ).Count();


        /*
         * 
         */
        if ( this.Json == true )
        {
            var json = JsonSerializer.Serialize( ot.Lines );

            var jsonText = new JsonText( json );
            AnsiConsole.Write( jsonText );

            if ( faCount > 0 )
                return 1;

            return 0;
        }


        /*
         * 
         */
        if ( this.Verbose == true )
        {
            var fired = new Table();
            fired.AddColumn( "Type" );
            fired.AddColumn( "Context" );
            fired.SimpleBorder();

            foreach ( var row in ot.Lines )
            {
                if ( row is ActivePattern ap )
                    fired.AddRow( new Text( "ActivePattern" ), new Text( ap.Name ?? "(no name)" ) );

                if ( row is FiredRule fr )
                    fired.AddRow( new Text( "FiredRule" ), new Text( fr.Context ) );

                if ( row is SuccessfulReport rp )
                    fired.AddRow( new Markup( "[blue]SuccessfulReport[/]" ), new Text( rp.Text ) );

                if ( row is SuppressedRule sr )
                    fired.AddRow( new Markup( "[yellow]SuppressedRule[/]" ), new Text( sr.Context ) );
            }

            AnsiConsole.Write( fired );
        }


        /*
         * 
         */
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
        table.AddColumn( "Flag" );
        table.AddColumn( "Text" );
        table.SimpleBorder();

        foreach ( var fa in ot.Lines.OfType<FailedAssert>() )
        {
            table.AddRow(
                new Text( fa.Id ),
                new Text( fa.Flag ),
                new Text( fa.Text )
            );
        }

        AnsiConsole.Write( table );
        AnsiConsole.MarkupLineInterpolated( $"[red]err[/]: file is invalid, {faCount} errors found" );

        return 1;
    }
}