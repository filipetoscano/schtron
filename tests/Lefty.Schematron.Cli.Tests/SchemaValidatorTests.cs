using Lefty.Schematron.Cli.Xml;

namespace Lefty.Schematron.Cli.Tests;

/// <summary>
/// Covers the decision SchemaValidator makes before anything else happens:
/// which schema applies -- the UBL set, a named file, the document's own
/// hints, or none -- and what each of those does when it goes wrong.
/// </summary>
public class SchemaValidatorTests
{
    private const string Note = """
        <?xml version="1.0" encoding="utf-8"?>
        <note xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
          <to>you</to>
        </note>
        """;

    private const string NoteXsd = """
        <?xml version="1.0" encoding="utf-8"?>
        <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema">
          <xs:element name="note">
            <xs:complexType>
              <xs:sequence>
                <xs:element name="to" type="xs:string" />
              </xs:sequence>
            </xs:complexType>
          </xs:element>
        </xs:schema>
        """;

    private const string NamespacedXsd = """
        <?xml version="1.0" encoding="utf-8"?>
        <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema"
                   targetNamespace="urn:note" xmlns="urn:note" elementFormDefault="qualified">
          <xs:element name="note">
            <xs:complexType>
              <xs:sequence>
                <xs:element name="to" type="xs:string" />
              </xs:sequence>
            </xs:complexType>
          </xs:element>
        </xs:schema>
        """;


    [Fact]
    public void Validate_Fails_WhenBothUblAndSchemaAreNamed()
    {
        using var ws = new Workspace();
        var xml = ws.Write( "doc.xml", Note );
        var xsd = ws.Write( "note.xsd", NoteXsd );

        var check = SchemaValidator.Validate( xml, xsd, isUbl: true );

        Assert.Equal( "options --ubl and --schema are mutually exclusive", check.Failure );
    }


    [Fact]
    public void Validate_Skips_WhenNoSchemaIsNamedAndDocumentPointsAtNone()
    {
        /*
         * Skipped is neither valid nor invalid: nothing was checked, and the
         * caller is the one who decides whether that is good enough.
         */
        using var ws = new Workspace();
        var xml = ws.Write( "doc.xml", Note );

        var check = SchemaValidator.Validate( xml, null, isUbl: false );

        Assert.True( check.Skipped );
        Assert.Null( check.Failure );
        Assert.Empty( check.Errors );
    }


    [Fact]
    public void Validate_ReportsError_WhenDocumentIsNotWellFormed()
    {
        // a document which doesn't parse is invalid, not a Failure
        using var ws = new Workspace();
        var xml = ws.Write( "doc.xml", "<note><to>you</note>" );
        var xsd = ws.Write( "note.xsd", NoteXsd );

        var check = SchemaValidator.Validate( xml, xsd, isUbl: false );

        Assert.Null( check.Failure );
        Assert.False( check.Skipped );
        Assert.NotEmpty( check.Errors );
        Assert.True( check.Errors[ 0 ].LineNumber > 0 );
    }


    [Fact]
    public void Validate_Fails_WhenDocumentHasNoRootElement()
    {
        using var ws = new Workspace();
        var xml = ws.Write( "doc.xml", "" );
        var xsd = ws.Write( "note.xsd", NoteXsd );

        var check = SchemaValidator.Validate( xml, xsd, isUbl: false );

        Assert.NotEmpty( check.Errors );
    }


    [Fact]
    public void Validate_PassesACleanDocument_AgainstANamedSchema()
    {
        using var ws = new Workspace();
        var xml = ws.Write( "doc.xml", Note );
        var xsd = ws.Write( "note.xsd", NoteXsd );

        var check = SchemaValidator.Validate( xml, xsd, isUbl: false );

        Assert.Null( check.Failure );
        Assert.False( check.Skipped );
        Assert.Empty( check.Errors );
    }


    [Fact]
    public void Validate_ReportsError_WhenDocumentBreaksTheNamedSchema()
    {
        using var ws = new Workspace();
        var xml = ws.Write( "doc.xml", "<note><wrong>you</wrong></note>" );
        var xsd = ws.Write( "note.xsd", NoteXsd );

        var check = SchemaValidator.Validate( xml, xsd, isUbl: false );

        Assert.Null( check.Failure );
        Assert.NotEmpty( check.Errors );
    }


    [Fact]
    public void Validate_Fails_WhenSchemaDoesNotDeclareTheRootElement()
    {
        /*
         * The trap this guards: an undeclared root element raises a validation
         * *warning*, not an error, so without this check the document would be
         * pronounced valid without a single rule having been applied to it.
         */
        using var ws = new Workspace();
        var xml = ws.Write( "doc.xml", "<other><to>you</to></other>" );
        var xsd = ws.Write( "note.xsd", NoteXsd );

        var check = SchemaValidator.Validate( xml, xsd, isUbl: false );

        Assert.Equal( "schema does not declare root element 'other'", check.Failure );
        Assert.Empty( check.Errors );
    }


    [Fact]
    public void Validate_Fails_WhenSchemaFileIsNotWellFormed()
    {
        using var ws = new Workspace();
        var xml = ws.Write( "doc.xml", Note );
        var xsd = ws.Write( "note.xsd", "<xs:schema" );

        var check = SchemaValidator.Validate( xml, xsd, isUbl: false );

        Assert.NotNull( check.Failure );
        Assert.StartsWith( "schema file is not well-formed", check.Failure );
    }


