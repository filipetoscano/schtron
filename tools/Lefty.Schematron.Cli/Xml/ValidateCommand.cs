using McMaster.Extensions.CommandLineUtils;
using Spectre.Console;
using System.ComponentModel.DataAnnotations;

namespace Lefty.Schematron.Cli.Xml;

/// <summary />
[Command( "validate", Description = "Validates an XML file against an XML schema" )]
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
    [Option( "-s|--schema", CommandOptionType.SingleValue, Description = "Schema file" )]
    [FileExists]
    public string? SchemaFile { get; set; }

    /// <summary />
    [Option( "--ubl", CommandOptionType.NoValue, Description = "Use UBL 2.1 schema" )]
    public bool IsUbl { get; set; }


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
         * Validating is the whole of the command, so a document which names no
         * schema of its own leaves nothing to validate against, and saying so is
         * better than reporting a file nothing was ever checked against as valid.
         */
        var check = SchemaValidator.Validate( this.InputFile!, this.SchemaFile, this.IsUbl );

        if ( check.Failure != null )
        {
            AnsiConsole.MarkupLineInterpolated( $"[red]err[/]: {check.Failure}" );
            return 2;
        }

        if ( check.Skipped == true )
        {
            AnsiConsole.MarkupLine( "[red]err[/]: no schema specified, and document carries no xsi:schemaLocation" );
            return 2;
        }


        /*
         *
         */
        var rc = SchemaValidator.Report( check, this.Json, this.Verbose );

        if ( this.Json == true )
            return rc;

        if ( rc == 0 )
            AnsiConsole.MarkupLine( "[green]ok[/]: file is valid as per schema" );
        else
            AnsiConsole.MarkupLineInterpolated( $"[red]err[/]: file is invalid, {check.Errors.Count} errors found" );

        return rc;
    }
}
