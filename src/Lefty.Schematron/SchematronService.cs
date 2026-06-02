using javax.xml.transform;
using Lefty.Schematron.Saxon;
using net.liberty_development.SaxonHE12s9apiExtensions;
using net.sf.saxon.s9api;
using System.Xml;
using System.Xml.Schema;

namespace Lefty.Schematron;

/// <summary />
public partial class SchematronService : ISchematronService
{
    private readonly XmlNamespaceManager _ns;


    /// <summary />
    public SchematronService()
    {
        _ns = Ns.Manager;
    }


    /// <inheritdoc />
    public ValidationResult Validate( Stream input )
    {
        /*
         * 
         */
        var doc = new XmlDocument();
        doc.PreserveWhitespace = true;

        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null
        };

        using ( var reader = XmlReader.Create( input, settings ) )
        {
            doc.Load( reader );
        }


        /*
         * 
         */
        var messages = new List<string>();

        doc.Schemas = Xsd.Schemas;
        doc.Validate( ( sender, e ) =>
        {
            if ( e.Severity == XmlSeverityType.Error )
                messages.Add( e.Message );
        } );

        messages.TrimExcess();


        /*
         * 
         */
        return new ValidationResult()
        {
            IsValid = messages.Count() == 0,
            Errors = messages.AsReadOnly(),
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