namespace Lefty.Schematron;

/// <summary />
public record ActivePattern : ISchematronLine
{
    /// <summary />
    public required string Id { get; init; }

    /// <summary />
    public string? Name { get; init; }
}