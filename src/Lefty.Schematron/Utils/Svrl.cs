using System.Xml;

namespace Lefty.Schematron;

/// <summary>
/// Reads the SVRL a compiled transform emits into the model.
/// </summary>
internal static class Svrl
{
    /// <summary />
    internal static SchematronOutput Parse( string xml )
    {
        var ns = Ns.Create();
        var doc = new XmlDocument();

        try
        {
            doc.LoadXml( xml );
        }
        catch ( XmlException ex )
        {
            throw new SchematronEvaluationException( "The transform did not produce well-formed SVRL.", ex );
        }


        /*
         * An attribute the schema need not have supplied comes back null: the
         * model says what the SVRL said, and it is the caller which decides
         * whether an assertion with no id or flag matters to it.
         */
        var lines = new List<ISchematronLine>();

        foreach ( XmlElement elem in doc.SelectNodes( " /svrl:schematron-output/svrl:* ", ns )! )
        {
            switch ( elem.LocalName )
            {
                case "active-pattern":
                    lines.Add( new ActivePattern()
                    {
                        Id = Attr( elem, "id" ),
                        Name = elem.Attributes[ "name" ]?.Value,
                    } );
                    break;

                case "failed-assert":
                    lines.Add( new FailedAssert()
                    {
                        Id = Attr( elem, "id" ),
                        Flag = Attr( elem, "flag" ),
                        Location = Required( elem, "location" ),
                        Test = Required( elem, "test" ),
                        Text = Text( elem, ns ),
                    } );
                    break;

                case "successful-report":
                    lines.Add( new SuccessfulReport()
                    {
                        Id = Attr( elem, "id" ),
                        Flag = Attr( elem, "flag" ),
                        Location = Required( elem, "location" ),
                        Test = Required( elem, "test" ),
                        Text = Text( elem, ns ),
                    } );
                    break;

                case "fired-rule":
                    lines.Add( new FiredRule()
                    {
                        Context = Required( elem, "context" ),
                    } );
                    break;

                case "suppressed-rule":
                    lines.Add( new SuppressedRule()
                    {
                        Context = Required( elem, "context" ),
                    } );
                    break;
            }
        }

        return new SchematronOutput()
        {
            Lines = lines.AsReadOnly(),
        };
    }


    /// <summary>
    /// An attribute the schema may legitimately have omitted.
    /// </summary>
    private static string? Attr( XmlElement elem, string name )
    {
        return elem.Attributes[ name ]?.Value;
    }


    /// <summary>
    /// An attribute SVRL requires. Absent, the output is not SVRL, and saying
    /// so beats handing the caller a placeholder to discover later.
    /// </summary>
    private static string Required( XmlElement elem, string name )
    {
        return elem.Attributes[ name ]?.Value
            ?? throw new SchematronEvaluationException( $"The transform produced a <svrl:{elem.LocalName}> with no @{name}." );
    }


    /// <summary />
    private static string Text( XmlElement elem, XmlNamespaceManager ns )
    {
        return elem.SelectSingleNode( " svrl:text ", ns )?.InnerText
            ?? throw new SchematronEvaluationException( $"The transform produced a <svrl:{elem.LocalName}> with no <svrl:text>." );
    }
}