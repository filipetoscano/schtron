namespace Lefty.Schematron;

/// <summary>
/// A report which fired. <c>sch:report</c> is the inverse of
/// <c>sch:assert</c>: it speaks up when its test is <em>true</em>, and says
/// something about the document rather than condemning it -- which is why a
/// report does not make the output invalid.
/// </summary>
public record SuccessfulReport : ISchematronLine
{
    /// <summary>
    /// The report's <c>@id</c>, or null where the schema gave it none.
    /// </summary>
    public required string? Id { get; init; }

    /// <summary>
    /// The report's <c>@flag</c> -- its severity, in whatever vocabulary the
    /// schema uses -- or null where the schema gave it none.
    /// </summary>
    public required string? Flag { get; init; }

    /// <summary>
    /// XPath to the node the report fired on, as the engine reached it.
    /// </summary>
    public required string Location { get; init; }

    /// <summary>
    /// The XPath expression which tested true.
    /// </summary>
    public required string Test { get; init; }

    /// <summary>
    /// The report's message, for a human to read.
    /// </summary>
    public required string Text { get; init; }
}