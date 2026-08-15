using net.sf.saxon.s9api;

namespace Lefty.Schematron;

/// <summary>
/// A Schematron schema which has been compiled to an XSL transform, ready to
/// be evaluated against any number of documents.
/// </summary>
/// <remarks>
/// <para>
/// Compiling is the expensive half of Schematron validation and does not
/// depend on the document being checked, so a caller validating a batch
/// should compile once and evaluate many times.
/// </para>
/// <para>
/// Instances are immutable and safe to share between threads.
/// </para>
/// </remarks>
public sealed class CompiledSchematron
{
    private readonly XsltExecutable _xslt;
    private readonly string _text;


    /// <summary />
    internal CompiledSchematron( string text )
    {
        /*
         * Compiled here rather than on first use, so that a stylesheet Saxon
         * will not accept is reported by the call that produced it instead of
         * by an Evaluate some distance away.
         */
        _xslt = Xslt.Compile( text );
        _text = text;
    }


    /// <summary>
    /// Evaluates a document against this schema.
    /// </summary>
    /// <param name="document">Document stream.</param>
    /// <returns>Schematron output.</returns>
    /// <exception cref="SchematronEvaluationException">
    /// The document could not be evaluated, or the transform did not produce
    /// well-formed SVRL.
    /// </exception>
    public SchematronOutput Evaluate( Stream document )
    {
        string xml;

        using ( var sr = new StreamReader( document, leaveOpen: true ) )
        {
            xml = sr.ReadToEnd();
        }

        return Evaluate( xml );
    }


    /// <summary>
    /// Evaluates a document against this schema.
    /// </summary>
    /// <param name="document">Document stream.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Schematron output.</returns>
    /// <remarks>
    /// Reading the document is asynchronous; the transform itself is not, as
    /// Saxon offers no asynchronous entry point. The token is observed before
    /// the transform starts but cannot interrupt one already running.
    /// </remarks>
    /// <exception cref="SchematronEvaluationException">
    /// The document could not be evaluated, or the transform did not produce
    /// well-formed SVRL.
    /// </exception>
    public async Task<SchematronOutput> EvaluateAsync( Stream document, CancellationToken cancellationToken = default )
    {
        string xml;

        using ( var sr = new StreamReader( document, leaveOpen: true ) )
        {
            xml = await sr.ReadToEndAsync( cancellationToken ).ConfigureAwait( false );
        }

        cancellationToken.ThrowIfCancellationRequested();

        return Evaluate( xml );
    }


    /// <summary>
    /// Writes the compiled transform, as XSLT.
    /// </summary>
    /// <param name="output">Output stream.</param>
    /// <remarks>
    /// The stream belongs to the caller and is written but never closed.
    /// </remarks>
    public void WriteTo( Stream output )
    {
        using ( var sw = new StreamWriter( output, leaveOpen: true ) )
        {
            sw.Write( _text );
        }
    }


    /// <summary>
    /// Writes the compiled transform, as XSLT.
    /// </summary>
    /// <param name="output">Output stream.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <remarks>
    /// The stream belongs to the caller and is written but never closed.
    /// </remarks>
    public async Task WriteToAsync( Stream output, CancellationToken cancellationToken = default )
    {
        var sw = new StreamWriter( output, leaveOpen: true );

        await using ( sw.ConfigureAwait( false ) )
        {
            await sw.WriteAsync( _text.AsMemory(), cancellationToken ).ConfigureAwait( false );
        }
    }


    /// <summary />
    private SchematronOutput Evaluate( string xml )
    {
        var svrl = Xslt.Run( xml, _xslt );

        return Svrl.Parse( svrl );
    }
}