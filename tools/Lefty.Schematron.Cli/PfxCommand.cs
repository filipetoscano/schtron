using McMaster.Extensions.CommandLineUtils;
using Spectre.Console;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Lefty.Schematron.Cli;

/// <summary />
[Command( "pfx", Description = "Creates a self-signed PFX file" )]
public class PfxCommand
{
    /// <summary />
    public PfxCommand()
    {
    }


    /// <summary />
    [Argument( 0, Description = "Common name" )]
    [Required]
    public string? CommonName { get; set; }

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
         * 
         */
        string oname;

        if ( this.OutputFile != null )
            oname = this.OutputFile;
        else
            oname = this.CommonName!.Replace( " ", "_" ) + ".pfx";


        /*
         * Generate random RSA key pair
         */
        using var rsa = RSA.Create( 2048 );


        /*
         * Generate self-signed cert with given common name
         */
        var dn = new X500DistinguishedName( $"CN={this.CommonName}" );
        var req = new CertificateRequest( dn, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1 );
        req.CertificateExtensions.Add( new X509BasicConstraintsExtension( false, false, 0, false ) );
        req.CertificateExtensions.Add( new X509KeyUsageExtension( X509KeyUsageFlags.DigitalSignature, true ) );
        req.CertificateExtensions.Add( new X509SubjectKeyIdentifierExtension( req.PublicKey, false ) );

        var notBefore = DateTimeOffset.UtcNow;

        using var cert = req.CreateSelfSigned( notBefore, notBefore.AddYears( 10 ) );


        /*
         * Export PFX with private key marked exportable; optionally protect with password
         */
        var pfxBytes = cert.Export( X509ContentType.Pfx, this.PfxPassword );
        File.WriteAllBytes( oname, pfxBytes );

        AnsiConsole.MarkupLineInterpolated( $"[green]ok[/]: created {oname}" );

        return 0;
    }
}