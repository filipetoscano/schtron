using System.Text.Json;

namespace Lefty.Schematron.Tests;

/// <summary>
/// SchematronServiceOptions is public API on a package, and the CLI builds it
/// by deserializing JSON. These pin both.
/// </summary>
public class OptionsTests
{
    [Fact]
    public void Options_RoundTripThroughJson()
    {
        /*
         * IReadOnlySet would read better here, and System.Text.Json cannot
         * deserialize one -- which the CLI would discover at run time, on a
         * user's machine, reading their options file.
         */
        var json = """
            {
              "IdRequired": true,
              "SeverityMode": 0,
              "AcceptedFlags": [ "fatal", "error" ],
              "AcceptedRoles": [ "advisory" ]
            }
            """;

        var opts = JsonSerializer.Deserialize<SchematronServiceOptions>( json );

        Assert.NotNull( opts );
        Assert.True( opts.IdRequired );
        Assert.Equal( SeverityMode.FlagRequired, opts.SeverityMode );
        Assert.Equal( [ "fatal", "error" ], opts.AcceptedFlags );
        Assert.Equal( [ "advisory" ], opts.AcceptedRoles );
    }


    [Fact]
    public void Options_AreUsable_FromACollectionExpression()
    {
        // the shape the CLI's own defaults are written in
        var opts = new SchematronServiceOptions()
        {
            IdRequired = true,
            SeverityMode = SeverityMode.FlagRequired,
            AcceptedFlags = [ "fatal", "error", "warning", "info", "debug" ],
            AcceptedRoles = [],
        };

        Assert.Equal( 5, opts.AcceptedFlags.Count );
        Assert.Empty( opts.AcceptedRoles );
    }


    [Fact]
    public void AcceptedFlags_AreOnlyEnumeratedOnce()
    {
        /*
         * The service takes a set from these at construction. A sequence which
         * can only be walked once used to be a hazard here, since every
         * assertion in the schema searched it afresh.
         */
        var walks = 0;

        IEnumerable<string> Counting()
        {
            walks++;
            yield return "error";
        }

        var opts = new SchematronServiceOptions()
        {
            IdRequired = false,
            SeverityMode = SeverityMode.Optional,
            AcceptedFlags = Counting().ToList(),
            AcceptedRoles = [],
        };

        var sut = new SchematronService( opts );

        sut.Validate( Sch.Schema(
            """
            <assert test="true()" flag="error">one</assert>
                  <assert test="true()" flag="error">two</assert>
                  <assert test="true()" flag="error">three</assert>
            """ ) );

        Assert.Equal( 1, walks );
    }
}