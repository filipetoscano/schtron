using McMaster.Extensions.CommandLineUtils;
using Spectre.Console;
using System.ComponentModel.DataAnnotations;

namespace Lefty.Schematron.Cli;

/// <summary />
[Command( "validate", Description = "Validates a Schematron file" )]
public class ValidateCommand
{
    /// <summary />
    public ValidateCommand()
    {
    }


    /// <summary />
    [Argument( 0, Description = "Input file" )]
    [Required]
    [FileExists]
    public string? InputFile { get; set; }


    /// <summary />
    public int OnExecute()
    {
        /*
         * 
         */
        var input = File.OpenRead( this.InputFile! );


        /*
         * 
         */
        var ss = new SchematronService();
        var res = ss.Validate( input );

        if ( res.IsValid == false )
        {
            var table = new Table();
            table.AddColumn( "Error" );
            table.SimpleBorder();

            foreach ( var msg in res.Errors )
                table.AddRow( msg );

            AnsiConsole.Write( table );
            AnsiConsole.MarkupLineInterpolated( $"[red]err[/]: file is invalid" );
            return 1;
        }
        
        AnsiConsole.MarkupLineInterpolated( $"[green]ok[/]: file is valid" );
        return 0;
    }
}