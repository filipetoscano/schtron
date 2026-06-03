using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using System.Reflection;

namespace Lefty.Schematron.Cli;

/// <summary />
[Command( "schtron", Description = "Swiss-knife for Schematron operations" )]
[Subcommand( typeof( EvaluateCommand ) )]
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
        var app = new CommandLineApplication<Program>();

        var svc = new ServiceCollection();

        svc.AddSingleton<SchematronServiceOptions>();
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
        catch ( UnrecognizedCommandParsingException ex )
        {
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