using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.XPath;

namespace Lefty.Schematron;

/// <summary>
/// Validates, compiles and evaluates Schematron.
/// </summary>
/// <remarks>
/// Instances are not safe to share between threads. A
/// <see cref="CompiledSchematron" /> is, so the usual arrangement for
/// concurrent work is a service per thread and a compiled schema shared
/// between them.
/// </remarks>
public partial class SchematronService : ISchematronService, ISchematronCompiler
{
    private readonly SchematronServiceOptions _options;
    private readonly ILogger _logger;

    /*
     * Taken once, rather than searched linearly for every assertion in the
     * schema.
     */
    private readonly HashSet<string> _acceptedFlags;
    private readonly HashSet<string> _acceptedRoles;


    /// <summary>
    /// Creates the service, discarding the XSLT engine's own diagnostics.
    /// </summary>
    /// <param name="options">Options.</param>
    public SchematronService( SchematronServiceOptions options )
        : this( options, null )
    {
    }


    /// <summary>
    /// Creates the service, logging the XSLT engine's own diagnostics -- every
    /// error and warning it reports while compiling or running a transform --
    /// to <paramref name="logger" /> at Debug.
    /// </summary>
    /// <param name="options">Options.</param>
    /// <param name="logger">Logger, or null to discard the diagnostics.</param>
    public SchematronService( SchematronServiceOptions options, ILogger<SchematronService>? logger )
    {
        _options = options;
        _logger = logger ?? (ILogger) NullLogger.Instance;
        _acceptedFlags = [ .. options.AcceptedFlags ];
        _acceptedRoles = [ .. options.AcceptedRoles ];
    }


    /// <inheritdoc />
    public ValidationResult Validate( Stream input )
    {
        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null,
        };

        XDocument doc;
        using ( var reader = XmlReader.Create( input, settings ) )
        {
            doc = XDocument.Load( reader, LoadOptions.SetLineInfo );
        }

