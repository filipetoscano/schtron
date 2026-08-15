namespace Lefty.Schematron;

/// <summary>
/// Raised when a Schematron schema, or an XSL transform, cannot be compiled.
/// </summary>
/// <remarks>
/// This covers a malformed or non-conforming schema, an XSLT which Saxon
/// refuses, and the schxslt pipelines themselves failing to load from the
/// embedded resources.
/// </remarks>
public class SchematronCompilationException : SchematronException
{
    /// <summary>
    /// Creates the exception with a message.
    /// </summary>
    /// <param name="message">Description of the failure.</param>
    public SchematronCompilationException( string message )
        : base( message )
    {
    }


    /// <summary>
    /// Creates the exception with a message and the underlying failure.
    /// </summary>
    /// <param name="message">Description of the failure.</param>
    /// <param name="innerException">The exception being wrapped.</param>
    public SchematronCompilationException( string message, Exception innerException )
        : base( message, innerException )
    {
    }
}