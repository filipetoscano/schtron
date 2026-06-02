namespace Lefty.Schematron;

/// <summary />
public class SchematronOutput
{
    /// <summary />
    public bool IsValid
    {
        get
        {
            return this.Lines.Any( x => x is FailedAssert ) == false;
        }
    }


    /// <summary />
    public required IReadOnlyList<ISchematronLine> Lines { get; init; }
}