using Lefty.Schemas;
using McMaster.Extensions.CommandLineUtils;
using Spectre.Console;
using Spectre.Console.Json;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Xml;
using System.Xml.Schema;

namespace Lefty.Schematron.Cli.Xml;

/// <summary />
[Command( "validate", Description = "Validates an XML file against an XML schema" )]
public class ValidateCommand
{
    /// <summary />
    private const string XsiNs = "http://www.w3.org/2001/XMLSchema-instance";


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
         * Each of --ubl and --schema names a different schema, so honouring both
         * is meaningless: refuse, rather than silently picking one.
         */
        if ( this.IsUbl == true && this.SchemaFile != null )
        {
            AnsiConsole.MarkupLine( "[red]err[/]: options --ubl and --schema are mutually exclusive" );
            return 2;
        }


        /*
         * The root element decides which schema applies: it is what says whether
         * the document is UBL, and it carries the xsi:schemaLocation hints which
         * are the last resort when no schema was named.
         */
        var errors = new List<ValidationError>();
        var warnings = new List<ValidationError>();

        XmlElement root;

        try
        {
            root = LoadRoot( this.InputFile! );
        }
        catch ( XmlException ex )
        {
            //
            // A document which doesn't parse is invalid in exactly the way the
            // caller asked about, so it is reported as any other error is --
            // never as a stack trace.
            //
            errors.Add( new ValidationError()
            {
                Message = ex.Message,
                LineNumber = ex.LineNumber,
                LinePosition = ex.LinePosition,
            } );

            return Report( errors, warnings );
        }


        /*
         *
         */
        XmlSchemaSet schemas;
        XmlResolver? resolver = null;
        var fromSchemaLocation = false;

        if ( this.IsUbl == true )
        {
            //
            // Validating a non-UBL document against the UBL set would report no
            // error at all: an undeclared root element is a validation warning,
            // not an error, so the file would come out "valid" without a single
            // rule ever being applied to it.
            //
            if ( Ubl21.Is( root ) == false )
            {
                AnsiConsole.MarkupLineInterpolated( $"[red]err[/]: document is not a UBL 2.1 Invoice or CreditNote" );
                return 2;
            }

            schemas = Ubl21.Schemas;
        }
        else if ( this.SchemaFile != null )
        {
            try
            {
                schemas = LoadSchema( this.SchemaFile );
            }
            catch ( XmlException ex )
            {
                AnsiConsole.MarkupLineInterpolated( $"[red]err[/]: schema file is not well-formed: {ex.Message}" );
                return 2;
            }
            catch ( XmlSchemaException ex )
            {
                AnsiConsole.MarkupLineInterpolated( $"[red]err[/]: schema file is not a valid schema: {ex.Message}" );
                return 2;
            }
            catch ( RemoteSchemaException ex )
            {
                AnsiConsole.MarkupLineInterpolated( $"[red]err[/]: refusing to fetch remote schema '{ex.Location}'" );
                return 2;
            }

            //
            // Same trap as --ubl above: a schema which says nothing about the
            // root element yields a warning and a clean bill of health.
            //
            if ( schemas.GlobalElements.Contains( new XmlQualifiedName( root.LocalName, root.NamespaceURI ) ) == false )
            {
                AnsiConsole.MarkupLineInterpolated( $"[red]err[/]: schema does not declare root element '{root.Name}'" );
                return 2;
            }
        }
        else
        {
            var hints = Hints( root ).ToList();

            if ( hints.Count == 0 )
            {
                AnsiConsole.MarkupLine( "[red]err[/]: no schema specified, and document carries no xsi:schemaLocation" );
                return 2;
            }

            //
            // Hints are followed on disk only, relative to the document itself:
            // validating a file is never a reason to reach out to the network.
            //
            var baseUri = new Uri( Path.GetFullPath( this.InputFile! ) );

            foreach ( var hint in hints )
            {
                if ( Uri.TryCreate( baseUri, hint, out var uri ) == false || uri.IsFile == false )
                {
                    AnsiConsole.MarkupLineInterpolated( $"[red]err[/]: refusing to fetch remote schema '{hint}'" );
                    return 2;
                }
            }

            resolver = new LocalResolver();

            schemas = new XmlSchemaSet();
            schemas.XmlResolver = resolver;

            fromSchemaLocation = true;
        }


