using System.Xml;

namespace Lefty.Schematron;

/// <summary>
/// Reads the SVRL a compiled transform emits into the model.
/// </summary>
internal static class Svrl
{
    /*
     * Substituted when the SVRL omits an attribute the model requires. This
     * is existing behaviour, kept as-is and named in one place so that
     * modelling absence properly is a single change rather than twelve.
     */
    private const string Missing = "##err";


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
         *
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
                        Location = Attr( elem, "location" ),
                        Test = Attr( elem, "test" ),
                        Text = Text( elem, ns ),
                    } );
                    break;

                case "successful-report":
                    lines.Add( new SuccessfulReport()
                    {
                        Id = Attr( elem, "id" ),
                        Flag = Attr( elem, "flag" ),
                        Location = Attr( elem, "location" ),
                        Test = Attr( elem, "test" ),
                        Text = Text( elem, ns ),
                    } );
                    break;

                case "fired-rule":
                    lines.Add( new FiredRule()
                    {
                        Context = Attr( elem, "context" ),
                    } );
                    break;

                case "suppressed-rule":
                    lines.Add( new SuppressedRule()
                    {
                        Context = Attr( elem, "context" ),
                    } );
                    break;
            }
        }

        return new SchematronOutput()
        {
            Lines = lines.AsReadOnly(),
        };
    }


    /// <summary />
    private static string Attr( XmlElement elem, string name )
    {
        return elem.Attributes[ name ]?.Value ?? Missing;
    }


    /// <summary />
    private static string Text( XmlElement elem, XmlNamespaceManager ns )
    {
        return elem.SelectSingleNode( " svrl:text ", ns )?.InnerText ?? Missing;
    }
}