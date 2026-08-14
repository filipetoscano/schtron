using System.Xml.Linq;

namespace Lefty.Schematron.Tests;

/// <summary>
/// Covers <see cref="SchematronService.Transform(Stream, Stream, OutputFormat)" />.
/// The compiled stylesheet is schxslt's output and changes between its
/// versions, so these assert the shape of the result rather than its text:
/// the contract that matters is "a stylesheet that Evaluate can run".
/// </summary>
public class TransformTests
{
    [Theory]
    [InlineData( OutputFormat.Xslt2 )]
    [InlineData( OutputFormat.Xslt3 )]
    public void Transform_ProducesAnXsltStylesheet( OutputFormat format )
    {
        var sut = Sch.Service();
        var output = new MemoryStream();

        sut.Transform( Sch.Schema( """<assert test="true()" flag="error">always</assert>""" ), output, format );

        var xslt = XDocument.Parse( Read( output ) );
        Assert.Equal( "http://www.w3.org/1999/XSL/Transform", xslt.Root!.Name.NamespaceName );

        // xsl:stylesheet and xsl:transform are synonyms; schxslt1 emits the
        // latter for XSLT 2, schxslt2 the former for XSLT 3
        Assert.Contains( xslt.Root.Name.LocalName, new[] { "stylesheet", "transform" } );
    }


    [Fact]
    public void Transform_LeavesTheOutputStreamOpen()
    {
        // documented contract: the streams belong to the caller
        var sut = Sch.Service();
        var output = new MemoryStream();

        sut.Transform( Sch.Schema( """<assert test="true()" flag="error">always</assert>""" ), output );

        Assert.True( output.CanRead );
        Assert.True( output.Length > 0 );
    }


    [Fact]
    public void Transform_CarriesTheAssertionThrough_ToTheStylesheet()
    {
        var sut = Sch.Service();
        var output = new MemoryStream();

        sut.Transform( Sch.Schema( """<assert test="true()" flag="error">a distinctive message</assert>""" ), output );

        Assert.Contains( "a distinctive message", Read( output ) );
    }


    /// <summary />
    private static string Read( MemoryStream stream )
    {
        stream.Position = 0;

        using var sr = new StreamReader( stream, leaveOpen: true );
        return sr.ReadToEnd();
    }
}