namespace Lefty.Schematron;

/// <summary>
/// A pattern the engine began applying. Patterns bracket the rules that
/// follow them, so these mark out which part of the schema produced the lines
/// after this one.
/// </summary>
public record ActivePattern : ISchematronLine
{
    /// <summary>
    /// The pattern's <c>@id</c>, or null where the schema gave it none.
    /// </summary>
    public required string? Id { get; init; }

    /// <summary>
    /// The pattern's <c>@name</c>, or null where the schema gave it none.
    /// </summary>
    public string? Name { get; init; }
}