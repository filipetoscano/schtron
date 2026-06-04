using McMaster.Extensions.CommandLineUtils;
using Spectre.Console;
using Spectre.Console.Json;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Lefty.Schematron.Cli;

/// <summary />
[Command( "validate", Description = "Validates a Schematron file" )]
public class ValidateCommand
{
    private readonly ISchematronService _ss;


    /// <summary />
    public ValidateCommand( ISchematronService ss )
    {
        _ss = ss;
    }


    /// <summary />
    [Argument( 0, Description = "Input file" )]
    [Required]
    [FileExists]
    public string? InputFile { get; set; }

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


        /*
         * 
         */
        var res = _ss.Validate( input );


        /*
         * 
         */
        if ( this.Json == true )
        {
            var json = JsonSerializer.Serialize( res.Errors );

            var jsonText = new JsonText( json );
            AnsiConsole.Write( jsonText );

            return res.IsValid == true ? 0 : 1;
        }


        /*
         * 
         */
        if ( res.IsValid == false )
        {
            var table = new Table();
            table.AddColumn( "Line" );
            table.AddColumn( "Col" );
            table.AddColumn( "Error" );
            table.SimpleBorder();

            foreach ( var msg in res.Errors )
                table.AddRow( msg.LineNumber.ToString(), msg.LinePosition.ToString(), msg.Message );

            AnsiConsole.Write( table );
            AnsiConsole.MarkupLineInterpolated( $"[red]err[/]: file is invalid" );
            return 1;
        }

        AnsiConsole.MarkupLineInterpolated( $"[green]ok[/]: file is valid" );
        return 0;
    }
}