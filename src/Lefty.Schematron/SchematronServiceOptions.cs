namespace Lefty.Schematron;

/// <summary>
/// The policy a schema is held to, over and above the Schematron schema
/// itself: whether assertions must be identified, and how they must declare
/// their severity.
/// </summary>
/// <remarks>
/// Schematron says nothing about any of this -- <c>@id</c>, <c>@flag</c> and
/// <c>@role</c> are all optional, and flags carry no defined vocabulary. A
/// house style that wants them is what these options express.
/// </remarks>
public class SchematronServiceOptions
{
    /// <summary>
    /// Whether every <c>sch:assert</c> and <c>sch:report</c> must carry an
    /// <c>@id</c>.
    /// </summary>
    public required bool IdRequired { get; init; }

    /// <summary>
    /// How assertions must declare their severity.
    /// </summary>
    public required SeverityMode SeverityMode { get; init; }

    /// <summary>
    /// The <c>@flag</c> values a schema may use. A flag outside this set is
    /// reported, whenever <see cref="SeverityMode" /> looks at flags at all.
    /// </summary>
    public required IReadOnlyCollection<string> AcceptedFlags { get; init; }

    /// <summary>
    /// The <c>@role</c> values a schema may use. A role outside this set is
    /// reported, whenever <see cref="SeverityMode" /> looks at roles at all.
    /// </summary>
    public required IReadOnlyCollection<string> AcceptedRoles { get; init; }
}