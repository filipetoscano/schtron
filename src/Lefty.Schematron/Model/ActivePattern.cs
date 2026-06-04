namespace Lefty.Schematron;

/// <summary />
public record ActivePattern : ISchematronLine
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Name of pattern.
    /// </summary>
    public string? Name { get; init; }
}