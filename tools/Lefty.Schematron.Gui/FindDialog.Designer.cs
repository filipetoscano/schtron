namespace Lefty.Schematron.Gui
{
    partial class FindDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose( bool disposing )
        {
            if ( disposing && ( components != null ) )
            {
                components.Dispose();
            }
            base.Dispose( disposing );
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            textFind = new TextBox();
            btnNext = new Button();
            SuspendLayout();
            // 
            // textFind
            // 
            textFind.Font = new Font( "Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point,  0 );
            textFind.Location = new Point( 12, 24 );
            textFind.Name = "textFind";
            textFind.Size = new Size( 482, 36 );
            textFind.TabIndex = 0;
            // 
            // btnNext
            // 
            btnNext.Location = new Point( 496, 24 );
            btnNext.Name = "btnNext";
            btnNext.Size = new Size( 66, 39 );
            btnNext.TabIndex = 1;
            btnNext.Text = ">";
            btnNext.UseVisualStyleBackColor = true;
            btnNext.Click +=  btnNext_Click ;
            // 
            // FindDialog
            // 
            AcceptButton = btnNext;
            AutoScaleDimensions = new SizeF( 13F, 32F );
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size( 574, 81 );
            Controls.Add( btnNext );
            Controls.Add( textFind );
            FormBorderStyle = FormBorderStyle.None;
            KeyPreview = true;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FindDialog";
            ShowInTaskbar = false;
            Text = "FindDialog";
            ResumeLayout( false );
            PerformLayout();
        }

        #endregion

        private TextBox textFind;
        private Button btnNext;
    }
}