        /*
         * Validating over the file, rather than over the document just parsed,
         * is what keeps line/position on each error -- and, in the schemaLocation
         * case, what gives relative hints a base URI to resolve against.
         */
        var flags = XmlSchemaValidationFlags.ProcessIdentityConstraints
            | XmlSchemaValidationFlags.ReportValidationWarnings;

        if ( fromSchemaLocation == true )
            flags |= XmlSchemaValidationFlags.ProcessSchemaLocation;

        var xrs = new XmlReaderSettings()
        {
            ValidationType = ValidationType.Schema,
            ValidationFlags = flags,
            Schemas = schemas,

            // DTDs are prohibited outright, which is also what keeps the resolver
            // above -- present only to reach the schemas -- away from entities.
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = resolver,
        };

        xrs.ValidationEventHandler += ( s, ev ) =>
        {
            var list = ev.Severity == XmlSeverityType.Error ? errors : warnings;

            list.Add( new ValidationError()
            {
                Message = ev.Message,
                LineNumber = ev.Exception?.LineNumber ?? -1,
                LinePosition = ev.Exception?.LinePosition ?? -1,
            } );
        };

        var rootWarnings = -1;

        try
        {
            using var xr = XmlReader.Create( this.InputFile!, xrs );

            while ( xr.Read() )
            {
                //
                // The root element is the first element the reader surfaces, and
                // any warning raised by the time it lands is the schema set saying
                // it knows nothing about it -- see below.
                //
                if ( rootWarnings == -1 && xr.NodeType == XmlNodeType.Element )
                    rootWarnings = warnings.Count;
            }
        }
        catch ( RemoteSchemaException ex )
        {
            AnsiConsole.MarkupLineInterpolated( $"[red]err[/]: refusing to fetch remote schema '{ex.Location}'" );
            return 2;
        }
        catch ( XmlSchemaException ex )
        {
            AnsiConsole.MarkupLineInterpolated( $"[red]err[/]: schema could not be loaded: {ex.Message}" );
            return 2;
        }
        catch ( XmlException ex )
        {
            errors.Add( new ValidationError()
            {
                Message = ex.Message,
                LineNumber = ex.LineNumber,
                LinePosition = ex.LinePosition,
            } );
        }


        /*
         * Same trap as the two branches above, except that hints can only be
         * judged once they have been followed: the schemas the reader pulls in
         * never reach the set handed to it, so the root element itself is what
         * gets asked whether anything described it.
         */
        if ( fromSchemaLocation == true && rootWarnings > 0 )
        {
            AnsiConsole.MarkupLineInterpolated( $"[red]err[/]: xsi:schemaLocation declares no schema for root element '{root.Name}'" );
            return 2;
        }


