using javax.xml.transform;
using net.liberty_development.SaxonHE12s9apiExtensions;
using net.sf.saxon.lib;
using System.Reflection;

namespace Lefty.Schematron.Saxon;

/// <summary />
public class ResxResourceResolver : ResourceResolver
{
    private static readonly Assembly _assembly = typeof( ResxResourceResolver ).Assembly;


    /// <summary />
    public ResxResourceResolver()
    {
    }


    /// <summary />
    public Source? resolve( ResourceRequest rr )
    {
        /*
         * 
         */
        if ( string.IsNullOrEmpty( rr.baseUri ) == true )
            throw new NotSupportedException();


        /*
         * 
         */
        var uri = new Uri( new Uri( rr.baseUri ), rr.relativeUri );

        var resx = ToResx( uri );
        var xml = LoadStringFromResx( resx );

        if ( xml == null )
            return null;


        /*
         * 
         */
        var src = xml.AsSource();
        src.setSystemId( uri.ToString() );

        return src;
    }


    /// <summary />
    private static string ToResx( Uri uri )
    {
        var sb = new System.Text.StringBuilder();

        // .Host lower-cases the value
        sb.Append( "Lefty.Schematron" );

        sb.Append( "." );

        foreach ( var s in uri.Segments )
        {
            if ( s == "/" )
            {
                sb.Append( "Resources." );
                continue;
            }

            if ( s.EndsWith( "/" ) == true )
            {
                sb.Append( s[ 0..^1 ] );
                sb.Append( "." );
                continue;
            }

            sb.Append( s );
        }

        return sb.ToString();
    }


    /// <summary />
    private string? LoadStringFromResx( string resx )
    {
        var stream = _assembly.GetManifestResourceStream( resx );

        if ( stream == null )
            return null;

        var reader = new StreamReader( stream );

        if ( reader == null )
            return null;

        return reader.ReadToEnd();
    }
}