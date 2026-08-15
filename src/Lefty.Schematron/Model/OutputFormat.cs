namespace Lefty.Schematron;

/// <summary>
/// Which XSLT version a schema is compiled to, and therefore which schxslt
/// pipeline compiles it.
/// </summary>
public enum OutputFormat
{
    /// <summary>
    /// XSLT 2.0, via schxslt.
    /// </summary>
    Xslt2 = 2,

    /// <summary>
    /// XSLT 3.0, via schxslt2.
    /// </summary>
    Xslt3 = 3,
}