using Microsoft.Extensions.Logging;
using net.sf.saxon.lib;
using net.sf.saxon.s9api;

namespace Lefty.Schematron.Saxon;

/// <summary>
/// Hands Saxon's diagnostics to an <see cref="ILogger" /> instead of letting
/// them go to the console.
/// </summary>
/// <remarks>
/// Saxon reports every error it meets and then throws for the one that
/// stopped it, so without a reporter of our own the console gets the whole
/// list -- twice over, once from the compiler and once from the wrapper --
/// on top of whatever the caller chooses to say about the exception. The
/// list is worth keeping, just not worth printing, so it goes to Debug.
/// </remarks>
internal class LoggingErrorReporter : ErrorReporter
{
    private readonly ILogger _logger;
    private readonly List<string> _diagnostics = [];


    /// <summary />
    internal LoggingErrorReporter( ILogger logger )
    {
        _logger = logger;
    }


    /// <summary>
    /// What was reported, in the order it was reported, for attaching to the
    /// exception which follows. The engine says more here, and says it better,
    /// than the Java exception chain does.
    /// </summary>
    internal IReadOnlyList<string> Diagnostics
    {
        get => _diagnostics.AsReadOnly();
    }


    /// <summary />
    public void report( XmlProcessingError error )
    {
        var code = error.getErrorCode()?.ToString();
        var location = error.getLocation();

        var text = string.Concat(
            error.isWarning() == true ? "warning: " : "",
            code == null ? "" : code + " ",
            error.getMessage() );

        if ( location != null && location.getLineNumber() > 0 )
            text += $" (line {location.getLineNumber()}, column {location.getColumnNumber()})";

        _diagnostics.Add( text );

        if ( _logger.IsEnabled( LogLevel.Debug ) == true )
            _logger.LogDebug( "saxon: {Diagnostic}", text );
    }
}