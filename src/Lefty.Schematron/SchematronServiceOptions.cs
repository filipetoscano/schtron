namespace Lefty.Schematron;

/// <summary />
public class SchematronServiceOptions
{
    /// <summary />
    public required bool IdRequired { get; set; }

    /// <summary />
    public required SeverityMode SeverityMode { get; set; } = SeverityMode.FlagRequired;

    /// <summary />
    public required IEnumerable<string> AcceptedFlags { get; set; } = [ "fatal", "error", "warning", "info", "debug" ];

    /// <summary />
    public required IEnumerable<string> AcceptedRoles { get; set; } = [];
}