namespace Lefty.Schematron.Gui;

/// <summary />
internal static class Extensions
{
    /// <summary />
    internal static string ToMessage( this XV value )
    {
        return value switch
        {
            XV.FileNotFound => "File not found",
            XV.InvalidXml => "File is an invalid XML file",
            XV.NotExpectedRoot => "File does not have expected schema",
            XV.Ok => "File is ok",
            _ => throw new InvalidOperationException(),
        };
    }
}