namespace Lefty.Schematron;

/// <summary>
/// What evaluating Schematron <c>assert</c> or <c>report</c> instructions,
/// how to handle 
/// </summary>
public enum SeverityMode
{
    /// <summary>
    /// The flag attribute is mandatory, and role is forbidden.
    /// </summary>
    FlagRequired,

    /// <summary>
    /// The role attribute is mandatory, and flag is forbidden.
    /// </summary>
    RoleRequired,

    /// <summary>
    /// One of the two attributes is required, but providing both is an error.
    /// </summary>
    OneOfRequired,

    /// <summary>
    /// At least one of the attributes is required, but providing both is also ok.
    /// </summary>
    OneRequired,

    /// <summary>
    /// Both flags are optional.
    /// </summary>
    Optional,
}