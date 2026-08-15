namespace Lefty.Schematron;

/// <summary>
/// What evaluating a document against a schema produced.
/// </summary>
public class SchematronOutput
{
    /// <summary>
    /// Whether the document satisfied the schema.
    /// </summary>
    /// <remarks>
    /// Only a <see cref="FailedAssert" /> makes a document invalid. A
    /// <see cref="SuccessfulReport" /> is the schema remarking on the
    /// document, not condemning it, however severe its flag.
    /// </remarks>
    public bool IsValid
    {
        get
        {
            return this.Lines.Any( x => x is FailedAssert ) == false;
        }
    }


    /// <summary>
    /// Every line the engine produced, in order.
    /// </summary>
    public required IReadOnlyList<ISchematronLine> Lines { get; init; }
}