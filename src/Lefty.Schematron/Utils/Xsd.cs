using System.Reflection;
using System.Xml;
using System.Xml.Schema;

namespace Lefty.Schematron;

/// <summary />
internal class Xsd
{
    internal static readonly Lazy<XmlSchemaSet> _manager = new Lazy<XmlSchemaSet>( Init );


    /// <summary />
    public static XmlSchemaSet Schemas
    {
        get => _manager.Value;
    }


    /// <summary />
    private static XmlSchemaSet Init()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var schemaSet = new XmlSchemaSet();

        using ( var schematronStream = assembly.GetManifestResourceStream( "Lefty.Schematron.Resources.schema.schematron.xsd" ) )
        {
            if ( schematronStream == null )
                throw new InvalidOperationException( "Embedded resource 'Resources.schema.schematron.xsd' not found in assembly." );

            using var schematronReader = XmlReader.Create( schematronStream );
            schemaSet.Add( "http://purl.oclc.org/dsdl/schematron", schematronReader );
        }

        using ( var svrlStream = assembly.GetManifestResourceStream( "Lefty.Schematron.Resources.schema.svrl.xsd" ) )
        {
            if ( svrlStream == null )
                throw new InvalidOperationException( "Embedded resource 'Resources.schema.svrl.xsd' not found in assembly." );

            using var svrlReader = XmlReader.Create( svrlStream );
            schemaSet.Add( "http://purl.oclc.org/dsdl/svrl", svrlReader );
        }

        schemaSet.Compile();

        return schemaSet;
    }
}