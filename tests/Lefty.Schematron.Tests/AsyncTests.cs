namespace Lefty.Schematron.Tests;

/// <summary>
/// The async overloads must agree with their synchronous counterparts, and
/// must honour cancellation at the points where honouring it is possible.
/// </summary>
public class AsyncTests
{
    private const string Rule = """<assert test="@name" flag="error">name is required</assert>""";


    [Fact]
    public async Task ValidateAsync_AgreesWithValidate()
    {
        var sut = Sch.Service( idRequired: true );

        var sync = sut.Validate( Sch.Schema( """<assert test="true()" flag="error">no id</assert>""" ) );
        var async = await sut.ValidateAsync( Sch.Schema( """<assert test="true()" flag="error">no id</assert>""" ), TestContext.Current.CancellationToken );

        Assert.Equal( sync.IsValid, async.IsValid );
        Assert.Equal( sync.Errors.Select( x => x.Message ), async.Errors.Select( x => x.Message ) );
    }


    [Fact]
    public async Task TransformAsync_AgreesWithTransform()
    {
        /*
         * Compared by what the stylesheets do, not by their text. schxslt
         * names its modes after Saxon's document numbering, which increments
         * for the life of a Processor, so two compilations of one schema are
         * equivalent without being identical.
         */
        var sut = Sch.Service();

        var sync = new MemoryStream();
        sut.Transform( Sch.Schema( Rule ), sync );
        sync.Position = 0;

        var async = new MemoryStream();
        await sut.TransformAsync( Sch.Schema( Rule ), async, cancellationToken: TestContext.Current.CancellationToken );
        async.Position = 0;

        var fromSync = sut.Evaluate( Sch.Utf8( "<doc />" ), sync );
        var fromAsync = sut.Evaluate( Sch.Utf8( "<doc />" ), async );

        Assert.Equal( fromSync.IsValid, fromAsync.IsValid );
        Assert.Equal(
            fromSync.Lines.OfType<FailedAssert>().Select( x => x.Text ),
            fromAsync.Lines.OfType<FailedAssert>().Select( x => x.Text ) );
    }


    [Fact]
    public async Task EvaluateAsync_AgreesWithEvaluate()
    {
        var sut = Sch.Service();

        var t1 = new MemoryStream();
        sut.Transform( Sch.Schema( Rule ), t1 );
        t1.Position = 0;
        var sync = sut.Evaluate( Sch.Utf8( "<doc />" ), t1 );

        var t2 = new MemoryStream();
        sut.Transform( Sch.Schema( Rule ), t2 );
        t2.Position = 0;
        var async = await sut.EvaluateAsync( Sch.Utf8( "<doc />" ), t2, TestContext.Current.CancellationToken );

        Assert.Equal( sync.IsValid, async.IsValid );
        Assert.Equal(
            sync.Lines.OfType<FailedAssert>().Select( x => x.Text ),
            async.Lines.OfType<FailedAssert>().Select( x => x.Text ) );
    }


    [Fact]
    public async Task CompileAsync_ProducesAWorkingSchema()
    {
        var sut = Sch.Service();

        var compiled = await sut.CompileAsync( Sch.Schema( Rule ), cancellationToken: TestContext.Current.CancellationToken );
        var output = await compiled.EvaluateAsync( Sch.Utf8( "<doc />" ), TestContext.Current.CancellationToken );

        Assert.False( output.IsValid );
    }


    [Fact]
    public async Task ValidateAsync_ObservesCancellation()
    {
        var sut = Sch.Service();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => sut.ValidateAsync( Sch.Schema( Rule ), cts.Token ) );
    }


    [Fact]
    public async Task CompileAsync_ObservesCancellation()
    {
        var sut = Sch.Service();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => sut.CompileAsync( Sch.Schema( Rule ), cancellationToken: cts.Token ) );
    }


    /// <summary />
    private static string Read( MemoryStream stream )
    {
        stream.Position = 0;

        using var sr = new StreamReader( stream, leaveOpen: true );
        return sr.ReadToEnd();
    }
}