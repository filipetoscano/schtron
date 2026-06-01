namespace Lefty.Schematron;

/// <summary />
public record ActivePattern : ISchematronLine
{
    /// <summary />
    public required string Id { get; init; }

    /// <summary />
    public required string Name { get; init; }
}