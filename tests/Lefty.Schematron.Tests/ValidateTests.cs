using System.Xml;

namespace Lefty.Schematron.Tests;

/// <summary>
/// Covers <see cref="SchematronService.Validate(Stream)" />: XSD conformance
/// of the schema itself, plus the @id/@flag/@role policy layered on top.
/// </summary>
public class ValidateTests
{
    [Fact]
    public void Validate_IsValid_ForMinimalSchema()
    {
        var sut = Sch.Service();

        var result = sut.Validate( Sch.Schema( """<assert test="true()" flag="error">always</assert>""" ) );

        Assert.True( result.IsValid );
        Assert.Empty( result.Errors );
    }


    [Fact]
    public void Validate_ReportsError_WhenAssertHasNoTest()
    {
        // @test is required by schematron.xsd
        var sut = Sch.Service();

        var result = sut.Validate( Sch.Schema( """<assert flag="error">missing test</assert>""" ) );

        Assert.False( result.IsValid );
        Assert.NotEmpty( result.Errors );
    }


    [Fact]
    public void Validate_ReportsError_WhenIdMissing_AndIdRequired()
    {
        var sut = Sch.Service( idRequired: true );

        var result = sut.Validate( Sch.Schema( """<assert test="true()" flag="error">no id</assert>""" ) );

        Assert.False( result.IsValid );
        Assert.Contains( result.Errors, x => x.Message.Contains( "@id" ) );
    }


    [Fact]
    public void Validate_IsValid_WhenIdPresent_AndIdRequired()
    {
        var sut = Sch.Service( idRequired: true );

        var result = sut.Validate( Sch.Schema( """<assert id="a1" test="true()" flag="error">has id</assert>""" ) );

        Assert.True( result.IsValid );
    }


    [Fact]
    public void Validate_ReportsError_WhenFlagMissing_InFlagRequiredMode()
    {
        var sut = Sch.Service( severityMode: SeverityMode.FlagRequired );

        var result = sut.Validate( Sch.Schema( """<assert test="true()">no flag</assert>""" ) );

        Assert.False( result.IsValid );
        Assert.Contains( result.Errors, x => x.Message.Contains( "Required @flag" ) );
    }


    [Fact]
    public void Validate_ReportsError_WhenRolePresent_InFlagRequiredMode()
    {
        var sut = Sch.Service( severityMode: SeverityMode.FlagRequired );

        var result = sut.Validate( Sch.Schema( """<assert test="true()" flag="error" role="whatever">both</assert>""" ) );

        Assert.False( result.IsValid );
        Assert.Contains( result.Errors, x => x.Message.Contains( "Forbidden @role" ) );
    }


    [Fact]
    public void Validate_ReportsError_WhenFlagValueNotAccepted()
    {
        var sut = Sch.Service( acceptedFlags: [ "error" ] );

        var result = sut.Validate( Sch.Schema( """<assert test="true()" flag="catastrophe">bad flag</assert>""" ) );

        Assert.False( result.IsValid );
        Assert.Contains( result.Errors, x => x.Message.Contains( "Invalid @flag value 'catastrophe'" ) );
    }


    [Fact]
    public void Validate_IsValid_WhenRoleValueIsAccepted()
    {
        var sut = Sch.Service( severityMode: SeverityMode.RoleRequired, acceptedRoles: [ "fatal" ] );

        var result = sut.Validate( Sch.Schema( """<assert test="true()" role="fatal">good role</assert>""" ) );

        Assert.True( result.IsValid );
    }


    [Fact]
    public void Validate_ReportsError_WhenBothSpecified_InOneOfRequiredMode()
    {
        var sut = Sch.Service( severityMode: SeverityMode.OneOfRequired, acceptedRoles: [ "fatal" ] );

        var result = sut.Validate( Sch.Schema( """<assert test="true()" flag="error" role="fatal">both</assert>""" ) );

        Assert.False( result.IsValid );
        Assert.Contains( result.Errors, x => x.Message.Contains( "Only one of" ) );
    }


    [Fact]
    public void Validate_IsValid_WhenBothSpecified_InOneRequiredMode()
    {
        // OneRequired differs from OneOfRequired precisely here: both is fine
        var sut = Sch.Service( severityMode: SeverityMode.OneRequired, acceptedRoles: [ "fatal" ] );

        var result = sut.Validate( Sch.Schema( """<assert test="true()" flag="error" role="fatal">both</assert>""" ) );

        Assert.True( result.IsValid );
    }


    [Fact]
    public void Validate_ReportsError_WhenNeitherSpecified_InOneRequiredMode()
    {
        var sut = Sch.Service( severityMode: SeverityMode.OneRequired );

        var result = sut.Validate( Sch.Schema( """<assert test="true()">neither</assert>""" ) );

        Assert.False( result.IsValid );
        Assert.Contains( result.Errors, x => x.Message.Contains( "Required @flag or @role" ) );
    }


    [Fact]
    public void Validate_AppliesPolicy_ToReportElements()
    {
        // sch:report is held to the same policy as sch:assert
        var sut = Sch.Service( idRequired: true );

        var result = sut.Validate( Sch.Schema( """<report test="false()" flag="info">no id</report>""" ) );

        Assert.False( result.IsValid );
        Assert.Contains( result.Errors, x => x.Message.Contains( "@id" ) );
    }


    [Fact]
    public void Validate_ReportsLineNumber_ForPolicyFailure()
    {
        var sut = Sch.Service( idRequired: true );

        var result = sut.Validate( Sch.Schema( """<assert test="true()" flag="error">no id</assert>""" ) );

        var error = Assert.Single( result.Errors );
        Assert.True( error.LineNumber > 0, $"expected a line number, got {error.LineNumber}" );
        Assert.True( error.LinePosition > 0, $"expected a line position, got {error.LinePosition}" );
    }


    [Fact]
    public void Validate_Throws_WhenInputIsNotWellFormedXml()
    {
        var sut = Sch.Service();

        Assert.Throws<XmlException>( () => sut.Validate( Sch.Utf8( "<schema>" ) ) );
    }


    [Fact]
    public void Validate_ReportsEveryFailure_NotJustTheFirst()
    {
        var sut = Sch.Service( idRequired: true, severityMode: SeverityMode.FlagRequired );

        var result = sut.Validate( Sch.Schema(
            """
            <assert test="true()">first</assert>
                  <assert test="true()">second</assert>
            """ ) );

        // two asserts, each missing both @id and @flag
        Assert.False( result.IsValid );
        Assert.Equal( 4, result.Errors.Count );
    }
}