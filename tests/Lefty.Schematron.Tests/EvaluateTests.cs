namespace Lefty.Schematron.Tests;

/// <summary>
/// Covers <see cref="SchematronService.Evaluate(Stream, Stream)" /> end to
/// end: a schema is compiled by Transform, then run against a document, and
/// the SVRL that comes back is parsed into the model.
/// </summary>
public class EvaluateTests
{
    [Fact]
    public void Evaluate_IsValid_WhenDocumentSatisfiesTheRule()
    {
        var sut = Sch.Service();
        var transform = Compile( sut, """<assert test="@name" flag="error">name is required</assert>""" );

        var output = sut.Evaluate( Sch.Utf8( """<doc name="present" />""" ), transform );

        Assert.True( output.IsValid );
        Assert.DoesNotContain( output.Lines, x => x is FailedAssert );
    }


    [Fact]
    public void Evaluate_ReportsFailedAssert_WhenDocumentViolatesTheRule()
    {
        var sut = Sch.Service();
        var transform = Compile( sut, """<assert test="@name" flag="error">name is required</assert>""" );

        var output = sut.Evaluate( Sch.Utf8( "<doc />" ), transform );

        Assert.False( output.IsValid );

        var failed = Assert.Single( output.Lines.OfType<FailedAssert>() );
        Assert.Equal( "name is required", failed.Text );
        Assert.Equal( "error", failed.Flag );
        Assert.Equal( "@name", failed.Test );
    }


    [Fact]
    public void Evaluate_CarriesTheAssertionId_ThroughToTheResult()
    {
        var sut = Sch.Service();
        var transform = Compile( sut, """<assert id="NAME-01" test="@name" flag="error">name is required</assert>""" );

        var output = sut.Evaluate( Sch.Utf8( "<doc />" ), transform );

        var failed = Assert.Single( output.Lines.OfType<FailedAssert>() );
        Assert.Equal( "NAME-01", failed.Id );
    }


    [Fact]
    public void Evaluate_ReportsSuccessfulReport_WhenReportTestMatches()
    {
        // sch:report is the inverse of sch:assert: it fires when the test is true
        var sut = Sch.Service();
        var transform = Compile( sut, """<report test="@deprecated" flag="warning">deprecated element</report>""" );

        var output = sut.Evaluate( Sch.Utf8( """<doc deprecated="yes" />""" ), transform );

        var report = Assert.Single( output.Lines.OfType<SuccessfulReport>() );
        Assert.Equal( "deprecated element", report.Text );

        // a report is not a failure
        Assert.True( output.IsValid );
    }


    [Fact]
    public void Evaluate_ReportsEveryFailedAssert()
    {
        var sut = Sch.Service();
        var transform = Compile( sut,
            """
            <assert test="@name" flag="error">name is required</assert>
                  <assert test="@kind" flag="error">kind is required</assert>
            """ );

        var output = sut.Evaluate( Sch.Utf8( "<doc />" ), transform );

        Assert.Equal( 2, output.Lines.OfType<FailedAssert>().Count() );
    }


    /// <summary>
    /// Compiles a schema to a stylesheet, positioned ready to be read.
    /// </summary>
    private static Stream Compile( SchematronService service, string rules )
    {
        var transform = new MemoryStream();
        service.Transform( Sch.Schema( rules ), transform );
        transform.Position = 0;

        return transform;
    }
}