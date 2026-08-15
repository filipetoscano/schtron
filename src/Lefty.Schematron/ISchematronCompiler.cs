namespace Lefty.Schematron;

/// <summary>
/// Compiles Schematron schemas, and loads transforms compiled earlier, into a
/// reusable form.
/// </summary>
/// <remarks>
/// Separating compilation from evaluation is what lets a caller validating
/// many documents pay the cost of compiling a schema once. <see
/// cref="ISchematronService" /> is the convenience layer over this: it does
/// both in a single call, which is the right shape for one-off use and the
/// wrong one for a batch.
/// </remarks>
public interface ISchematronCompiler
{
    /// <summary>
    /// Compiles a Schematron schema into a reusable transform.
    /// </summary>
    /// <param name="schema">Schematron schema stream.</param>
    /// <param name="format">Output format.</param>
    /// <returns>The compiled schema.</returns>
    /// <exception cref="SchematronCompilationException">
    /// The schema could not be compiled.
    /// </exception>
    CompiledSchematron Compile( Stream schema, OutputFormat format = OutputFormat.Xslt3 );


    /// <summary>
    /// Compiles a Schematron schema into a reusable transform.
    /// </summary>
    /// <param name="schema">Schematron schema stream.</param>
    /// <param name="format">Output format.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The compiled schema.</returns>
    /// <exception cref="SchematronCompilationException">
    /// The schema could not be compiled.
    /// </exception>
    Task<CompiledSchematron> CompileAsync( Stream schema, OutputFormat format = OutputFormat.Xslt3, CancellationToken cancellationToken = default );


    /// <summary>
    /// Loads an XSL transform which was compiled earlier.
    /// </summary>
    /// <param name="transform">Transform stream.</param>
    /// <returns>The compiled schema.</returns>
    /// <exception cref="SchematronCompilationException">
    /// The transform could not be compiled.
    /// </exception>
    CompiledSchematron Load( Stream transform );


    /// <summary>
    /// Loads an XSL transform which was compiled earlier.
    /// </summary>
    /// <param name="transform">Transform stream.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The compiled schema.</returns>
    /// <exception cref="SchematronCompilationException">
    /// The transform could not be compiled.
    /// </exception>
    Task<CompiledSchematron> LoadAsync( Stream transform, CancellationToken cancellationToken = default );
}