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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager( typeof( AppForm ) );
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
            backgroundWorker = new System.ComponentModel.BackgroundWorker();
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
            splitContainer.Margin = new Padding( 2, 1, 2, 1 );
            splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            splitContainer.Panel1.Controls.Add( tableLayoutPanelLeft );
            splitContainer.Panel1.Padding = new Padding( 3, 2, 3, 2 );
            // 
            // splitContainer.Panel2
            // 
            splitContainer.Panel2.Controls.Add( tableLayoutPanelRight );
            splitContainer.Panel2.Padding = new Padding( 3, 2, 3, 2 );
            splitContainer.Size = new Size( 603, 392 );
            splitContainer.SplitterDistance = 306;
            splitContainer.SplitterWidth = 2;
            splitContainer.TabIndex = 5;
            // 
            // tableLayoutPanelLeft
            // 
            tableLayoutPanelLeft.ColumnCount = 1;
            tableLayoutPanelLeft.ColumnStyles.Add( new ColumnStyle( SizeType.Percent, 100F ) );
            tableLayoutPanelLeft.Controls.Add( flowLayoutPanelLeft, 0, 0 );
            tableLayoutPanelLeft.Controls.Add( textXml, 0, 1 );
            tableLayoutPanelLeft.Dock = DockStyle.Fill;
            tableLayoutPanelLeft.Location = new Point( 3, 2 );
            tableLayoutPanelLeft.Margin = new Padding( 2, 1, 2, 1 );
            tableLayoutPanelLeft.Name = "tableLayoutPanelLeft";
            tableLayoutPanelLeft.RowCount = 2;
            tableLayoutPanelLeft.RowStyles.Add( new RowStyle( SizeType.Absolute, 38F ) );
            tableLayoutPanelLeft.RowStyles.Add( new RowStyle( SizeType.Percent, 100F ) );
            tableLayoutPanelLeft.Size = new Size( 300, 388 );
            tableLayoutPanelLeft.TabIndex = 7;
            // 
            // flowLayoutPanelLeft
            // 
            flowLayoutPanelLeft.Controls.Add( btnLoadXslt );
            flowLayoutPanelLeft.Controls.Add( btnLoadXml );
            flowLayoutPanelLeft.Dock = DockStyle.Fill;
            flowLayoutPanelLeft.Location = new Point( 2, 1 );
            flowLayoutPanelLeft.Margin = new Padding( 2, 1, 2, 1 );
            flowLayoutPanelLeft.Name = "flowLayoutPanelLeft";
            flowLayoutPanelLeft.Size = new Size( 296, 36 );
            flowLayoutPanelLeft.TabIndex = 8;
            // 
            // btnLoadXslt
            // 
            btnLoadXslt.Location = new Point( 2, 1 );
            btnLoadXslt.Margin = new Padding( 2, 1, 2, 1 );
            btnLoadXslt.Name = "btnLoadXslt";
            btnLoadXslt.Size = new Size( 127, 22 );
            btnLoadXslt.TabIndex = 3;
            btnLoadXslt.Text = "Load xslt...";
            btnLoadXslt.UseVisualStyleBackColor = true;
            btnLoadXslt.Click += btnLoadXslt_Click;
            // 
            // btnLoadXml
            // 
            btnLoadXml.Location = new Point( 133, 1 );
            btnLoadXml.Margin = new Padding( 2, 1, 2, 1 );
            btnLoadXml.Name = "btnLoadXml";
            btnLoadXml.Size = new Size( 118, 22 );
            btnLoadXml.TabIndex = 5;
            btnLoadXml.Text = "Load UBL...";
            btnLoadXml.UseVisualStyleBackColor = true;
            btnLoadXml.Click += btnLoadXml_Click;
            // 
            // textXml
            // 
            textXml.AcceptsReturn = true;
            textXml.Dock = DockStyle.Fill;
            textXml.Font = new Font( "Consolas", 7.875F, FontStyle.Regular, GraphicsUnit.Point, 0 );
            textXml.Location = new Point( 2, 39 );
            textXml.Margin = new Padding( 2, 1, 2, 1 );
            textXml.Multiline = true;
            textXml.Name = "textXml";
            textXml.ScrollBars = ScrollBars.Both;
            textXml.Size = new Size( 296, 348 );
            textXml.TabIndex = 4;
            textXml.KeyUp += textXml_KeyUp;
            // 
            // tableLayoutPanelRight
            // 
            tableLayoutPanelRight.ColumnCount = 1;
            tableLayoutPanelRight.ColumnStyles.Add( new ColumnStyle( SizeType.Percent, 100F ) );
            tableLayoutPanelRight.Controls.Add( textOutput, 0, 1 );
            tableLayoutPanelRight.Controls.Add( btnRun, 0, 0 );
            tableLayoutPanelRight.Dock = DockStyle.Fill;
            tableLayoutPanelRight.Location = new Point( 3, 2 );
            tableLayoutPanelRight.Margin = new Padding( 2, 1, 2, 1 );
            tableLayoutPanelRight.Name = "tableLayoutPanelRight";
            tableLayoutPanelRight.RowCount = 2;
            tableLayoutPanelRight.RowStyles.Add( new RowStyle( SizeType.Absolute, 38F ) );
            tableLayoutPanelRight.RowStyles.Add( new RowStyle( SizeType.Percent, 100F ) );
            tableLayoutPanelRight.Size = new Size( 289, 388 );
            tableLayoutPanelRight.TabIndex = 8;
            // 
            // textOutput
            // 
            textOutput.AcceptsReturn = true;
            textOutput.Dock = DockStyle.Fill;
            textOutput.Font = new Font( "Consolas", 7.875F, FontStyle.Regular, GraphicsUnit.Point, 0 );
            textOutput.Location = new Point( 2, 39 );
            textOutput.Margin = new Padding( 2, 1, 2, 1 );
            textOutput.Multiline = true;
            textOutput.Name = "textOutput";
            textOutput.ScrollBars = ScrollBars.Both;
            textOutput.Size = new Size( 285, 348 );
            textOutput.TabIndex = 2;
            // 
            // btnRun
            // 
            btnRun.Location = new Point( 2, 1 );
            btnRun.Margin = new Padding( 2, 1, 2, 1 );
            btnRun.Name = "btnRun";
            btnRun.Size = new Size( 127, 23 );
            btnRun.TabIndex = 1;
            btnRun.Text = "Evaluate";
            btnRun.UseVisualStyleBackColor = true;
            btnRun.Click += btnRun_Click;
            // 
            // backgroundWorker
            // 
            backgroundWorker.WorkerSupportsCancellation = true;
            backgroundWorker.DoWork += backgroundWorker_DoWork;
            backgroundWorker.RunWorkerCompleted += backgroundWorker_RunWorkerCompleted;
            // 
            // AppForm
            // 
            AutoScaleDimensions = new SizeF( 7F, 15F );
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size( 603, 392 );
            Controls.Add( splitContainer );
            Icon = (Icon) resources.GetObject( "$this.Icon" );
            Margin = new Padding( 2, 1, 2, 1 );
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
        private System.ComponentModel.BackgroundWorker backgroundWorker;
    }
}
