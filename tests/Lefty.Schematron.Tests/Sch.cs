using System.Text;

namespace Lefty.Schematron.Tests;

/// <summary>
/// Builds the small Schematron/XML fixtures the tests run against, so that
/// each test only has to state the part it actually cares about.
/// </summary>
internal static class Sch
{
    /// <summary />
    internal const string Namespace = "http://purl.oclc.org/dsdl/schematron";


    /// <summary>
    /// Wraps one or more assert/report elements in a minimal, otherwise
    /// valid, schema. The rule matches the document element.
    /// </summary>
    internal static Stream Schema( string rules, string queryBinding = "xslt2" )
    {
        return Utf8( $"""
            <?xml version="1.0" encoding="utf-8"?>
            <schema xmlns="{Namespace}" queryBinding="{queryBinding}">
              <pattern>
                <rule context="/*">
                  {rules}
                </rule>
              </pattern>
            </schema>
            """ );
    }


    /// <summary />
    internal static Stream Utf8( string xml )
    {
        return new MemoryStream( Encoding.UTF8.GetBytes( xml ) );
    }


    /// <summary>
    /// Options with every severity check switched off, so a test that is
    /// interested in one rule is not tripped by the others.
    /// </summary>
    internal static SchematronServiceOptions Options(
        bool idRequired = false,
        SeverityMode severityMode = SeverityMode.Optional,
        IReadOnlyCollection<string>? acceptedFlags = null,
        IReadOnlyCollection<string>? acceptedRoles = null )
    {
        return new SchematronServiceOptions()
        {
            IdRequired = idRequired,
            SeverityMode = severityMode,
            AcceptedFlags = acceptedFlags ?? [ "fatal", "error", "warning", "info", "debug" ],
            AcceptedRoles = acceptedRoles ?? [],
        };
    }


    /// <summary />
    internal static SchematronService Service(
        bool idRequired = false,
        SeverityMode severityMode = SeverityMode.Optional,
        IReadOnlyCollection<string>? acceptedFlags = null,
        IReadOnlyCollection<string>? acceptedRoles = null )
    {
        return new SchematronService( Options( idRequired, severityMode, acceptedFlags, acceptedRoles ) );
    }
}