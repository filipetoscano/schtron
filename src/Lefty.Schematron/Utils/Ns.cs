using System.Xml;

namespace Lefty.Schematron;

/// <summary />
internal class Ns
{
    internal static readonly Lazy<XmlNamespaceManager> _manager = new Lazy<XmlNamespaceManager>( Init );


    /// <summary />
    public static XmlNamespaceManager Manager
    {
        get => _manager.Value;
    }


    /// <summary />
    private static XmlNamespaceManager Init()
    {
        var ns = new XmlNamespaceManager( new NameTable() );
        ns.AddNamespace( "rdf", "http://www.w3.org/1999/02/22-rdf-syntax-ns#" );
        ns.AddNamespace( "sch", "http://purl.oclc.org/dsdl/schematron" );
        ns.AddNamespace( "svrl", "http://purl.oclc.org/dsdl/svrl" );

        return ns;
    }
}