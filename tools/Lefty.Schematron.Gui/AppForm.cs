using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
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
    private void textXml_KeyUp( object sender, KeyEventArgs e )
    {
        if ( e.KeyCode == Keys.F5 )
            btnRun_Click( sender, e );
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


        UiLock();
        backgroundWorker.RunWorkerAsync();
    }


    /// <summary />
    private MemoryStream AsStream( string xml )
    {
        var bytes = Encoding.UTF8.GetBytes( xml );

        var ms = new MemoryStream( bytes );
        ms.Position = 0;

        return ms;
    }


    /// <summary />
    private void backgroundWorker_DoWork( object sender, DoWorkEventArgs e )
    {
        /*
         * 
         */
        using var xslt = AsStream( _xslt! );
        using var xml = AsStream( textXml.Text.Trim() );

        var res = _ss.Evaluate( xml, xslt );


        /*
         * 
         */
        var fa = res.Lines.Where( x => x is FailedAssert ).OfType<FailedAssert>();

        var nrErrors = fa.Where( x => x.Flag == "fatal" || x.Flag == "error" ).Count();
        var nrWarns = fa.Where( x => x.Flag == "warn" || x.Flag == "warning" ).Count();

        var isOk = nrErrors == 0;


        /*
         * 
         */
        var sb = new StringBuilder();

        foreach ( var f in fa )
            sb.AppendFormat( "{0}\t{1}{2}", f.Flag, f.Text, Environment.NewLine );


        /*
         * 
         */
        e.Result = new EvalResult()
        {
            IsValid = isOk,
            NrErrors = nrErrors,
            NrWarnings = nrWarns,
            Output = sb.ToString(),
        };
    }


    /// <summary />
    private void backgroundWorker_RunWorkerCompleted( object sender, RunWorkerCompletedEventArgs e )
    {
        UiUnlock();


        /*
         * 
         */
        if ( e.Cancelled == true )
            return;

        if ( e.Error != null )
        {
            MessageBox.Show( e.Error.ToString() );
            return;
        }


        /*
         * 
         */
        var res = (EvalResult) e.Result!;

        this.textOutput.Text = res.Output;
    }


    /// <summary />
    public class EvalResult
    {
        /// <summary />
        public required bool IsValid { get; set; }

        /// <summary />
        public required int NrErrors { get; set; }

        /// <summary />
        public required int NrWarnings { get; set; }

        /// <summary />
        public required string Output { get; set; }
    }


    /// <summary>
    /// Locks the UI while background thread is processing.
    /// </summary>
    private void UiLock()
    {
        UiLockSet( true );
    }


    /// <summary>
    /// Unlocks the UI after background thread finished processing.
    /// </summary>
    private void UiUnlock()
    {
        UiLockSet( false );
    }


    /// <summary />
    private void UiLockSet( bool uiLock )
    {
        var enabled = !uiLock;

        btnLoadXml.Enabled = enabled;
        btnLoadXslt.Enabled = enabled;
        btnRun.Enabled = enabled;

        textXml.Enabled = enabled;
        textOutput.Enabled = enabled;
    }
}