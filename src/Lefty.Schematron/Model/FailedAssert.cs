namespace Lefty.Schematron;

/// <summary>
/// An assertion which did not hold: the document reached a rule whose
/// <c>sch:assert</c> tested false, which is Schematron's way of saying the
/// document is wrong.
/// </summary>
public record FailedAssert : ISchematronLine
{
    /// <summary>
    /// The assertion's <c>@id</c>, or null where the schema gave it none.
    /// </summary>
    public required string? Id { get; init; }

    /// <summary>
    /// The assertion's <c>@flag</c> -- its severity, in whatever vocabulary
    /// the schema uses -- or null where the schema gave it none.
    /// </summary>
    public required string? Flag { get; init; }

    /// <summary>
    /// XPath to the node which failed, as the engine reached it.
    /// </summary>
    public required string Location { get; init; }

    /// <summary>
    /// The XPath expression which tested false.
    /// </summary>
    public required string Test { get; init; }

    /// <summary>
    /// The assertion's message, for a human to read.
    /// </summary>
    public required string Text { get; init; }
}