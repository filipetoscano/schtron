using javax.xml.transform;
using net.liberty_development.SaxonHE12s9apiExtensions;
using net.sf.saxon.lib;

namespace Lefty.Schematron.Saxon;

/// <summary />
public class FilesystemResourceResolver : ResourceResolver
{
    private readonly string _basePath;


    /// <summary />
    public FilesystemResourceResolver( string basePath )
    {
        _basePath = basePath;
    }


    /// <summary />
    public Source? resolve( ResourceRequest rr )
    {
        /*
         * 
         */
        string path;

        if ( string.IsNullOrEmpty( rr.baseUri ) == false )
        {
            var baseUri = new Uri( rr.baseUri );
            var resolvedUri = new Uri( baseUri, rr.relativeUri );
            path = resolvedUri.LocalPath;
        }
        else
        {
            var relativePath = rr.relativeUri.Replace( '/', Path.DirectorySeparatorChar );
            path = Path.GetFullPath( Path.Combine( _basePath, relativePath ) );
        }


        /*
         * 
         */
        if ( File.Exists( path ) == true )
        {
            var xml = File.ReadAllText( path );
            var src = xml.AsSource();
            src.setSystemId( new Uri( path ).AbsoluteUri );

            return src;
        }

        return null;
    }
}