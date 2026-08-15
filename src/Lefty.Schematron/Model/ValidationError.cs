namespace Lefty.Schematron;

/// <summary>
/// A fault found in a Schematron schema, positioned in the file it came from.
/// </summary>
public record ValidationError
{
    /// <summary>
    /// What is wrong, for a human to read.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// One-based line, or -1 where the source did not carry position.
    /// </summary>
    public required int LineNumber { get; init; }

    /// <summary>
    /// One-based column, or -1 where the source did not carry position.
    /// </summary>
    public required int LinePosition { get; init; }
}