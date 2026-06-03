using javax.xml.transform;
using Lefty.Schematron.Saxon;
using net.liberty_development.SaxonHE12s9apiExtensions;
using net.sf.saxon.s9api;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.XPath;

namespace Lefty.Schematron;

/// <summary />
public partial class SchematronService : ISchematronService
{
    private readonly XmlNamespaceManager _ns;
    private readonly SchematronServiceOptions _options;


    /// <summary />
    public SchematronService( SchematronServiceOptions options )
    {
        _ns = Ns.Manager;
        _options = options;
    }


    /// <inheritdoc />
    public ValidationResult Validate( Stream input )
    {
        /*
         *
         */
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

            if ( flag != null )
            {
                if ( _options.AcceptedFlags.Contains( flag ) == false )
                {
                    errors.Add( new ValidationError()
                    {
                        Message = $"Invalid @flag value '{flag}'",
                        LineNumber = ln,
                        LinePosition = lp,
                    } );
                }
            }

            if ( role != null )
            {
                if ( _options.AcceptedRoles.Contains( role ) == false )
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


    /// <inheritdoc />
    public void Transform( Stream input, Stream output, OutputFormat format = OutputFormat.Xslt3 )
    {
        /*
         * 
         */
        string inputXml;

        using ( var sr = new StreamReader( input ) )
        {
            inputXml = sr.ReadToEnd();
        }

        var transform = LoadTransformer( format );


        /*
         * 
         */
        var sch = inputXml.AsSource();
        var outputXml = ApplyTransform( sch, transform );


        /*
         * 
         */
        using ( var sw = new StreamWriter( output ) )
        {
            sw.Write( outputXml );
        }
    }


    /// <inheritdoc />
    public SchematronOutput Evaluate( Stream document, Stream transform )
    {
        /*
         * 
         */
        string inputXml;
        string transformXml;

        using ( var sr = new StreamReader( document ) )
        {
            inputXml = sr.ReadToEnd();
        }

        using ( var sr = new StreamReader( transform ) )
        {
            transformXml = sr.ReadToEnd();
        }


        /*
         * 
         */
        var processor = new Processor();
        var compiler = processor.newXsltCompiler();

        var transformSrc = transformXml.AsSource();
        var xslt = compiler.compile( transformSrc );


        /*
         * 
         */
        var inputSrc = inputXml.AsSource();
        var xml = ApplyTransform( inputSrc, xslt );

        return ParseOutput( xml );
    }


    /// <summary />
    private SchematronOutput ParseOutput( string xml )
    {
        var doc = new XmlDocument();
        doc.LoadXml( xml );


        /*
         * 
         */
        var lines = new List<ISchematronLine>();

        foreach ( XmlElement elem in doc.SelectNodes( " /svrl:schematron-output/svrl:* ", _ns )! )
        {
            if ( elem.LocalName == "active-pattern" )
            {
                lines.Add( new ActivePattern()
                {
                    Id = elem.Attributes[ "id" ]?.Value ?? "##err",
                    Name = elem.Attributes[ "name" ]?.Value,
                } );
            }

            if ( elem.LocalName == "failed-assert" )
            {
                lines.Add( new FailedAssert()
                {
                    Id = elem.Attributes[ "id" ]?.Value ?? "##err",
                    Flag = elem.Attributes[ "flag" ]?.Value ?? "##err",
                    Location = elem.Attributes[ "location" ]?.Value ?? "##err",
                    Test = elem.Attributes[ "test" ]?.Value ?? "##err",
                    Text = elem.SelectSingleNode( " svrl:text ", _ns )?.InnerText ?? "##err",
                } );
            }

            if ( elem.LocalName == "successful-report" )
            {
                lines.Add( new SuccessfulReport()
                {
                    Id = elem.Attributes[ "id" ]?.Value ?? "##err",
                    Flag = elem.Attributes[ "flag" ]?.Value ?? "##err",
                    Location = elem.Attributes[ "location" ]?.Value ?? "##err",
                    Test = elem.Attributes[ "test" ]?.Value ?? "##err",
                    Text = elem.SelectSingleNode( " svrl:text ", _ns )?.InnerText ?? "##err",
                } );
            }

            if ( elem.LocalName == "fired-rule" )
            {
                lines.Add( new FiredRule()
                {
                    Context = elem.Attributes[ "context" ]?.Value ?? "##err",
                } );
            }

            if ( elem.LocalName == "suppressed-rule" )
            {
                lines.Add( new SuppressedRule()
                {
                    Context = elem.Attributes[ "context" ]?.Value ?? "##err",
                } );
            }
        }

        return new SchematronOutput()
        {
            Lines = lines.AsReadOnly(),
        };
    }


    /// <summary />
    private XsltExecutable LoadTransformer( OutputFormat format )
    {
        /*
         * 
         */
        string folder;
        string entryPoint;

        if ( format == OutputFormat.Xslt3 )
        {
            folder = "schxslt2";
            entryPoint = "transpile.xsl";
        }
        else
        {
            folder = "schxslt1";
            entryPoint = "pipeline-for-svrl.xsl";
        }


        /*
         * 
         */
        var resolver = new ResxResourceResolver();

        var src = resolver.resolve( new net.sf.saxon.lib.ResourceRequest()
        {
            uri = "resx://Lefty.Schematron/" + folder + "/" + entryPoint,
            baseUri = "resx://Lefty.Schematron/" + folder + "/" + entryPoint,
            relativeUri = "./" + entryPoint,
            entityName = "",
            nature = "",
            publicId = "",
            purpose = "",
            requestedEncoding = "utf-8",
            streamable = false,
            uriIsNamespace = false,
        } );

        if ( src == null )
            throw new FileNotFoundException( $"Resx {folder}/{entryPoint}" );


        /*
         * 
         */
        var processor = new Processor();
        var compiler = processor.newXsltCompiler();
        compiler.setResourceResolver( resolver );


        /*
         * 
         */
        var xslt = compiler.compile( src );

        return xslt;
    }


    /// <summary />
    private string ApplyTransform( Source src, XsltExecutable xslt )
    {
        var transformer = xslt.load30();

        using ( var jsw = new java.io.StringWriter() )
        {
            var serializer = xslt.getProcessor().newSerializer( jsw );

            transformer.applyTemplates( src, serializer );

            return jsw.toString();
        }
    }
}