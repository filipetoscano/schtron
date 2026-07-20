using com.sun.crypto.provider;
using Microsoft.Extensions.DependencyInjection;
using org.omg.CORBA;
using System.Text;

namespace Lefty.Schematron.Gui;

/// <summary />
public partial class AppForm : Form
{
    private readonly ISchematronService _ss;
    private string? _xslt;


    /// <summary />
    public AppForm()
    {
        InitializeComponent();


        /*
         * 
         */
        var svc = new ServiceCollection();

        svc.AddSingleton<SchematronServiceOptions>();
        svc.AddTransient<ISchematronService, SchematronService>();

        var sp = svc.BuildServiceProvider();

        _ss = sp.GetRequiredService<ISchematronService>();
    }



    /// <summary />
    private void btnLoadXslt_Click( object sender, EventArgs e )
    {
        this.openFileDialog.Filter = "Transforms|*.xslt|All files|*.*";
        var res = this.openFileDialog.ShowDialog();

        if ( res != DialogResult.OK )
            return;

        _xslt = File.ReadAllText( this.openFileDialog.FileName );
    }


    /// <summary />
    private void btnLoadXml_Click( object sender, EventArgs e )
    {
        this.openFileDialog.Filter = "XML files|*.xml|All files|*.*";
        var res = this.openFileDialog.ShowDialog();

        if ( res != DialogResult.OK )
            return;

        this.textXml.Text = File.ReadAllText( this.openFileDialog.FileName );
    }


    /// <summary />
    private void btnRun_Click( object sender, EventArgs e )
    {
        if ( _xslt == null )
        {
            MessageBox.Show( "XSLT file is required" );
            return;
        }

        if ( textXml.Text.Length == 0 )
        {
            MessageBox.Show( "XML is required" );
            return;
        }


        /*
         * 
         */
        using var xslt = AsStream( _xslt );
        using var xml = AsStream( textXml.Text.Trim() );

        var res = _ss.Evaluate( xml, xslt );


        /*
         * 
         */
        var sb = new StringBuilder();

        foreach ( var f in res.Lines.Where( x => x is FailedAssert ).OfType<FailedAssert>() )
        {
            sb.AppendFormat( "{0}\t{1}\n", f.Flag, f.Text );
        }

        this.textOutput.Text = sb.ToString();
    }


    /// <summary />
    private MemoryStream AsStream( string xml )
    {
        var bytes = Encoding.UTF8.GetBytes( xml );

        var ms = new MemoryStream( bytes );
        ms.Position = 0;

        return ms;
    }
}