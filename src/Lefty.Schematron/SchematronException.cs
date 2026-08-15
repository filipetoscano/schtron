namespace Lefty.Schematron;

/// <summary>
/// Base type for every error raised by this library.
/// </summary>
/// <remarks>
/// The XSLT engine underneath is Saxon, reached through IKVM, so its failures
/// arrive as Java exception types. Those are wrapped rather than propagated:
/// a caller should never have to reference <c>net.sf.saxon</c> to handle a
/// bad schema. The original is always kept as the inner exception.
/// </remarks>
public class SchematronException : Exception
{
    /// <summary>
    /// Creates the exception with a message.
    /// </summary>
    /// <param name="message">Description of the failure.</param>
    public SchematronException( string message )
        : base( message )
    {
    }


    /// <summary>
    /// Creates the exception with a message and the underlying failure.
    /// </summary>
    /// <param name="message">Description of the failure.</param>
    /// <param name="innerException">The exception being wrapped.</param>
    public SchematronException( string message, Exception innerException )
        : base( message, innerException )
    {
    }
}