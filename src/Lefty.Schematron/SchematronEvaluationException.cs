namespace Lefty.Schematron;

/// <summary>
/// Raised when a compiled transform cannot be run against a document, or when
/// the SVRL it produces cannot be read back.
/// </summary>
public class SchematronEvaluationException : SchematronException
{
    /// <summary>
    /// Creates the exception with a message.
    /// </summary>
    /// <param name="message">Description of the failure.</param>
    public SchematronEvaluationException( string message )
        : base( message )
    {
    }


    /// <summary>
    /// Creates the exception with a message and the underlying failure.
    /// </summary>
    /// <param name="message">Description of the failure.</param>
    /// <param name="innerException">The exception being wrapped.</param>
    public SchematronEvaluationException( string message, Exception innerException )
        : base( message, innerException )
    {
    }
}