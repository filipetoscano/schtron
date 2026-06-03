using McMaster.Extensions.CommandLineUtils;
using Spectre.Console;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;

namespace Lefty.Schematron.Cli;

/// <summary />
[Command( "verify", Description = "Verifies digital signature on a Schematron file" )]
public class VerifyCommand
{
    /// <summary />
    public VerifyCommand()
    {
    }


    /// <summary />
    [Argument( 0, Description = "Input file" )]
    [Required]
    [FileExists]
    public string? InputFile { get; set; }

    /// <summary />
    [Option( "-c|--cert", CommandOptionType.SingleValue, Description = "Certificate file (.pem/.crt) to verify against; if omitted, uses certificate embedded in the signature" )]
    public string? CertFile { get; set; }


    /// <summary />
    public int OnExecute()
    {
        /*
         * Load document
         */
        var doc = new XmlDocument();
        doc.PreserveWhitespace = true;
        doc.Load( this.InputFile! );


        /*
         * Find ds:Signature
         */
        var sigNodes = doc.GetElementsByTagName( "Signature", "http://www.w3.org/2000/09/xmldsig#" );

        if ( sigNodes.Count == 0 )
        {
            AnsiConsole.MarkupLine( "[red]err[/]: no ds:Signature found in document" );
            return 1;
        }

        var signedXml = new SignedXml( doc );
        signedXml.LoadXml( (XmlElement) sigNodes[ 0 ]! );


        /*
         * Verify
         */
        bool valid;

        if ( this.CertFile != null )
        {
            using var cert = X509CertificateLoader.LoadCertificateFromFile( this.CertFile );
            valid = signedXml.CheckSignature( cert, verifySignatureOnly: true );
        }
        else
        {
            var embedded = ExtractCertFromKeyInfo( signedXml.KeyInfo );

            if ( embedded != null )
            {
                using ( embedded )
                    valid = signedXml.CheckSignature( embedded, verifySignatureOnly: true );
            }
            else
            {
                valid = signedXml.CheckSignature();
            }
        }


        /*
         *
         */
        if ( valid )
        {
            AnsiConsole.MarkupLine( "[green]ok[/]: signature is valid" );
            return 0;
        }

        AnsiConsole.MarkupLine( "[red]err[/]: signature is invalid" );
        return 1;
    }


    /// <summary />
    private static X509Certificate2? ExtractCertFromKeyInfo( KeyInfo keyInfo )
    {
        foreach ( KeyInfoClause clause in keyInfo )
        {
            if ( clause is KeyInfoX509Data x509Data )
                return x509Data.Certificates!.OfType<X509Certificate2>().FirstOrDefault();
        }

        return null;
    }
}