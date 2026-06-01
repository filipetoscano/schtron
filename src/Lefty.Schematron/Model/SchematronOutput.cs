namespace Lefty.Schematron;

/// <summary />
public class SchematronOutput
{
    /// <summary />
    public required IReadOnlyList<ISchematronLine> Lines { get; init; }
}