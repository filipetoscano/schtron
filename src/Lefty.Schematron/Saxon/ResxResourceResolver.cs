using javax.xml.transform;
using net.liberty_development.SaxonHE12s9apiExtensions;
using net.sf.saxon.lib;
using System.Reflection;

namespace Lefty.Schematron.Saxon;

/// <summary>
/// Resolves <c>resx://</c> URIs to embedded resources, which is how the
/// schxslt pipelines reach the files they import.
/// </summary>
/// <remarks>
/// The pipelines are shipped inside the assembly rather than on disk, so the
/// XSLT engine cannot open them itself. Handing it this resolver is what
/// makes a self-contained tool possible.
/// </remarks>
public class ResxResourceResolver : ResourceResolver
{
    private static readonly Assembly _assembly = typeof( ResxResourceResolver ).Assembly;


    /// <summary>
    /// Creates the resolver.
    /// </summary>
    public ResxResourceResolver()
    {
    }


    /// <summary>
    /// Resolves a request, returning null for anything which is not a
    /// <c>resx://</c> URI so that the engine falls back to its own handling.
    /// </summary>
    /// <param name="rr">The request, as the engine phrased it.</param>
    /// <returns>The embedded resource, or null.</returns>
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
        using var stream = _assembly.GetManifestResourceStream( resx );

        if ( stream == null )
            return null;

        using var reader = new StreamReader( stream );

        return reader.ReadToEnd();
    }
}