using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using System.Reflection;
using System.Text.Json;

namespace Lefty.Schematron.Cli;

/// <summary />
[Command( "schtron", Description = "Swiss-knife for Schematron operations" )]
[Subcommand( typeof( EvaluateCommand ) )]
[Subcommand( typeof( FormatCommand ) )]
[Subcommand( typeof( PfxCommand ) )]
[Subcommand( typeof( SignCommand ) )]
[Subcommand( typeof( TransformCommand ) )]
[Subcommand( typeof( ValidateCommand ) )]
[Subcommand( typeof( VerifyCommand ) )]
[Subcommand( typeof( VersionCommand ) )]
[VersionOptionFromMember( MemberName = nameof( GetVersion ) )]
public class Program
{
    /// <summary />
    public static int Main( string[] args )
    {
        /*
         * 
         */
        SchematronServiceOptions opts;
        var optsFile = Environment.GetEnvironmentVariable( "SCHTRON_OPTIONS" );

        if ( optsFile != null )
        {
            var json = File.ReadAllText( optsFile );

            opts = JsonSerializer.Deserialize<SchematronServiceOptions>( json )!;
        }
        else
        {
            opts = new SchematronServiceOptions()
            {
                IdRequired = true,
                SeverityMode = SeverityMode.FlagRequired,
                AcceptedFlags = [ "fatal", "error", "warning", "info", "debug" ],
                AcceptedRoles = [],
            };
        }


        /*
         * 
         */
        var app = new CommandLineApplication<Program>();

        var svc = new ServiceCollection();

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
        catch ( Exception ex )
        {
            AnsiConsole.MarkupLine( $"[purple]ftl[/]: unhandled exception" );
            AnsiConsole.WriteException( ex );

            return 2;
        }
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