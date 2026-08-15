namespace Lefty.Schematron;

/// <summary>
/// The versions of the embedded schxslt pipelines, which are what actually
/// compile a schema. Reported by the tooling so that a transform can be
/// attributed to the implementation which produced it.
/// </summary>
public class DepVersions
{
    /// <summary>
    /// schxslt, used for <see cref="OutputFormat.Xslt2" />.
    /// </summary>
    public const string Schxslt1 = "1.10.1";

    /// <summary>
    /// schxslt2, used for <see cref="OutputFormat.Xslt3" />.
    /// </summary>
    public const string Schxslt2 = "1.11.2";
}