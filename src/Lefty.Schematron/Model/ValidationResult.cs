namespace Lefty.Schematron;

/// <summary />
public record ValidationResult
{
    /// <summary />
    public required bool IsValid { get; init; }

    /// <summary />
    public required IReadOnlyList<ValidationError> Errors { get; init; }
}