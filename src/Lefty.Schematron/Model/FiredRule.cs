namespace Lefty.Schematron;

/// <summary />
public record FiredRule : ISchematronLine
{
    /// <summary />
    public required string Context { get; init; }
}