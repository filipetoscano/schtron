using Spectre.Console;
using Spectre.Console.Json;
using System.Text.Json;

namespace Lefty.Schematron.Cli.Xml;

/// <summary />
internal static class JsonOut
{
    /// <summary>
    /// Writes <paramref name="value" /> as the command's JSON document.
    /// </summary>
    internal static void Write<T>( T value )
    {
        var json = JsonSerializer.Serialize( value );

        //
        // The pretty renderer hard-wraps at the console width, which drops literal
        // newlines inside string values: pleasant to read, and rejected by every
        // strict parser. Whatever isn't a terminal is something reading the
        // document rather than looking at it, and gets it verbatim.
        //
        if ( AnsiConsole.Profile.Out.IsTerminal == false )
        {
            Console.Out.WriteLine( json );
            return;
        }

        var jsonText = new JsonText( json );
        AnsiConsole.Write( jsonText );
    }
}
