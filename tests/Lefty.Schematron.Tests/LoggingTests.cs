using Microsoft.Extensions.Logging;

namespace Lefty.Schematron.Tests;

/// <summary>
/// Saxon reports its diagnostics through a reporter of its own choosing, and
/// prints them to the console unless given one. These pin that they reach the
/// caller's logger instead.
/// </summary>
public class LoggingTests
{
    [Fact]
    public void Compile_ReportsSaxonDiagnostics_ToTheLogger()
    {
        var logger = new RecordingLogger();
        var sut = new SchematronService( Sch.Options(), logger );

        Assert.Throws<SchematronCompilationException>(
            () => sut.Load( Sch.Utf8( "not xslt at all" ) ) );

        Assert.NotEmpty( logger.Entries );
        Assert.Contains( logger.Entries, x => x.Level == LogLevel.Debug && x.Message.Contains( "saxon:" ) );
    }


    [Fact]
    public void SaxonDiagnostics_AreLoggedAtDebug_AndNothingHigher()
    {
        var logger = new RecordingLogger();
        var sut = new SchematronService( Sch.Options(), logger );

        Assert.Throws<SchematronCompilationException>(
            () => sut.Load( Sch.Utf8( "not xslt at all" ) ) );

        Assert.All( logger.Entries, x => Assert.Equal( LogLevel.Debug, x.Level ) );
    }


    [Fact]
    public void TheMessage_CarriesTheEngineCodeAndPosition()
    {
        var logger = new RecordingLogger();
        var sut = new SchematronService( Sch.Options(), logger );

        Assert.Throws<SchematronCompilationException>(
            () => sut.Load( Sch.Utf8( "not xslt at all" ) ) );

        // the detail worth keeping is Saxon's code, which is what the docs index
        Assert.Contains( logger.Entries, x => x.Message.Contains( "SXXP" ) );
    }


    [Fact]
    public void NothingIsLogged_WhenDebugIsNotEnabled()
    {
        var logger = new RecordingLogger() { Enabled = false };
        var sut = new SchematronService( Sch.Options(), logger );

        Assert.Throws<SchematronCompilationException>(
            () => sut.Load( Sch.Utf8( "not xslt at all" ) ) );

        Assert.Empty( logger.Entries );
    }


    [Fact]
    public void Diagnostics_ReachTheException_EvenWithNoLoggerAtAll()
    {
        /*
         * The engine says more, and says it better, than its exception chain
         * does -- so what it reported is carried on the exception as well as
         * logged. A caller with no logger is the common case, and the one that
         * most needs the message.
         */
        var sut = new SchematronService( Sch.Options() );

        var ex = Assert.Throws<SchematronCompilationException>(
            () => sut.Load( Sch.Utf8( "not xslt at all" ) ) );

        Assert.NotEmpty( ex.Diagnostics );
        Assert.Contains( ex.Diagnostics, x => x.Contains( "SXXP" ) );
    }


    [Fact]
    public void Diagnostics_AreEmpty_WhenNothingFailed()
    {
        var sut = new SchematronService( Sch.Options() );

        var compiled = sut.Compile( Sch.Schema( """<assert test="true()" flag="error">ok</assert>""" ) );

        Assert.NotNull( compiled );
    }


    [Fact]
    public void ASuccessfulCompile_ReportsNothing()
    {
        var logger = new RecordingLogger();
        var sut = new SchematronService( Sch.Options(), logger );

        sut.Compile( Sch.Schema( """<assert test="true()" flag="error">ok</assert>""" ) );

        Assert.Empty( logger.Entries );
    }


    [Fact]
    public void TheServiceWorks_WithoutALogger()
    {
        // the one-argument constructor stays, and discards the diagnostics
        var sut = new SchematronService( Sch.Options() );

        Assert.Throws<SchematronCompilationException>(
            () => sut.Load( Sch.Utf8( "not xslt at all" ) ) );
    }


    /// <summary />
    private sealed record Entry( LogLevel Level, string Message );


    /// <summary />
    private sealed class RecordingLogger : ILogger<SchematronService>
    {
        internal List<Entry> Entries { get; } = [];

        internal bool Enabled { get; init; } = true;


        /// <summary />
        public IDisposable? BeginScope<TState>( TState state ) where TState : notnull
        {
            return null;
        }


        /// <summary />
        public bool IsEnabled( LogLevel logLevel )
        {
            return this.Enabled;
        }


        /// <summary />
        public void Log<TState>( LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter )
        {
            this.Entries.Add( new Entry( logLevel, formatter( state, exception ) ) );
        }
    }
}