namespace Lefty.Schematron;

/// <summary />
public record SuccessfulReport : ISchematronLine
{
    /// <summary />
    public required string Id { get; init; }

    /// <summary />
    public required string Flag { get; init; }

    /// <summary />
    public required string Location { get; init; }

    /// <summary />
    public required string Test { get; init; }

    /// <summary />
    public required string Text { get; init; }
}