        return Check( doc );
    }


    /// <inheritdoc />
    public async Task<ValidationResult> ValidateAsync( Stream input, CancellationToken cancellationToken = default )
    {
        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null,
            Async = true,
        };

        XDocument doc;
        using ( var reader = XmlReader.Create( input, settings ) )
        {
            doc = await XDocument.LoadAsync( reader, LoadOptions.SetLineInfo, cancellationToken ).ConfigureAwait( false );
        }

        cancellationToken.ThrowIfCancellationRequested();

        return Check( doc );
    }


    /// <summary>
    /// Applies the XSD, then the @id/@flag/@role policy, to a schema which has
    /// already been read.
    /// </summary>
    private ValidationResult Check( XDocument doc )
    {
        /*
         * TODO: Add support for sch:include
         */


        /*
         * Schema validation
         */
        var errors = new List<ValidationError>();

        doc.Validate( Xsd.Schemas, ( sender, e ) =>
        {
            if ( e.Severity != XmlSeverityType.Error )
                return;

            var ex = e.Exception as XmlSchemaValidationException;
            errors.Add( new ValidationError()
            {
                Message = e.Message,
                LineNumber = ex?.LineNumber ?? -1,
                LinePosition = ex?.LinePosition ?? -1,
            } );
        } );


        /*
         *
         */
        var checkFlag = CheckFlag( _options.SeverityMode );
        var checkRole = CheckRole( _options.SeverityMode );

        foreach ( var elem in doc.XPathSelectElements( " //sch:assert | //sch:report ", Ns.Manager ) )
        {
            var lineInfo = (IXmlLineInfo) elem;
            var ln = lineInfo.HasLineInfo() ? lineInfo.LineNumber : -1;
            var lp = lineInfo.HasLineInfo() ? lineInfo.LinePosition : -1;

            // Validate @id
            if ( _options.IdRequired == true )
            {
                if ( elem.Attribute( "id" ) == null )
                {
                    errors.Add( new ValidationError()
                    {
                        Message = $"Required @id attribute missing",
                        LineNumber = ln,
                        LinePosition = lp,
                    } );
                }
            }


            // Validate @flag/@role
            var flag = elem.Attribute( "flag" )?.Value;
            var role = elem.Attribute( "role" )?.Value;

            if ( flag != null && checkFlag == true )
            {
                if ( _acceptedFlags.Contains( flag ) == false )
                {
                    errors.Add( new ValidationError()
                    {
                        Message = $"Invalid @flag value '{flag}'",
                        LineNumber = ln,
                        LinePosition = lp,
                    } );
                }
            }

            if ( role != null && checkRole == true )
            {
                if ( _acceptedRoles.Contains( role ) == false )
                {
                    errors.Add( new ValidationError()
                    {
                        Message = $"Invalid @role value '{role}'",
                        LineNumber = ln,
                        LinePosition = lp,
                    } );
                }
            }

            switch ( _options.SeverityMode )
            {
                case SeverityMode.FlagRequired:
                    {
                        if ( flag == null )
                        {
                            errors.Add( new ValidationError()
                            {
                                Message = $"Required @flag attribute missing",
                                LineNumber = ln,
                                LinePosition = lp,
                            } );
                        }

                        if ( role != null )
                        {
                            errors.Add( new ValidationError()
                            {
                                Message = $"Forbidden @role attribute specified",
                                LineNumber = ln,
                                LinePosition = lp,
                            } );
                        }

                        break;
                    }

                case SeverityMode.RoleRequired:
                    {
                        if ( role == null )
                        {
                            errors.Add( new ValidationError()
                            {
                                Message = $"Required @role attribute missing",
                                LineNumber = ln,
                                LinePosition = lp,
                            } );
                        }

                        if ( flag != null )
                        {
                            errors.Add( new ValidationError()
                            {
                                Message = $"Forbidden @flag attribute specified",
                                LineNumber = ln,
                                LinePosition = lp,
                            } );
                        }

                        break;
                    }

                case SeverityMode.OneOfRequired:
                    {
                        if ( role == null && flag == null )
                        {
                            errors.Add( new ValidationError()
                            {
                                Message = $"Required @flag or @role attribute missing",
                                LineNumber = ln,
                                LinePosition = lp,
                            } );
                        }

                        if ( role != null && flag != null )
                        {
                            errors.Add( new ValidationError()
                            {
                                Message = $"Only one of @flag/@role attribute may be specified",
                                LineNumber = ln,
                                LinePosition = lp,
                            } );
                        }

                        break;
                    }

                case SeverityMode.OneRequired:
                    {
                        if ( role == null && flag == null )
                        {
                            errors.Add( new ValidationError()
                            {
                                Message = $"Required @flag or @role attribute missing",
                                LineNumber = ln,
                                LinePosition = lp,
                            } );
                        }

                        break;
                    }

                default:
                    //
                    break;
            }
        }


        /*
         *
         */
        errors.TrimExcess();

        return new ValidationResult()
        {
            IsValid = errors.Count == 0,
            Errors = errors.AsReadOnly(),
        };
    }


    /// <summary />
    private bool CheckFlag( SeverityMode severityMode )
    {
        return severityMode switch
        {
            SeverityMode.FlagRequired => true,
            SeverityMode.OneRequired => true,
            SeverityMode.OneOfRequired => true,
            SeverityMode.Optional => true,

            _ => false,
        };
    }


    /// <summary />
    private bool CheckRole( SeverityMode severityMode )
    {
        return severityMode switch
        {
            SeverityMode.RoleRequired => true,
            SeverityMode.OneRequired => true,
            SeverityMode.OneOfRequired => true,
            SeverityMode.Optional => true,

            _ => false,
        };
    }


    /// <inheritdoc />
    public CompiledSchematron Compile( Stream schema, OutputFormat format = OutputFormat.Xslt3 )
    {
        return Compile( Read( schema ), format );
    }


    /// <inheritdoc />
    public async Task<CompiledSchematron> CompileAsync( Stream schema, OutputFormat format = OutputFormat.Xslt3, CancellationToken cancellationToken = default )
    {
        var text = await ReadAsync( schema, cancellationToken ).ConfigureAwait( false );

        cancellationToken.ThrowIfCancellationRequested();

        return Compile( text, format );
    }


    /// <inheritdoc />
    public CompiledSchematron Load( Stream transform )
    {
        return Load( Read( transform ) );
    }


    /// <inheritdoc />
    public async Task<CompiledSchematron> LoadAsync( Stream transform, CancellationToken cancellationToken = default )
    {
        var text = await ReadAsync( transform, cancellationToken ).ConfigureAwait( false );

        cancellationToken.ThrowIfCancellationRequested();

        return Load( text );
    }


    /// <inheritdoc />
    public void Transform( Stream input, Stream output, OutputFormat format = OutputFormat.Xslt3 )
    {
        /*
         * Transpiles without going on to compile: the caller wants the XSLT
         * as text, and compiling an executable to discard it is pure cost.
         */
        var xslt = Xslt.Transpile( Read( input ), format, _logger );

        using ( var sw = new StreamWriter( output, leaveOpen: true ) )
        {
            sw.Write( xslt );
        }
    }


    /// <inheritdoc />
    public async Task TransformAsync( Stream input, Stream output, OutputFormat format = OutputFormat.Xslt3, CancellationToken cancellationToken = default )
    {
        var schema = await ReadAsync( input, cancellationToken ).ConfigureAwait( false );

        cancellationToken.ThrowIfCancellationRequested();

        var xslt = Xslt.Transpile( schema, format, _logger );

        var sw = new StreamWriter( output, leaveOpen: true );

        await using ( sw.ConfigureAwait( false ) )
        {
            await sw.WriteAsync( xslt.AsMemory(), cancellationToken ).ConfigureAwait( false );
        }
    }


    /// <inheritdoc />
    public SchematronOutput Evaluate( Stream document, Stream transform )
    {
        return Load( transform ).Evaluate( document );
    }


    /// <inheritdoc />
    public async Task<SchematronOutput> EvaluateAsync( Stream document, Stream transform, CancellationToken cancellationToken = default )
    {
        var compiled = await LoadAsync( transform, cancellationToken ).ConfigureAwait( false );

        return await compiled.EvaluateAsync( document, cancellationToken ).ConfigureAwait( false );
    }


    /// <summary />
    private CompiledSchematron Compile( string schema, OutputFormat format )
    {
        return new CompiledSchematron( Xslt.Transpile( schema, format, _logger ), _logger );
    }


    /// <summary />
    private CompiledSchematron Load( string transform )
    {
        return new CompiledSchematron( transform, _logger );
    }


    /// <summary />
    private static string Read( Stream stream )
    {
        using var sr = new StreamReader( stream, leaveOpen: true );

        return sr.ReadToEnd();
    }


    /// <summary />
    private static async Task<string> ReadAsync( Stream stream, CancellationToken cancellationToken )
    {
        using var sr = new StreamReader( stream, leaveOpen: true );

        return await sr.ReadToEndAsync( cancellationToken ).ConfigureAwait( false );
    }
}