    [Fact]
    public void Validate_Fails_WhenSchemaFileIsNotASchema()
    {
        using var ws = new Workspace();
        var xml = ws.Write( "doc.xml", Note );
        var xsd = ws.Write( "note.xsd", """<?xml version="1.0"?><not-a-schema />""" );

        var check = SchemaValidator.Validate( xml, xsd, isUbl: false );

        Assert.NotNull( check.Failure );
    }


    [Fact]
    public void Validate_Fails_WhenDocumentIsNotUbl()
    {
        /*
         * Same trap as the named-schema case: the UBL set declares nothing
         * about <note>, so validating against it would find no fault at all.
         */
        using var ws = new Workspace();
        var xml = ws.Write( "doc.xml", Note );

        var check = SchemaValidator.Validate( xml, null, isUbl: true );

        Assert.Equal( "document is not a UBL 2.1 Invoice or CreditNote", check.Failure );
    }


    [Fact]
    public void Validate_FollowsNoNamespaceSchemaLocation_OnDisk()
    {
        using var ws = new Workspace();
        ws.Write( "note.xsd", NoteXsd );
        var xml = ws.Write( "doc.xml", """
            <?xml version="1.0" encoding="utf-8"?>
            <note xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
                  xsi:noNamespaceSchemaLocation="note.xsd">
              <to>you</to>
            </note>
            """ );

        var check = SchemaValidator.Validate( xml, null, isUbl: false );

        Assert.Null( check.Failure );
        Assert.False( check.Skipped );
        Assert.Empty( check.Errors );
    }


    [Fact]
    public void Validate_ReportsError_WhenSchemaLocationSchemaIsBroken()
    {
        using var ws = new Workspace();
        ws.Write( "note.xsd", NoteXsd );
        var xml = ws.Write( "doc.xml", """
            <?xml version="1.0" encoding="utf-8"?>
            <note xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
                  xsi:noNamespaceSchemaLocation="note.xsd">
              <wrong>you</wrong>
            </note>
            """ );

        var check = SchemaValidator.Validate( xml, null, isUbl: false );

        Assert.NotEmpty( check.Errors );
    }


    [Fact]
    public void Validate_RefusesToFetchARemoteSchema()
    {
        /*
         * Validating a file is never a reason to reach out to the network.
         */
        using var ws = new Workspace();
        var xml = ws.Write( "doc.xml", """
            <?xml version="1.0" encoding="utf-8"?>
            <note xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
                  xsi:noNamespaceSchemaLocation="https://example.invalid/note.xsd">
              <to>you</to>
            </note>
            """ );

        var check = SchemaValidator.Validate( xml, null, isUbl: false );

        Assert.NotNull( check.Failure );
        Assert.Contains( "refusing to fetch remote schema", check.Failure );
    }


    [Fact]
    public void Validate_TakesTheLocationHalf_OfEachSchemaLocationPair()
    {
        /*
         * xsi:schemaLocation is a whitespace-separated list of pairs, and it is
         * the second of each -- the location -- which is followed. Pointing the
         * namespace half at a URL must not be mistaken for a remote fetch.
         */
        using var ws = new Workspace();
        ws.Write( "note.xsd", NamespacedXsd );

        var xml = ws.Write( "doc.xml", """
            <?xml version="1.0" encoding="utf-8"?>
            <note xmlns="urn:note"
                  xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
                  xsi:schemaLocation="urn:note note.xsd">
              <to>you</to>
            </note>
            """ );

        var check = SchemaValidator.Validate( xml, null, isUbl: false );

        Assert.Null( check.Failure );
        Assert.False( check.Skipped );
        Assert.Empty( check.Errors );
    }


    [Fact]
    public void Validate_Fails_WhenSchemaLocationDeclaresNothingForTheRoot()
    {
        /*
         * The third face of the same trap, and the narrowest. It needs the
         * hints to load a schema for one namespace while the root sits in
         * another: nothing then describes the root, which the validator can
         * only report as a warning, so the root has to be asked directly.
         */
        using var ws = new Workspace();
        ws.Write( "note.xsd", NamespacedXsd );
        var xml = ws.Write( "doc.xml", """
            <?xml version="1.0" encoding="utf-8"?>
            <other xmlns="urn:elsewhere"
                   xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
                   xsi:schemaLocation="urn:note note.xsd">
              <to>you</to>
            </other>
            """ );

        var check = SchemaValidator.Validate( xml, null, isUbl: false );

        Assert.NotNull( check.Failure );
        Assert.Contains( "declares no schema for root element", check.Failure );
    }


    [Fact]
    public void Validate_ReportsError_WhenTheRootIsUndeclaredByALoadedSchema()
    {
        /*
         * The neighbouring case, kept because the distinction is easy to lose:
         * when the hint covers the root's own namespace, an undeclared root is
         * an error outright, and never reaches the warning check above.
         */
        using var ws = new Workspace();
        ws.Write( "note.xsd", NoteXsd );
        var xml = ws.Write( "doc.xml", """
            <?xml version="1.0" encoding="utf-8"?>
            <other xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
                   xsi:noNamespaceSchemaLocation="note.xsd">
              <to>you</to>
            </other>
            """ );

        var check = SchemaValidator.Validate( xml, null, isUbl: false );

        Assert.Null( check.Failure );
        Assert.Contains( check.Errors, x => x.Message.Contains( "'other' element is not declared" ) );
    }
}