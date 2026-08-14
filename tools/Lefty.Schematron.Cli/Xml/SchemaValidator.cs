using Lefty.Schemas;
using Spectre.Console;
using Spectre.Console.Json;
using System.Text.Json;
using System.Xml;
using System.Xml.Schema;

namespace Lefty.Schematron.Cli.Xml;

/// <summary>
/// Validates an XML file against the schema named by the caller -- the UBL 2.1
/// set, a schema file, or the document's own xsi:schemaLocation hints -- which
/// is what both 'xml validate' and 'xml eval' need done before anything else.
/// </summary>
internal static class SchemaValidator
{
    /// <summary />
    private const string XsiNs = "http://www.w3.org/2001/XMLSchema-instance";


    /// <summary>
    /// Validates <paramref name="inputFile" /> against the UBL 2.1 set, against
    /// <paramref name="schemaFile" />, or -- failing both -- against whatever the
    /// document's own xsi:schemaLocation points at. A document which names no
    /// schema at all is neither valid nor invalid: the check comes back skipped,
    /// and it is the caller who decides whether that is good enough.
    /// </summary>
    internal static SchemaCheck Validate( string inputFile, string? schemaFile, bool isUbl )
    {
        /*
         * Each of --ubl and --schema names a different schema, so honouring both
         * is meaningless: refuse, rather than silently picking one.
         */
        if ( isUbl == true && schemaFile != null )
            return Failure( "options --ubl and --schema are mutually exclusive" );


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
            root = LoadRoot( inputFile );
        }
        catch ( XmlException ex )
        {
            //
            // A document which doesn't parse is invalid in exactly the way the
            // caller asked about, so it is reported as any other error is --
            // never as a stack trace.
            //
            errors.Add( AsError( ex ) );

            return new SchemaCheck()
            {
                Failure = null,
                Skipped = false,
                Errors = errors.AsReadOnly(),
                Warnings = warnings.AsReadOnly(),
            };
        }


        /*
         *
         */
        XmlSchemaSet schemas;
        XmlResolver? resolver = null;
        var fromSchemaLocation = false;

        if ( isUbl == true )
        {
            //
            // Validating a non-UBL document against the UBL set would report no
            // error at all: an undeclared root element is a validation warning,
            // not an error, so the file would come out "valid" without a single
            // rule ever being applied to it.
            //
            if ( Ubl21.Is( root ) == false )
                return Failure( "document is not a UBL 2.1 Invoice or CreditNote" );

            schemas = Ubl21.Schemas;
        }
        else if ( schemaFile != null )
        {
            try
            {
                schemas = LoadSchema( schemaFile );
            }
            catch ( XmlException ex )
            {
                return Failure( $"schema file is not well-formed: {ex.Message}" );
            }
            catch ( XmlSchemaException ex )
            {
                return Failure( $"schema file is not a valid schema: {ex.Message}" );
            }
            catch ( RemoteSchemaException ex )
            {
                return Failure( $"refusing to fetch remote schema '{ex.Location}'" );
            }

            //
            // Same trap as --ubl above: a schema which says nothing about the
            // root element yields a warning and a clean bill of health.
            //
            if ( schemas.GlobalElements.Contains( new XmlQualifiedName( root.LocalName, root.NamespaceURI ) ) == false )
                return Failure( $"schema does not declare root element '{root.Name}'" );
        }
        else
        {
            var hints = Hints( root ).ToList();

            if ( hints.Count == 0 )
                return Skip();

            //
            // Hints are followed on disk only, relative to the document itself:
            // validating a file is never a reason to reach out to the network.
            //
            var baseUri = new Uri( Path.GetFullPath( inputFile ) );

            foreach ( var hint in hints )
            {
                if ( Uri.TryCreate( baseUri, hint, out var uri ) == false || uri.IsFile == false )
                    return Failure( $"refusing to fetch remote schema '{hint}'" );
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
            using var xr = XmlReader.Create( inputFile, xrs );

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
            return Failure( $"refusing to fetch remote schema '{ex.Location}'" );
        }
        catch ( XmlSchemaException ex )
        {
            return Failure( $"schema could not be loaded: {ex.Message}" );
        }
        catch ( XmlException ex )
        {
            errors.Add( AsError( ex ) );
        }


        /*
         * Same trap as the two branches above, except that hints can only be
         * judged once they have been followed: the schemas the reader pulls in
         * never reach the set handed to it, so the root element itself is what
         * gets asked whether anything described it.
         */
        if ( fromSchemaLocation == true && rootWarnings > 0 )
            return Failure( $"xsi:schemaLocation declares no schema for root element '{root.Name}'" );


        /*
         *
         */
        return new SchemaCheck()
        {
            Failure = null,
            Skipped = false,
            Errors = errors.AsReadOnly(),
            Warnings = warnings.AsReadOnly(),
        };
    }


    /// <summary>
    /// Writes whatever the check found, and returns the exit code the errors
    /// warrant. The 'valid' line is left to the caller: 'eval' has more work to
    /// do before it can claim anything of the sort.
    /// </summary>
    internal static int Report( SchemaCheck check, bool json, bool verbose )
    {
        /*
         *
         */
        if ( json == true )
        {
            var text = JsonSerializer.Serialize( check.Errors );

            var jsonText = new JsonText( text );
            AnsiConsole.Write( jsonText );

            return check.Errors.Count == 0 ? 0 : 1;
        }


        /*
         *
         */
        if ( verbose == true && check.Warnings.Count > 0 )
        {
            var table = new Table();
            table.AddColumn( "Line" );
            table.AddColumn( "Col" );
            table.AddColumn( "Warning" );
            table.SimpleBorder();

            foreach ( var msg in check.Warnings )
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
        if ( check.Errors.Count > 0 )
        {
            var table = new Table();
            table.AddColumn( "Line" );
            table.AddColumn( "Col" );
            table.AddColumn( "Error" );
            table.SimpleBorder();

            foreach ( var msg in check.Errors )
            {
                table.AddRow(
                    new Text( msg.LineNumber.ToString() ),
                    new Text( msg.LinePosition.ToString() ),
                    new Text( msg.Message )
                );
            }

            AnsiConsole.Write( table );

            return 1;
        }


        /*
         *
         */
        return 0;
    }


    /// <summary />
    private static SchemaCheck Failure( string message )
    {
        return new SchemaCheck()
        {
            Failure = message,
            Skipped = false,
            Errors = Array.Empty<ValidationError>(),
            Warnings = Array.Empty<ValidationError>(),
        };
    }


    /// <summary />
    private static SchemaCheck Skip()
    {
        return new SchemaCheck()
        {
            Failure = null,
            Skipped = true,
            Errors = Array.Empty<ValidationError>(),
            Warnings = Array.Empty<ValidationError>(),
        };
    }


    /// <summary />
    private static ValidationError AsError( XmlException ex )
    {
        return new ValidationError()
        {
            Message = ex.Message,
            LineNumber = ex.LineNumber,
            LinePosition = ex.LinePosition,
        };
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


/// <summary />
internal record SchemaCheck
{
    /// <summary>
    /// Set when no validation could be performed at all -- a bad option, an
    /// unusable schema -- as opposed to a document which was validated and
    /// found wanting.
    /// </summary>
    public required string? Failure { get; init; }

    /// <summary>
    /// Set when no schema was named and the document points at none of its own,
    /// so nothing was validated either way.
    /// </summary>
    public required bool Skipped { get; init; }

    /// <summary />
    public required IReadOnlyList<ValidationError> Errors { get; init; }

    /// <summary />
    public required IReadOnlyList<ValidationError> Warnings { get; init; }
}
