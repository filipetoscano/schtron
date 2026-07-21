using System.Data;

namespace Lefty.Schematron.Gui;

/// <summary />
public partial class FindDialog : Form
{
    private readonly TextBox _target;


    /// <summary />
    public FindDialog( TextBox target )
    {
        _target = target;

        InitializeComponent();

        textFind.BackColor = Color.FromArgb( 255, 255, 255 );

        StartPosition = FormStartPosition.Manual;
        Location = TopRightOf( target );
    }


    /// <summary>
    /// Anchors the dialog to the top right corner of the target, kept inside the screen.
    /// </summary>
    private Point TopRightOf( Control target )
    {
        var origin = target.PointToScreen( Point.Empty );
        var x = origin.X + target.ClientSize.Width - Width;
        var y = origin.Y;

        var screen = Screen.FromControl( target ).WorkingArea;

        if ( x + Width > screen.Right )
            x = screen.Right - Width;

        if ( x < screen.Left )
            x = screen.Left;

        if ( y + Height > screen.Bottom )
            y = screen.Bottom - Height;

        if ( y < screen.Top )
            y = screen.Top;

        return new Point( x, y );
    }


    /// <summary />
    protected override bool ProcessCmdKey( ref Message msg, Keys keyData )
    {
        if ( keyData == Keys.Escape )
        {
            Close();
            return true;
        }

        if ( keyData == Keys.F3 )
        {
            FindNext();
            return true;
        }

        return base.ProcessCmdKey( ref msg, keyData );
    }


    /// <summary>
    /// Pre-loads the search box (with the target's current selection) and gives it focus.
    /// </summary>
    public void Prime()
    {
        if ( _target.SelectionLength > 0 && _target.SelectedText.Contains( '\n' ) == false )
            textFind.Text = _target.SelectedText;

        textFind.Focus();
        textFind.SelectAll();
    }



    /// <summary />
    private void btnNext_Click( object sender, EventArgs e )
    {
        FindNext();
    }


    /// <summary>
    /// Searches forward from the current caret position, wrapping around at the end.
    /// </summary>
    public void FindNext()
    {
        var needle = textFind.Text;

        if ( needle.Length == 0 )
            return;


        /*
         *
         */
        var cmp = false
            ? StringComparison.Ordinal
            : StringComparison.OrdinalIgnoreCase;

        var haystack = _target.Text;
        var start = _target.SelectionStart + _target.SelectionLength;

        if ( start > haystack.Length )
            start = 0;


        /*
         *
         */
        var pos = haystack.IndexOf( needle, start, cmp );

        if ( pos < 0 && start > 0 )
            pos = haystack.IndexOf( needle, 0, cmp );

        if ( pos < 0 )
        {
            textFind.BackColor = Color.FromArgb( 250, 220, 220 );
            return;
        }

        textFind.BackColor = Color.FromArgb( 255, 255, 255 );


        /*
         *
         */
        _target.Select( pos, needle.Length );
        _target.ScrollToCaret();
    }
}