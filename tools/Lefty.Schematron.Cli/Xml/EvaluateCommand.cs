using McMaster.Extensions.CommandLineUtils;
using Spectre.Console;
using System.ComponentModel.DataAnnotations;

namespace Lefty.Schematron.Cli.Xml;

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
    [Option( "-s|--schema", CommandOptionType.SingleValue, Description = "Schema file" )]
    [FileExists]
    public string? SchemaFile { get; set; }

    /// <summary />
    [Option( "--ubl", CommandOptionType.NoValue, Description = "Use UBL 2.1 schema" )]
    public bool IsUbl { get; set; }

    /// <summary />
    [Option( "-c|--continue-if-xsd-errors", CommandOptionType.NoValue, Description = "When set, will continue if XSD errors are found" )]
    public bool ContinueIfSchemaErrors { get; set; }


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
         * Asserts written against a document of the right shape say little about
         * one which isn't, so whichever schema applies -- named, or pointed at by
         * the document itself -- is checked before the transform ever runs. A
         * document which names none is evaluated as it always was.
         */
        var check = SchemaValidator.Validate( this.InputFile!, this.SchemaFile, this.IsUbl );

        if ( check.Failure != null )
        {
            AnsiConsole.MarkupLineInterpolated( $"[red]err[/]: {check.Failure}" );
            return 2;
        }

        var schemaErrors = check.Errors;

        if ( schemaErrors.Count > 0 && this.ContinueIfSchemaErrors == false )
        {
            //
            // The transform never ran, so the schematron half of the document is
            // empty rather than absent: one shape, whichever way the run ended.
            //
            if ( this.Json == true )
            {
                WriteJson( schemaErrors, Array.Empty<ISchematronLine>() );
                return 1;
            }

            SchemaValidator.Report( check, json: false, this.Verbose );
            AnsiConsole.MarkupLineInterpolated( $"[red]err[/]: file is invalid as per schema, {schemaErrors.Count} errors found" );

            return 1;
        }


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
            WriteJson( schemaErrors, ot.Lines );

            if ( faCount > 0 || schemaErrors.Count > 0 )
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

        if ( faCount == 0 && schemaErrors.Count == 0 )
        {
            AnsiConsole.MarkupLineInterpolated( $"[green]ok[/]: file is valid. {frCount} rules fired" );
            return 0;
        }


        /*
         * Continuing past the schema errors is what --continue-if-xsd-errors asks
         * for -- not that they be forgotten, so they are listed here alongside the
         * asserts, and count towards the verdict just the same.
         */
        var table = new Table();
        table.AddColumn( "Rule" );
        table.AddColumn( "Flag" );
        table.AddColumn( "Text" );
        table.SimpleBorder();

        foreach ( var se in schemaErrors )
        {
            table.AddRow(
                new Markup( "[grey](xsd)[/]" ),
                new Text( "error" ),
                new Text( $"({se.LineNumber},{se.LinePosition}) {se.Message}" )
            );
        }

        foreach ( var fa in ot.Lines.OfType<FailedAssert>() )
        {
            table.AddRow(
                new Text( fa.Id ),
                new Text( fa.Flag ),
                new Text( fa.Text )
            );
        }

        AnsiConsole.Write( table );


        /*
         *
         */
        if ( schemaErrors.Count > 0 && faCount > 0 )
            AnsiConsole.MarkupLineInterpolated( $"[red]err[/]: file is invalid, {schemaErrors.Count} schema errors and {faCount} failed asserts found" );
        else if ( schemaErrors.Count > 0 )
            AnsiConsole.MarkupLineInterpolated( $"[red]err[/]: file is invalid as per schema, {schemaErrors.Count} errors found" );
        else
            AnsiConsole.MarkupLineInterpolated( $"[red]err[/]: file is invalid, {faCount} errors found" );

        return 1;
    }


    /// <summary>
    /// Writes both halves of the run as one document: whatever the schema had to
    /// say, and whatever the transform did -- a second top-level array written
    /// after the first would leave nothing a parser could read.
    /// </summary>
    private void WriteJson( IReadOnlyList<ValidationError> schema, IReadOnlyList<ISchematronLine> lines )
    {
        var output = new EvaluateOutput()
        {
            Schema = schema,
            Schematron = lines,
        };

        JsonOut.Write( output );
    }
}


/// <summary />
internal record EvaluateOutput
{
    /// <summary>
    /// Errors the schema found, if one applied; empty otherwise.
    /// </summary>
    public required IReadOnlyList<ValidationError> Schema { get; init; }

    /// <summary>
    /// Lines the transform emitted; empty when the schema errors stopped the run
    /// before it ever got there.
    /// </summary>
    public required IReadOnlyList<ISchematronLine> Schematron { get; init; }
}