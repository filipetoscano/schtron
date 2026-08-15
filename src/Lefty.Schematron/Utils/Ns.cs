using System.Xml;

namespace Lefty.Schematron;

/// <summary>
/// The namespace prefixes this library binds when it evaluates XPath --
/// <c>sch</c>, <c>svrl</c>, <c>rdf</c> and <c>ds</c>.
/// </summary>
public class Ns
{
    private static readonly Lazy<XmlNamespaceManager> _manager = new Lazy<XmlNamespaceManager>( Init );


    /// <summary>
    /// The shared manager.
    /// </summary>
    /// <remarks>
    /// One instance for the process. XmlNamespaceManager is not documented as
    /// thread-safe, so code which evaluates XPath concurrently should not use
    /// this one.
    /// </remarks>
    public static XmlNamespaceManager Manager
    {
        get => _manager.Value;
    }


    /// <summary>
    /// A namespace manager of its own, for a caller which cannot share the
    /// process-wide one -- XmlNamespaceManager is not documented as
    /// thread-safe, and building one costs four dictionary writes.
    /// </summary>
    internal static XmlNamespaceManager Create()
    {
        return Init();
    }


    /// <summary />
    private static XmlNamespaceManager Init()
    {
        var ns = new XmlNamespaceManager( new NameTable() );
        ns.AddNamespace( "rdf", "http://www.w3.org/1999/02/22-rdf-syntax-ns#" );
        ns.AddNamespace( "sch", "http://purl.oclc.org/dsdl/schematron" );
        ns.AddNamespace( "svrl", "http://purl.oclc.org/dsdl/svrl" );
        ns.AddNamespace( "ds", "http://www.w3.org/2000/09/xmldsig#" );

        return ns;
    }
}