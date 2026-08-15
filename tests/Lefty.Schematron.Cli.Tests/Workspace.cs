namespace Lefty.Schematron.Cli.Tests;

/// <summary>
/// SchemaValidator works in file paths -- it validates over the file rather
/// than over a parsed document, which is what keeps line and column on every
/// error -- so its tests need real files. Each one gets a directory of its
/// own, removed when the test ends.
/// </summary>
internal sealed class Workspace : IDisposable
{
    private readonly string _root;


    /// <summary />
    internal Workspace()
    {
        _root = Path.Combine( Path.GetTempPath(), "schtron-tests", Path.GetRandomFileName() );

        Directory.CreateDirectory( _root );
    }


    /// <summary>
    /// Writes a file into the workspace, returning its full path.
    /// </summary>
    internal string Write( string name, string content )
    {
        var path = Reserve( name );

        File.WriteAllText( path, content );

        return path;
    }


    /// <summary>
    /// A path inside the workspace, whether or not anything is written to it.
    /// </summary>
    internal string Reserve( string name )
    {
        return Path.Combine( _root, name );
    }


    /// <summary />
    public void Dispose()
    {
        try
        {
            Directory.Delete( _root, recursive: true );
        }
        catch ( IOException )
        {
        }
        catch ( UnauthorizedAccessException )
        {
        }
    }
}