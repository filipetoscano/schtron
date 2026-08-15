using Lefty.Schemas;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Text;
using System.Xml;
using System.Xml.Schema;

namespace Lefty.Schematron.Gui;

/// <summary />
public partial class AppForm : Form
{
    private readonly ISchematronService _ss;
    private string? _xslt;
    private FindDialog? _find;
    private int _selStart;
    private int _selLength;
    private bool _selFocused;


    /// <summary />
    public AppForm()
    {
        InitializeComponent();


        /*
         * 
         */
        textXml.AllowDrop = true;
        textXml.DragEnter += AppForm_DragEnter;
        textXml.DragDrop += AppForm_DragDrop;

        textOutput.AllowDrop = true;
        textOutput.DragEnter += AppForm_DragEnter;
        textOutput.DragDrop += AppForm_DragDrop;


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
    protected override bool ProcessCmdKey( ref Message msg, Keys keyData )
    {
        if ( keyData == ( Keys.Control | Keys.F ) )
        {
            ShowFind();
            return true;
        }

        if ( keyData == Keys.F3 )
        {
            if ( _find == null )
                ShowFind();
            else
                _find.FindNext();

            return true;
        }

        return base.ProcessCmdKey( ref msg, keyData );
    }


    /// <summary>
    /// Finds the text box that currently has focus. The text boxes sit inside nested
    /// containers, so the form's own ActiveControl is never the text box itself.
    /// </summary>
    private static TextBox? FocusedTextBox( Control parent )
    {
        foreach ( Control c in parent.Controls )
        {
            if ( c.ContainsFocus == false )
                continue;

            if ( c is TextBox tb )
                return tb;

            return FocusedTextBox( c );
        }

        return null;
    }


    /// <summary>
    /// Opens (or re-focuses) the find dialog, bound to whichever text box has focus.
    /// </summary>
    private void ShowFind()
    {
        var target = FocusedTextBox( this ) ?? textXml;

        if ( _find != null && _find.IsDisposed == false )
        {
            if ( _find.Tag as TextBox != target )
            {
                _find.Close();
            }
            else
            {
                _find.Prime();
                _find.Activate();
                return;
            }
        }

        _find = new FindDialog( target ) { Tag = target };
        _find.FormClosed += ( s, e ) => _find = null;
        _find.Show( this );
        _find.Prime();
    }


    /// <summary />
    private void btnLoadXslt_Click( object sender, EventArgs e )
    {
        this.openFileDialog.Filter = "Transforms|*.xslt|All files|*.*";
        var res = this.openFileDialog.ShowDialog();

        if ( res != DialogResult.OK )
            return;

        LoadSchematron( this.openFileDialog.FileName );
    }


    /// <summary />
    private void LoadSchematron( string fileName )
    {
        var res = LoadXml( fileName, XT.Namespace, [ XT.Root1, XT.Root2 ] );

        if ( res.xv != XV.Ok )
        {
            var msg = res.xv.ToMessage();
            MessageBox.Show( msg, "schtron", MessageBoxButtons.OK, MessageBoxIcon.Error );

            // A previous load may have left the button green: put it back to the
            // themed default, so it never claims a transform is loaded while
            // _xslt is null. Assigning BackColor is what turned the visual style
            // off, so both have to be restored, in this order.
            btnLoadXslt.BackColor = SystemColors.Control;
            btnLoadXslt.UseVisualStyleBackColor = true;

            _xslt = null;
            return;
        }

        btnLoadXslt.BackColor = K.Ok;
        _xslt = res.xml;
    }


    /// <summary />
    private static (XV xv, string? xml) LoadXml( string fileName, string @ns, string[] localNames )
    {
        /*
         * 
         */
        if ( File.Exists( fileName ) == false )
            return (XV.FileNotFound, null);

        var xml = File.ReadAllText( fileName );


        /*
         * 
         */
        var doc = new XmlDocument();

        try
        {
            doc.LoadXml( xml );
        }
        catch
        {
            return (XV.InvalidXml, null);
        }


        /*
         * 
         */
        if ( doc.DocumentElement?.NamespaceURI != ns )
            return (XV.NotExpectedRoot, null);

        if ( localNames.Contains( doc.DocumentElement!.LocalName ) == false )
            return (XV.NotExpectedRoot, null);

        return (XV.Ok, xml);
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

        // Snapshot the text here, on the UI thread: touching a control from the
        // worker is an illegal cross-thread call, and throws outright whenever a
        // debugger is attached.
        backgroundWorker.RunWorkerAsync( textXml.Text );
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
         * Load
         */
        var sourceXml = (string) e.Argument!;

        var doc = new XmlDocument();

        try
        {
            doc.LoadXml( sourceXml );
        }
        catch ( XmlException ex )
        {
            e.Result = new EvalResult()
            {
                IsValid = false,
                NrErrors = 1,
                NrWarnings = 0,
                Output = ex.Message,
            };

            return;
        }

        if ( Ubl21.Is( doc.DocumentElement! ) == true )
        {
            var ne = 0;
            var se = new StringBuilder();

            /*
             * Validating over the source text, rather than the XmlDocument that was
             * just parsed, is what keeps line/position on each error -- the parsed
             * document no longer carries them. A StringReader (over a stream) also
             * means an encoding declaration can't fight the text we already hold.
             */
            var xrs = new XmlReaderSettings
            {
                ValidationType = ValidationType.Schema,
                Schemas = Ubl21.Schemas,
                XmlResolver = null,
            };

            xrs.ValidationEventHandler += ( s, ev ) =>
            {
                if ( ev.Severity != XmlSeverityType.Error )
                    return;

                ne++;

                se.AppendFormat( "error\t({0},{1}) {2}{3}",
                    ev.Exception?.LineNumber ?? 0,
                    ev.Exception?.LinePosition ?? 0,
                    ev.Message,
                    Environment.NewLine );
            };

            try
            {
                using ( var sr = new StringReader( sourceXml ) )
                using ( var xr = XmlReader.Create( sr, xrs ) )
                {
                    while ( xr.Read() )
                        ;
                }
            }
            catch ( XmlException ex )
            {
                //
                // This reader is stricter than the XmlDocument above -- a DTD, for
                // one, is prohibited here but accepted by LoadXml -- so a document
                // can reach this point and still fail to parse. Report it the way
                // every other bad document is reported, not as a stack trace.
                //
                ne++;

                se.AppendFormat( "error\t({0},{1}) {2}{3}",
                    ex.LineNumber, ex.LinePosition, ex.Message, Environment.NewLine );
            }

            if ( ne > 0 )
            {
                e.Result = new EvalResult()
                {
                    IsValid = false,
                    NrErrors = ne,
                    NrWarnings = 0,
                    Output = se.ToString(),
                };

                return;
            }
        }


        /*
         * 
         */
        using var xslt = AsStream( _xslt! );
        using var xml = AsStream( sourceXml.Trim() );

        var res = _ss.Evaluate( xml, xslt );


        /*
         * Every failed assert has to land in one bucket or the other, otherwise a
         * document that fails only on asserts carrying no @flag -- which arrive as
         * the '##err' sentinel -- counts as zero errors, and the panel turns green
         * while listing the failures. Anything not recognised as advisory is an
         * error: an unknown flag is a reason to shout, not to stay quiet.
         */
        var fa = res.Lines.OfType<FailedAssert>();

        var nrWarns = fa.Where( x => IsAdvisory( x.Flag ) ).Count();
        var nrErrors = fa.Count() - nrWarns;

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


    /// <summary>
    /// Whether a failed assert's <c>@flag</c> is advisory rather than a failure.
    /// Everything else -- including a missing or unrecognised flag -- is an error.
    /// </summary>
    private static bool IsAdvisory( string? flag )
    {
        if ( flag == null )
            return false;

        return flag.Equals( "warn", StringComparison.OrdinalIgnoreCase )
            || flag.Equals( "warning", StringComparison.OrdinalIgnoreCase )
            || flag.Equals( "info", StringComparison.OrdinalIgnoreCase )
            || flag.Equals( "debug", StringComparison.OrdinalIgnoreCase );
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

        this.textOutput.BackColor = ToColor( res );
        this.textOutput.Text = res.Output;
    }


    /// <summary />
    private Color ToColor( EvalResult res )
    {
        if ( res.NrErrors > 0 )
            return K.Error;

        if ( res.NrWarnings > 0 )
            return K.Warning;

        return K.Ok;
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

        /*
         * Disabling a text box takes the focus away and drops the caret, so where
         * the user was sitting when they hit F5 has to be remembered here and put
         * back once the run is over.
         */
        if ( uiLock == true )
        {
            _selStart = textXml.SelectionStart;
            _selLength = textXml.SelectionLength;
            _selFocused = textXml.ContainsFocus;
        }

        btnLoadXml.Enabled = enabled;
        btnLoadXslt.Enabled = enabled;
        btnRun.Enabled = enabled;

        textXml.Enabled = enabled;
        textOutput.Enabled = enabled;

        if ( uiLock == false )
        {
            textXml.SelectionStart = _selStart;
            textXml.SelectionLength = _selLength;

            if ( _selFocused == true )
                textXml.Focus();

            textXml.ScrollToCaret();
        }
    }


    /// <summary />
    private void AppForm_DragEnter( object? sender, DragEventArgs e )
    {
        if ( e.Data!.GetDataPresent( DataFormats.FileDrop ) == false )
        {
            e.Effect = DragDropEffects.None;
            return;
        }

        var files = (string[]?) e.Data.GetData( DataFormats.FileDrop );

        if ( files?.Any( f =>
        {
            var ext = Path.GetExtension( f ).ToLowerInvariant();
            return ext is ".xml" or ".xsl" or ".xslt";
        } ) == true )
        {
            e.Effect = DragDropEffects.Copy;
        }
        else
        {
            e.Effect = DragDropEffects.None;
        }
    }


    /// <summary />
    private void AppForm_DragDrop( object? sender, DragEventArgs e )
    {
        if ( e.Data!.GetDataPresent( DataFormats.FileDrop ) == false )
            return;

        var files = (string[]?) e.Data.GetData( DataFormats.FileDrop );

        if ( files is null )
            return;


        /*
         * 
         */
        textOutput.Text = "";

        foreach ( var file in files )
        {
            var ext = Path.GetExtension( file );

            switch ( ext.ToLowerInvariant() )
            {
                case ".xslt":
                case ".xsl":
                    LoadSchematron( file );
                    break;

                case ".xml":
                    textXml.Text = File.ReadAllText( file, Encoding.UTF8 );
                    break;

                default:
                    textOutput.Text += $"Unsupported file extension: {ext}" + Environment.NewLine;
                    break;
            }
        }
    }
}