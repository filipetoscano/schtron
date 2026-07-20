namespace Lefty.Schematron.Gui
{
    partial class AppForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            openFileDialog = new OpenFileDialog();
            splitContainer = new SplitContainer();
            tableLayoutPanelLeft = new TableLayoutPanel();
            flowLayoutPanelLeft = new FlowLayoutPanel();
            btnLoadXslt = new Button();
            btnLoadXml = new Button();
            textXml = new TextBox();
            tableLayoutPanelRight = new TableLayoutPanel();
            textOutput = new TextBox();
            btnRun = new Button();
            ( (System.ComponentModel.ISupportInitialize) splitContainer ).BeginInit();
            splitContainer.Panel1.SuspendLayout();
            splitContainer.Panel2.SuspendLayout();
            splitContainer.SuspendLayout();
            tableLayoutPanelLeft.SuspendLayout();
            flowLayoutPanelLeft.SuspendLayout();
            tableLayoutPanelRight.SuspendLayout();
            SuspendLayout();
            // 
            // openFileDialog
            // 
            openFileDialog.Filter = "XSLT|*.xslt|All files|*.*";
            // 
            // splitContainer
            // 
            splitContainer.Dock = DockStyle.Fill;
            splitContainer.Location = new Point( 0, 0 );
            splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            splitContainer.Panel1.Controls.Add( tableLayoutPanelLeft );
            splitContainer.Panel1.Padding = new Padding( 5 );
            // 
            // splitContainer.Panel2
            // 
            splitContainer.Panel2.Controls.Add( tableLayoutPanelRight );
            splitContainer.Panel2.Padding = new Padding( 5 );
            splitContainer.Size = new Size( 1120, 837 );
            splitContainer.SplitterDistance = 570;
            splitContainer.TabIndex = 5;
            // 
            // tableLayoutPanelLeft
            // 
            tableLayoutPanelLeft.ColumnCount = 1;
            tableLayoutPanelLeft.ColumnStyles.Add( new ColumnStyle( SizeType.Percent, 100F ) );
            tableLayoutPanelLeft.Controls.Add( flowLayoutPanelLeft, 0, 0 );
            tableLayoutPanelLeft.Controls.Add( textXml, 0, 1 );
            tableLayoutPanelLeft.Dock = DockStyle.Fill;
            tableLayoutPanelLeft.Location = new Point( 5, 5 );
            tableLayoutPanelLeft.Name = "tableLayoutPanelLeft";
            tableLayoutPanelLeft.RowCount = 2;
            tableLayoutPanelLeft.RowStyles.Add( new RowStyle( SizeType.Absolute, 80F ) );
            tableLayoutPanelLeft.RowStyles.Add( new RowStyle( SizeType.Percent, 100F ) );
            tableLayoutPanelLeft.Size = new Size( 560, 827 );
            tableLayoutPanelLeft.TabIndex = 7;
            // 
            // flowLayoutPanelLeft
            // 
            flowLayoutPanelLeft.Controls.Add( btnLoadXslt );
            flowLayoutPanelLeft.Controls.Add( btnLoadXml );
            flowLayoutPanelLeft.Dock = DockStyle.Fill;
            flowLayoutPanelLeft.Location = new Point( 3, 3 );
            flowLayoutPanelLeft.Name = "flowLayoutPanelLeft";
            flowLayoutPanelLeft.Size = new Size( 554, 74 );
            flowLayoutPanelLeft.TabIndex = 8;
            // 
            // btnLoadXslt
            // 
            btnLoadXslt.Location = new Point( 3, 3 );
            btnLoadXslt.Name = "btnLoadXslt";
            btnLoadXslt.Size = new Size( 236, 46 );
            btnLoadXslt.TabIndex = 3;
            btnLoadXslt.Text = "Load xslt...";
            btnLoadXslt.UseVisualStyleBackColor = true;
            btnLoadXslt.Click +=  btnLoadXslt_Click ;
            // 
            // btnLoadXml
            // 
            btnLoadXml.Location = new Point( 245, 3 );
            btnLoadXml.Name = "btnLoadXml";
            btnLoadXml.Size = new Size( 219, 46 );
            btnLoadXml.TabIndex = 5;
            btnLoadXml.Text = "Load UBL...";
            btnLoadXml.UseVisualStyleBackColor = true;
            btnLoadXml.Click +=  btnLoadXml_Click ;
            // 
            // textXml
            // 
            textXml.AcceptsReturn = true;
            textXml.Dock = DockStyle.Fill;
            textXml.Font = new Font( "Consolas", 7.875F, FontStyle.Regular, GraphicsUnit.Point,  0 );
            textXml.Location = new Point( 3, 83 );
            textXml.Multiline = true;
            textXml.Name = "textXml";
            textXml.ScrollBars = ScrollBars.Both;
            textXml.Size = new Size( 554, 741 );
            textXml.TabIndex = 4;
            // 
            // tableLayoutPanelRight
            // 
            tableLayoutPanelRight.ColumnCount = 1;
            tableLayoutPanelRight.ColumnStyles.Add( new ColumnStyle( SizeType.Percent, 100F ) );
            tableLayoutPanelRight.Controls.Add( textOutput, 0, 1 );
            tableLayoutPanelRight.Controls.Add( btnRun, 0, 0 );
            tableLayoutPanelRight.Dock = DockStyle.Fill;
            tableLayoutPanelRight.Location = new Point( 5, 5 );
            tableLayoutPanelRight.Name = "tableLayoutPanelRight";
            tableLayoutPanelRight.RowCount = 2;
            tableLayoutPanelRight.RowStyles.Add( new RowStyle( SizeType.Absolute, 80F ) );
            tableLayoutPanelRight.RowStyles.Add( new RowStyle( SizeType.Percent, 100F ) );
            tableLayoutPanelRight.Size = new Size( 536, 827 );
            tableLayoutPanelRight.TabIndex = 8;
            // 
            // textOutput
            // 
            textOutput.AcceptsReturn = true;
            textOutput.Dock = DockStyle.Fill;
            textOutput.Font = new Font( "Consolas", 7.875F, FontStyle.Regular, GraphicsUnit.Point,  0 );
            textOutput.Location = new Point( 3, 83 );
            textOutput.Multiline = true;
            textOutput.Name = "textOutput";
            textOutput.ScrollBars = ScrollBars.Both;
            textOutput.Size = new Size( 530, 741 );
            textOutput.TabIndex = 2;
            // 
            // btnRun
            // 
            btnRun.Location = new Point( 3, 3 );
            btnRun.Name = "btnRun";
            btnRun.Size = new Size( 235, 49 );
            btnRun.TabIndex = 1;
            btnRun.Text = "Evaluate";
            btnRun.UseVisualStyleBackColor = true;
            btnRun.Click +=  btnRun_Click ;
            // 
            // AppForm
            // 
            AutoScaleDimensions = new SizeF( 13F, 32F );
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size( 1120, 837 );
            Controls.Add( splitContainer );
            Name = "AppForm";
            Text = "schtron";
            splitContainer.Panel1.ResumeLayout( false );
            splitContainer.Panel2.ResumeLayout( false );
            ( (System.ComponentModel.ISupportInitialize) splitContainer ).EndInit();
            splitContainer.ResumeLayout( false );
            tableLayoutPanelLeft.ResumeLayout( false );
            tableLayoutPanelLeft.PerformLayout();
            flowLayoutPanelLeft.ResumeLayout( false );
            tableLayoutPanelRight.ResumeLayout( false );
            tableLayoutPanelRight.PerformLayout();
            ResumeLayout( false );
        }

        #endregion
        private OpenFileDialog openFileDialog;
        private SplitContainer splitContainer;
        private TableLayoutPanel tableLayoutPanelLeft;
        private FlowLayoutPanel flowLayoutPanelLeft;
        private Button btnLoadXslt;
        private Button btnLoadXml;
        private TextBox textXml;
        private TableLayoutPanel tableLayoutPanelRight;
        private TextBox textOutput;
        private Button btnRun;
    }
}
