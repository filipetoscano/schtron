using System.Xml;
using System.Xml.Schema;

namespace Lefty.Schemas;

/// <summary />
public class Ubl21
{
    /// <summary />
    public static XmlSchemaSet Schemas { get => _ss.Value; }


    /// <summary />
    public static bool Is( XmlElement elem )
    {
        if ( elem.LocalName == "Invoice" && elem.NamespaceURI == "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2" )
            return true;

        if ( elem.LocalName == "CreditNote" && elem.NamespaceURI == "urn:oasis:names:specification:ubl:schema:xsd:CreditNote-2" )
            return true;

        return false;
    }


    /// <summary />
    private static readonly Lazy<XmlSchemaSet> _ss = new Lazy<XmlSchemaSet>( Load );


    /// <summary />
    private static XmlSchemaSet Load()
    {
        var ss = new XmlSchemaSet( new NameTable() );

        //
        // Every module the maindoc schemas (transitively) depend on is added
        // below, so imports/includes resolve from the set itself, by target
        // namespace. Nulling the resolver stops the schemaLocation hints from
        // reaching the file system or the network.
        //
        ss.XmlResolver = null;

        // common/
        ss.Add( Load( "ubl2_1.common.CCTS_CCT_SchemaModule-2.1.xsd" ) );
        ss.Add( Load( "ubl2_1.common.UBL-CoreComponentParameters-2.1.xsd" ) );
        ss.Add( Load( "ubl2_1.common.UBL-UnqualifiedDataTypes-2.1.xsd" ) );
        ss.Add( Load( "ubl2_1.common.UBL-QualifiedDataTypes-2.1.xsd" ) );
        ss.Add( Load( "ubl2_1.common.UBL-CommonBasicComponents-2.1.xsd" ) );
        ss.Add( Load( "ubl2_1.common.UBL-CommonAggregateComponents-2.1.xsd" ) );
        ss.Add( Load( "ubl2_1.common.UBL-xmldsig-core-schema-2.1.xsd" ) );
        ss.Add( Load( "ubl2_1.common.UBL-XAdESv132-2.1.xsd" ) );
        ss.Add( Load( "ubl2_1.common.UBL-XAdESv141-2.1.xsd" ) );
        ss.Add( Load( "ubl2_1.common.UBL-SignatureBasicComponents-2.1.xsd" ) );
        ss.Add( Load( "ubl2_1.common.UBL-SignatureAggregateComponents-2.1.xsd" ) );
        ss.Add( Load( "ubl2_1.common.UBL-CommonSignatureComponents-2.1.xsd" ) );
        ss.Add( Load( "ubl2_1.common.UBL-ExtensionContentDataType-2.1.xsd" ) );
        ss.Add( Load( "ubl2_1.common.UBL-CommonExtensionComponents-2.1.xsd" ) );

        // maindoc/
        ss.Add( Load( "ubl2_1.maindoc.UBL-CreditNote-2.1.xsd" ) );
        ss.Add( Load( "ubl2_1.maindoc.UBL-Invoice-2.1.xsd" ) );

        ss.Compile();

        return ss;
    }


    /// <summary />
    private static XmlSchema Load( string resx )
    {
        var res = "Lefty.Schemas." + resx;
        var mrs = typeof( Ubl21 ).Assembly.GetManifestResourceStream( res );

        if ( mrs is null )
            throw new InvalidOperationException( $"Embedded resource '{res}' not found." );

        //
        // The W3C xmldsig module carries an internal DTD subset (entities it
        // uses for its own namespace), so DTDs have to be parsed; the null
        // resolver keeps that to the internal subset -- no external entity,
        // and no external schemaLocation, is ever fetched.
        //
        var xrs = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Parse,
            XmlResolver = null,
        };

        using ( mrs )
        using ( var xr = XmlReader.Create( mrs, xrs ) )
        {
            var xs = XmlSchema.Read( xr, null );

            if ( xs is null )
                throw new InvalidOperationException( $"Embedded resource '{res}' is not a valid schema." );

            return xs;
        }
    }
}