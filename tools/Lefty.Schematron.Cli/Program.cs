using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Spectre.Console;
using System.Reflection;
using System.Text.Json;

namespace Lefty.Schematron.Cli;

/// <summary />
[Command( "schtron", Description = "Swiss-knife for Schematron operations" )]
[Subcommand( typeof( PfxCommand ) )]
[Subcommand( typeof( SchematronCommand ) )]
[Subcommand( typeof( VersionCommand ) )]
[Subcommand( typeof( XmlCommand ) )]
[VersionOptionFromMember( MemberName = nameof( GetVersion ) )]
public class Program
{
    /// <summary />
    public static int Main( string[] args )
    {
        /*
         * Nothing is logged by default: the XSLT engine's diagnostics are the
         * only thing which reaches the log today, and the errors worth reading
         * are already on the error line. SCHTRON_LOG_LEVEL=Debug turns them on.
         */
        LogEventLevel level;

        try
        {
            level = Level();
        }
        catch ( ArgumentException ex )
        {
            AnsiConsole.MarkupLineInterpolated( $"[red]err[/]: {ex.Message}" );

            return 2;
        }

        /*
         * To stderr, always: 'sch transform' with no -o writes the transform to
         * stdout, and a diagnostic landing in the middle of it would corrupt a
         * pipeline. The template matches the ok:/err:/ftl: lines elsewhere.
         */
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is( level )
            .WriteTo.Console(
                standardErrorFromLevel: LogEventLevel.Verbose,
                outputTemplate: "{Level:w3}: {Message:lj}{NewLine}{Exception}" )
            .CreateLogger();

        try
        {
            return Run( args );
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }


    /// <summary />
    private static int Run( string[] args )
    {
        /*
         * A misspelt path or a stray comma in the options file is bad input like
         * any other, and earns an error line rather than the raw stack trace an
         * unhandled exception out of Main would produce.
         */
        SchematronServiceOptions opts;

        try
        {
            opts = LoadOptions();
        }
        catch ( Exception ex ) when ( ex is IOException or UnauthorizedAccessException
            or ArgumentException or NotSupportedException or JsonException )
        {
            AnsiConsole.MarkupLineInterpolated( $"[red]err[/]: SCHTRON_OPTIONS could not be read: {ex.Message}" );

            return 2;
        }


        /*
         * 
         */
        var app = new CommandLineApplication<Program>();

        var svc = new ServiceCollection();

        svc.AddLogging( b => b.AddSerilog( Log.Logger ) );
        svc.AddSingleton<SchematronServiceOptions>( opts );
        svc.AddTransient<ISchematronService, SchematronService>();

        var sp = svc.BuildServiceProvider();


        /*
         * 
         */
        try
        {
            app.Conventions
                .UseDefaultConventions()
                .UseConstructorInjection( sp );
        }
        catch ( Exception ex )
        {
            AnsiConsole.MarkupLine( $"[purple]ftl[/]: unhandled exception" );
            AnsiConsole.WriteException( ex );

            return 2;
        }


        /*
         * 
         */
        try
        {
            return app.Execute( args );
        }
        catch ( CommandParsingException ex )
        {
            /*
             * The base type, not UnrecognizedCommandParsingException: a value that
             * fails to parse into its option's type -- 'format -n abc', or a bogus
             * --format -- raises the base directly, and is bad input just the same,
             * so it earns an error line rather than a stack trace.
             */
            AnsiConsole.MarkupLineInterpolated( $"[red]err[/]: {ex.Message}" );

            return 2;
        }
        catch ( SchematronException ex )
        {
            /*
             * A schema which will not compile, or a document which is not XML,
             * is bad input rather than a defect: it earns an error line too.
             *
             * What the engine reported is preferred over its exception chain,
             * which says the same thing in Java's terms and only for whichever
             * fault happened to stop it.
             */
            AnsiConsole.MarkupLineInterpolated( $"[red]err[/]: {ex.Message}" );

            foreach ( var diagnostic in ex.Diagnostics )
                AnsiConsole.MarkupLineInterpolated( $"     {diagnostic}" );

            if ( ex.Diagnostics.Count == 0 )
            {
                var detail = Detail( ex );

                if ( detail != null )
                    AnsiConsole.MarkupLineInterpolated( $"     {detail}" );
            }

            return 2;
        }
        catch ( Exception ex )
        {
            AnsiConsole.MarkupLine( $"[purple]ftl[/]: unhandled exception" );
            AnsiConsole.WriteException( ex );

            return 2;
        }
    }


    /// <summary>
    /// The level named by SCHTRON_LOG_LEVEL, or Warning.
    /// </summary>
    private static LogEventLevel Level()
    {
        var value = Environment.GetEnvironmentVariable( "SCHTRON_LOG_LEVEL" );

        if ( string.IsNullOrWhiteSpace( value ) == true )
            return LogEventLevel.Warning;

        if ( Enum.TryParse<LogEventLevel>( value, ignoreCase: true, out var level ) == false )
            throw new ArgumentException( $"SCHTRON_LOG_LEVEL '{value}' is not one of: {string.Join( ", ", Enum.GetNames<LogEventLevel>() )}" );

        return level;
    }


    /// <summary>
    /// Reads the options named by SCHTRON_OPTIONS, or the defaults.
    /// </summary>
    private static SchematronServiceOptions LoadOptions()
    {
        var optsFile = Environment.GetEnvironmentVariable( "SCHTRON_OPTIONS" );

        if ( optsFile == null )
        {
            return new SchematronServiceOptions()
            {
                IdRequired = true,
                SeverityMode = SeverityMode.FlagRequired,
                AcceptedFlags = [ "fatal", "error", "warning", "info", "debug" ],
                AcceptedRoles = [],
            };
        }

        var json = File.ReadAllText( optsFile );

        return JsonSerializer.Deserialize<SchematronServiceOptions>( json )
            ?? throw new JsonException( "the options file contains no options" );
    }


    /// <summary>
    /// The innermost message in the chain, flattened onto one line.
    /// </summary>
    private static string? Detail( Exception ex )
    {
        var inner = ex.InnerException;

        if ( inner == null )
            return null;

        while ( inner.InnerException != null )
            inner = inner.InnerException;

        return string.Join( " ", inner.Message.Split( (char[]?) null, StringSplitOptions.RemoveEmptyEntries ) );
    }


    /// <summary />
    private static string GetVersion()
    {
        return typeof( Program )
            .Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()!
            .InformationalVersion;
    }


    /// <summary />
    public int OnExecute( CommandLineApplication app )
    {
        app.ShowHelp();
        return 1;
    }
}