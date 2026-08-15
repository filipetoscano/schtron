namespace Lefty.Schematron;

/// <summary>
/// Validates, compiles and evaluates Schematron in single calls.
/// </summary>
/// <remarks>
/// Each of <see cref="Transform" /> and <see cref="Evaluate" /> compiles on
/// every call. For repeated use against one schema, compile once through
/// <see cref="ISchematronCompiler" /> and reuse the result.
/// </remarks>
public interface ISchematronService
{
    /// <summary>
    /// Validates a Schematron file.
    /// </summary>
    /// <param name="input">Input stream.</param>
    /// <returns>Validation result.</returns>
    ValidationResult Validate( Stream input );


    /// <summary>
    /// Validates a Schematron file.
    /// </summary>
    /// <param name="input">Input stream.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Validation result.</returns>
    Task<ValidationResult> ValidateAsync( Stream input, CancellationToken cancellationToken = default );


    /// <summary>
    /// Transforms a Schematron file to an XSL transformation.
    /// </summary>
    /// <param name="input">Input stream.</param>
    /// <param name="output">Output stream.</param>
    /// <param name="format">Output format.</param>
    /// <exception cref="SchematronCompilationException">
    /// The schema could not be compiled.
    /// </exception>
    void Transform( Stream input, Stream output, OutputFormat format = OutputFormat.Xslt3 );


    /// <summary>
    /// Transforms a Schematron file to an XSL transformation.
    /// </summary>
    /// <param name="input">Input stream.</param>
    /// <param name="output">Output stream.</param>
    /// <param name="format">Output format.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="SchematronCompilationException">
    /// The schema could not be compiled.
    /// </exception>
    Task TransformAsync( Stream input, Stream output, OutputFormat format = OutputFormat.Xslt3, CancellationToken cancellationToken = default );


    /// <summary>
    /// Evaluates a document against an XSL transform.
    /// </summary>
    /// <param name="document">Document stream.</param>
    /// <param name="transform">Transform stream.</param>
    /// <returns>Schematron output.</returns>
    /// <exception cref="SchematronCompilationException">
    /// The transform could not be compiled.
    /// </exception>
    /// <exception cref="SchematronEvaluationException">
    /// The document could not be evaluated.
    /// </exception>
    SchematronOutput Evaluate( Stream document, Stream transform );


    /// <summary>
    /// Evaluates a document against an XSL transform.
    /// </summary>
    /// <param name="document">Document stream.</param>
    /// <param name="transform">Transform stream.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Schematron output.</returns>
    /// <exception cref="SchematronCompilationException">
    /// The transform could not be compiled.
    /// </exception>
    /// <exception cref="SchematronEvaluationException">
    /// The document could not be evaluated.
    /// </exception>
    Task<SchematronOutput> EvaluateAsync( Stream document, Stream transform, CancellationToken cancellationToken = default );
}