namespace Lefty.Schematron;

/// <summary />
public interface ISchematronService
{
    /// <summary>
    /// Validates a Schematron file.
    /// </summary>
    /// <param name="input">Input stream.</param>
    /// <returns>Validation result.</returns>
    ValidationResult Validate( Stream input );


    /// <summary>
    /// Transforms a Schematron file to an XSL transformation.
    /// </summary>
    /// <param name="input">Input stream.</param>
    /// <param name="output">Output stream.</param>
    /// <param name="format">Output format.</param>
    void Transform( Stream input, Stream output, OutputFormat format = OutputFormat.Xslt3 );


    /// <summary>
    /// Evaluates a document against an XSL transform.
    /// </summary>
    /// <param name="document">Document stream.</param>
    /// <param name="transform">Transform stream.</param>
    /// <returns>Schematron output.</returns>
    SchematronOutput Evaluate( Stream document, Stream transform );
}