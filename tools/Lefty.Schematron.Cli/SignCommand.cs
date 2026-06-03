using McMaster.Extensions.CommandLineUtils;
using Spectre.Console;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;

namespace Lefty.Schematron.Cli;

/// <summary />
[Command( "sign", Description = "Signs a Schematron file" )]
public class SignCommand
{
    /// <summary />
    public SignCommand()
    {
    }


    /// <summary />
    [Argument( 0, Description = "Input file" )]
    [Required]
    [FileExists]
    public string? InputFile { get; set; }

    /// <summary />
    [Option( "-p|--pfx-file", CommandOptionType.SingleValue, Description = "PFX file, with private key" )]
    [Required]
    [FileExists]
    public string? PfxFile { get; set; }

    /// <summary />
    [Option( "--pfx-password", CommandOptionType.SingleValue, Description = "PFX password" )]
    public string? PfxPassword { get; set; }

    /// <summary />
    [Option( "-o|--output-file", CommandOptionType.SingleValue, Description = "Output file" )]
    public string? OutputFile { get; set; }


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
         * Load certificate and private key
         */
        using var x509 = X509CertificateLoader.LoadPkcs12FromFile( this.PfxFile!, this.PfxPassword,
            X509KeyStorageFlags.EphemeralKeySet );

        using var rsa = x509.GetRSAPrivateKey();

        if ( rsa == null )
        {
            AnsiConsole.MarkupLine( "[red]err[/]: certificate does not contain an RSA private key" );
            return 2;
        }


        /*
         * Build enveloped XML digital signature (RSA-SHA256, Exc-C14N)
         */
        var signedXml = new SignedXml( doc );
        signedXml.SigningKey = rsa;
        signedXml.SignedInfo!.SignatureMethod = SignedXml.XmlDsigRSASHA256Url;

        var reference = new Reference();
        reference.Uri = "";
        reference.DigestMethod = SignedXml.XmlDsigSHA256Url;
        reference.AddTransform( new XmlDsigEnvelopedSignatureTransform() );
        reference.AddTransform( new XmlDsigExcC14NTransform() );
        signedXml.AddReference( reference );

        var keyInfo = new KeyInfo();
        keyInfo.AddClause( new KeyInfoX509Data( x509 ) );
        signedXml.KeyInfo = keyInfo;

        signedXml.ComputeSignature();


        /*
         * Insert ds:Signature as the first child of sch:schema
         */
        var root = doc.DocumentElement!;
        root.InsertBefore( doc.ImportNode( signedXml.GetXml(), deep: true ), root.FirstChild );


        /*
         * Write output
         */
        using var output = this.OutputFile != null
            ? (Stream) File.Create( this.OutputFile )
            : Console.OpenStandardOutput();

        using var writer = new StreamWriter( output, new UTF8Encoding( encoderShouldEmitUTF8Identifier: false ) );
        doc.Save( writer );

        if ( this.OutputFile != null )
            AnsiConsole.MarkupLineInterpolated( $"[green]ok[/]: signed {this.OutputFile}" );

        return 0;
    }
}