namespace Lefty.Schematron;

/// <summary>
/// What checking a Schematron schema found -- faults in the schema itself,
/// not in any document it might be applied to.
/// </summary>
public record ValidationResult
{
    /// <summary>
    /// Whether the schema is free of faults.
    /// </summary>
    public required bool IsValid { get; init; }

    /// <summary>
    /// Every fault found, in the order they were met. Empty when the schema
    /// is valid.
    /// </summary>
    public required IReadOnlyList<ValidationError> Errors { get; init; }
}