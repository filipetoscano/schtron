namespace Lefty.Schematron;

/// <summary />
public record ValidationError
{
    /// <summary />
    public required string Message { get; init; }

    /// <summary />
    public required int LineNumber { get; init; }

    /// <summary />
    public required int LinePosition { get; init; }
}