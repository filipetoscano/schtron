using javax.xml.transform;
using Lefty.Schematron.Saxon;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using net.liberty_development.SaxonHE12s9apiExtensions;
using net.sf.saxon.s9api;

namespace Lefty.Schematron;

/// <summary>
/// Every call into Saxon goes through here, which is what keeps its Java
/// exception types from reaching a caller, and what makes the compiled
/// schxslt pipelines shareable.
/// </summary>
internal static class Xslt
{
    /*
     * XsltExecutable is immutable and safe to share between threads -- it is
     * the per-run transformer from load30() that is not. Compiling schxslt
     * dominates the cost of a transform, so the two pipelines are compiled at
     * most once for the lifetime of the process.
     */
    private static readonly Lazy<XsltExecutable> _xslt2 =
        new( () => Pipeline( "schxslt1", "pipeline-for-svrl.xsl" ), LazyThreadSafetyMode.ExecutionAndPublication );

    private static readonly Lazy<XsltExecutable> _xslt3 =
        new( () => Pipeline( "schxslt2", "transpile.xsl" ), LazyThreadSafetyMode.ExecutionAndPublication );


    /// <summary>
    /// Compiles a Schematron schema into an XSL transform, by running it
    /// through the schxslt pipeline for the requested format.
    /// </summary>
    internal static string Transpile( string schema, OutputFormat format, ILogger logger )
    {
        var pipeline = format == OutputFormat.Xslt3 ? _xslt3.Value : _xslt2.Value;
        var reporter = new LoggingErrorReporter( logger );

        try
        {
            return Apply( schema.AsSource(), pipeline, reporter );
        }
        catch ( Exception ex ) when ( ex is not SchematronException )
        {
            throw new SchematronCompilationException( "The Schematron schema could not be compiled to an XSL transform.", ex )
            {
                Diagnostics = reporter.Diagnostics,
            };
        }
    }


    /// <summary>
    /// Compiles an XSL transform, ready to be run against documents.
    /// </summary>
    internal static XsltExecutable Compile( string xslt, ILogger logger )
    {
        var reporter = new LoggingErrorReporter( logger );

        try
        {
            var processor = new Processor();
            var compiler = processor.newXsltCompiler();
            compiler.setErrorReporter( reporter );

            return compiler.compile( xslt.AsSource() );
        }
        catch ( Exception ex ) when ( ex is not SchematronException )
        {
            throw new SchematronCompilationException( "The XSL transform could not be compiled.", ex )
            {
                Diagnostics = reporter.Diagnostics,
            };
        }
    }


    /// <summary>
    /// Runs a compiled transform over a document, returning what it emits.
    /// </summary>
    internal static string Run( string document, XsltExecutable xslt, ILogger logger )
    {
        var reporter = new LoggingErrorReporter( logger );

        try
        {
            return Apply( document.AsSource(), xslt, reporter );
        }
        catch ( Exception ex ) when ( ex is not SchematronException )
        {
            throw new SchematronEvaluationException( "The document could not be evaluated against the transform.", ex )
            {
                Diagnostics = reporter.Diagnostics,
            };
        }
    }


    /// <summary>
    /// Loads one of the embedded schxslt pipelines.
    /// </summary>
    private static XsltExecutable Pipeline( string folder, string entryPoint )
    {
        var resolver = new ResxResourceResolver();
        var uri = "resx://Lefty.Schematron/" + folder + "/" + entryPoint;

        var src = resolver.resolve( new net.sf.saxon.lib.ResourceRequest()
        {
            uri = uri,
            baseUri = uri,
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
            throw new SchematronCompilationException( $"The embedded schxslt pipeline '{folder}/{entryPoint}' could not be resolved." );

        try
        {
            var processor = new Processor();
            var compiler = processor.newXsltCompiler();
            compiler.setResourceResolver( resolver );

            /*
             * The pipelines are embedded and compiled once, so there is no
             * caller to attribute a failure to and nobody's logger to reach:
             * one failing is a packaging fault, and the exception below says
             * so plainly.
             */
            compiler.setErrorReporter( new LoggingErrorReporter( NullLogger.Instance ) );

            return compiler.compile( src );
        }
        catch ( Exception ex ) when ( ex is not SchematronException )
        {
            throw new SchematronCompilationException( $"The embedded schxslt pipeline '{folder}/{entryPoint}' could not be compiled.", ex );
        }
    }


    /// <summary>
    /// Applies a transform to a source, serializing the result.
    /// </summary>
    private static string Apply( Source src, XsltExecutable xslt, LoggingErrorReporter reporter )
    {
        var transformer = xslt.load30();

        /*
         * The transformer reports separately from the compiler: a schema whose
         * sch:include cannot be found fails here, at run time, and would go to
         * the console rather than to the caller's log without this.
         */
        transformer.setErrorReporter( reporter );

        using ( var jsw = new java.io.StringWriter() )
        {
            var serializer = xslt.getProcessor().newSerializer( jsw );

            transformer.applyTemplates( src, serializer );

            return jsw.toString();
        }
    }
}