        /*
         *
         */
        return Report( errors, warnings );
    }


    /// <summary />
    private int Report( List<ValidationError> errors, List<ValidationError> warnings )
    {
        /*
         *
         */
        if ( this.Json == true )
        {
            var json = JsonSerializer.Serialize( errors );

            var jsonText = new JsonText( json );
            AnsiConsole.Write( jsonText );

            return errors.Count == 0 ? 0 : 1;
        }


        /*
         *
         */
        if ( this.Verbose == true && warnings.Count > 0 )
        {
            var table = new Table();
            table.AddColumn( "Line" );
            table.AddColumn( "Col" );
            table.AddColumn( "Warning" );
            table.SimpleBorder();

            foreach ( var msg in warnings )
            {
                table.AddRow(
                    new Text( msg.LineNumber.ToString() ),
                    new Text( msg.LinePosition.ToString() ),
                    new Markup( $"[yellow]{Markup.Escape( msg.Message )}[/]" )
                );
            }

            AnsiConsole.Write( table );
        }


        /*
         *
         */
        if ( errors.Count > 0 )
        {
            var table = new Table();
            table.AddColumn( "Line" );
            table.AddColumn( "Col" );
            table.AddColumn( "Error" );
            table.SimpleBorder();

            foreach ( var msg in errors )
            {
                table.AddRow(
                    new Text( msg.LineNumber.ToString() ),
                    new Text( msg.LinePosition.ToString() ),
                    new Text( msg.Message )
                );
            }

            AnsiConsole.Write( table );
            AnsiConsole.MarkupLineInterpolated( $"[red]err[/]: file is invalid, {errors.Count} errors found" );

            return 1;
        }


        /*
         *
         */
        AnsiConsole.MarkupLine( "[green]ok[/]: file is valid as per schema" );

        return 0;
    }


    /// <summary />
    private static XmlElement LoadRoot( string file )
    {
        var xrs = new XmlReaderSettings()
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null,
        };

        var doc = new XmlDocument();

        using ( var xr = XmlReader.Create( file, xrs ) )
        {
            doc.Load( xr );
        }

        if ( doc.DocumentElement == null )
            throw new XmlException( "Document has no root element." );

        return doc.DocumentElement;
    }


    /// <summary />
    private static XmlSchemaSet LoadSchema( string file )
    {
        //
        // Includes and imports are resolved from the schema's own folder, so the
        // set carries a resolver -- while the reader keeps a null one, which is
        // what confines DTD parsing to whatever internal subset the file has.
        //
        var xrs = new XmlReaderSettings()
        {
            DtdProcessing = DtdProcessing.Parse,
            XmlResolver = null,
        };

        var ss = new XmlSchemaSet();
        ss.XmlResolver = new LocalResolver();

        using ( var xr = XmlReader.Create( file, xrs ) )
        {
            ss.Add( null, xr );
        }

        ss.Compile();

        return ss;
    }


    /// <summary />
    private static IEnumerable<string> Hints( XmlElement root )
    {
        /*
         * xsi:schemaLocation is a whitespace-separated list of pairs, of which
         * the second half of each pair is the location; xsi:noNamespaceSchemaLocation
         * is a location on its own.
         */
        var sl = root.GetAttribute( "schemaLocation", XsiNs );

        if ( string.IsNullOrWhiteSpace( sl ) == false )
        {
            var parts = sl.Split( (char[]?) null, StringSplitOptions.RemoveEmptyEntries );

            for ( var i = 1; i < parts.Length; i += 2 )
                yield return parts[ i ];
        }

        var nnsl = root.GetAttribute( "noNamespaceSchemaLocation", XsiNs );

        if ( string.IsNullOrWhiteSpace( nnsl ) == false )
            yield return nnsl.Trim();
    }


    /// <summary />
    private class LocalResolver : XmlUrlResolver
    {
        /// <summary />
        public override object? GetEntity( Uri absoluteUri, string? role, Type? ofObjectToReturn )
        {
            //
            // The pre-flight check only sees the hints on the root element: an
            // import nested inside a local schema can still point off-machine,
            // and is turned back here.
            //
            if ( absoluteUri.IsFile == false )
                throw new RemoteSchemaException( absoluteUri.OriginalString );

            return base.GetEntity( absoluteUri, role, ofObjectToReturn );
        }
    }


    /// <summary />
    private class RemoteSchemaException : Exception
    {
        /// <summary />
        public RemoteSchemaException( string location )
            : base( $"Refusing to fetch remote schema '{location}'." )
        {
            this.Location = location;
        }


        /// <summary />
        public string Location { get; private set; }
    }
}
