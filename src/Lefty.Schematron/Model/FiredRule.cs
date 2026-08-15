namespace Lefty.Schematron;

/// <summary>
/// A rule which matched, and whose assertions were therefore evaluated.
/// </summary>
public record FiredRule : ISchematronLine
{
    /// <summary>
    /// The rule's <c>@context</c> -- the XPath which selected the nodes it
    /// was applied to.
    /// </summary>
    public required string Context { get; init; }
}