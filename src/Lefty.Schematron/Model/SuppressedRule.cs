namespace Lefty.Schematron;

/// <summary />
public record SuppressedRule : ISchematronLine
{
    /// <summary />
    public required string Context { get; init; }
}