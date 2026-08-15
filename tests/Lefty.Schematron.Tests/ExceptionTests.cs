namespace Lefty.Schematron.Tests;

/// <summary>
/// The library wraps Saxon, which is Java reached through IKVM. These pin the
/// boundary: a caller sees this library's exception types, never Saxon's.
/// </summary>
public class ExceptionTests
{
    [Fact]
    public void Compile_Throws_WhenSchemaIsNotWellFormed()
    {
        var sut = Sch.Service();

        var ex = Assert.Throws<SchematronCompilationException>(
            () => sut.Compile( Sch.Utf8( "<schema" ) ) );

        Assert.IsAssignableFrom<SchematronException>( ex );
    }


    [Fact]
    public void Compile_Throws_WhenSchemaIsNotSchematron()
    {
        /*
         * Worth knowing why this fails: the schxslt pipeline transpiles the
         * document happily -- it does not check conformance -- and what comes
         * out is not a stylesheet Saxon will accept. The error is real, but it
         * arrives from compiling the output rather than from inspecting the
         * input, so the message speaks about XSLT and not about Schematron.
         */
        var sut = Sch.Service();

        Assert.Throws<SchematronCompilationException>(
            () => sut.Compile( Sch.Utf8( "<not-a-schema />" ) ) );
    }


    [Fact]
    public void Load_Throws_WhenTransformIsNotXslt()
    {
        var sut = Sch.Service();

        Assert.Throws<SchematronCompilationException>(
            () => sut.Load( Sch.Utf8( "<not-a-transform />" ) ) );
    }


    [Fact]
    public void Evaluate_Throws_WhenDocumentIsNotWellFormed()
    {
        var sut = Sch.Service();
        var compiled = sut.Compile( Sch.Schema( """<assert test="true()" flag="error">ok</assert>""" ) );

        Assert.Throws<SchematronEvaluationException>(
            () => compiled.Evaluate( Sch.Utf8( "<doc" ) ) );
    }


    [Fact]
    public void Exceptions_KeepTheUnderlyingFailure_AsInnerException()
    {
        var sut = Sch.Service();

        var ex = Assert.Throws<SchematronCompilationException>(
            () => sut.Compile( Sch.Utf8( "<schema" ) ) );

        Assert.NotNull( ex.InnerException );
    }


    [Fact]
    public void NoSaxonTypeEscapes_ThroughThePublicApi()
    {
        /*
         * The failure this guards against is a Java exception reaching a
         * caller who would have to reference net.sf.saxon to catch it.
         */
        var sut = Sch.Service();

        var thrown = Record.Exception( () => sut.Compile( Sch.Utf8( "<schema" ) ) );

        Assert.NotNull( thrown );
        Assert.DoesNotContain( "saxon", thrown.GetType().FullName!, StringComparison.OrdinalIgnoreCase );
        Assert.DoesNotContain( "java", thrown.GetType().FullName!, StringComparison.OrdinalIgnoreCase );
    }
}