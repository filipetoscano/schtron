namespace Lefty.Schematron;

/// <summary>
/// A rule which matched a node an earlier rule had already claimed, and was
/// therefore not evaluated.
/// </summary>
/// <remarks>
/// Within a pattern, the first rule to match a node wins and the rest are
/// suppressed. That is Schematron working as specified, but it is also the
/// usual explanation for an assertion which mysteriously never fires.
/// </remarks>
public record SuppressedRule : ISchematronLine
{
    /// <summary>
    /// The rule's <c>@context</c> -- the XPath which would have selected the
    /// nodes it was applied to.
    /// </summary>
    public required string Context { get; init; }
}