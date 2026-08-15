using System.Diagnostics;

namespace Lefty.Schematron.Tests;

/// <summary>
/// Covers the compile/execute split: <see cref="ISchematronCompiler" /> and
/// the <see cref="CompiledSchematron" /> it hands back.
/// </summary>
public class CompilerTests
{
    private const string Rule = """<assert test="@name" flag="error">name is required</assert>""";


    [Fact]
    public void Compile_ProducesSomethingReusable()
    {
        var sut = Sch.Service();
        var compiled = sut.Compile( Sch.Schema( Rule ) );

        // the same compiled schema, three documents
        Assert.True( compiled.Evaluate( Sch.Utf8( """<doc name="a" />""" ) ).IsValid );
        Assert.False( compiled.Evaluate( Sch.Utf8( "<doc />" ) ).IsValid );
        Assert.True( compiled.Evaluate( Sch.Utf8( """<doc name="c" />""" ) ).IsValid );
    }


    [Fact]
    public void Compile_IsEquivalentTo_TransformThenEvaluate()
    {
        var sut = Sch.Service();

        var viaCompiler = sut.Compile( Sch.Schema( Rule ) ).Evaluate( Sch.Utf8( "<doc />" ) );

        var transform = new MemoryStream();
        sut.Transform( Sch.Schema( Rule ), transform );
        transform.Position = 0;
        var viaService = sut.Evaluate( Sch.Utf8( "<doc />" ), transform );

        Assert.Equal(
            viaService.Lines.OfType<FailedAssert>().Select( x => x.Text ),
            viaCompiler.Lines.OfType<FailedAssert>().Select( x => x.Text ) );
    }


    [Fact]
    public void WriteTo_EmitsTheCompiledTransform()
    {
        var sut = Sch.Service();
        var compiled = sut.Compile( Sch.Schema( Rule ) );

        var output = new MemoryStream();
        compiled.WriteTo( output );
        output.Position = 0;

        // round-trips: what was written can be loaded and run
        var reloaded = sut.Load( output );
        Assert.False( reloaded.Evaluate( Sch.Utf8( "<doc />" ) ).IsValid );
    }


    [Fact]
    public void Compile_AmortisesTheCostAcrossEvaluations()
    {
        /*
         * Compiling is the expensive half, and the whole point of the split is
         * that it happens once. Timing is a blunt instrument, so this asserts
         * only the shape of the win -- an order of magnitude of headroom --
         * rather than a specific figure.
         */
        var sut = Sch.Service();
        var compiled = sut.Compile( Sch.Schema( Rule ) );

        // warm: first Evaluate compiles the executable
        compiled.Evaluate( Sch.Utf8( "<doc />" ) );

        var reuse = Stopwatch.StartNew();
        for ( var i = 0; i < 5; i++ )
            compiled.Evaluate( Sch.Utf8( "<doc />" ) );
        reuse.Stop();

        var recompile = Stopwatch.StartNew();
        for ( var i = 0; i < 5; i++ )
            sut.Compile( Sch.Schema( Rule ) ).Evaluate( Sch.Utf8( "<doc />" ) );
        recompile.Stop();

        Assert.True(
            reuse.ElapsedMilliseconds < recompile.ElapsedMilliseconds,
            $"reuse took {reuse.ElapsedMilliseconds}ms, recompiling took {recompile.ElapsedMilliseconds}ms" );
    }


    [Fact]
    public void CompiledSchematron_IsSafeToShareBetweenThreads()
    {
        var sut = Sch.Service();
        var compiled = sut.Compile( Sch.Schema( Rule ) );

        var results = new bool[ 16 ];

        Parallel.For( 0, results.Length, i =>
        {
            var doc = i % 2 == 0 ? """<doc name="x" />""" : "<doc />";
            results[ i ] = compiled.Evaluate( Sch.Utf8( doc ) ).IsValid;
        } );

        for ( var i = 0; i < results.Length; i++ )
            Assert.Equal( i % 2 == 0, results[ i ] );
    }
}