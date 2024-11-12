namespace WorkFlowDiagram
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.gViewer1 = new Microsoft.Msagl.GraphViewerGdi.GViewer();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.hideStateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hideLinksToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.unhideAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.button_loadWorkFlow = new System.Windows.Forms.Button();
            this.checkBox_useShelf = new System.Windows.Forms.CheckBox();
            this.checkBox_showStateElements = new System.Windows.Forms.CheckBox();
            this.checkBox_useVsCode = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBox_layoutMode = new System.Windows.Forms.ComboBox();
            this.textBox_tag = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // gViewer1
            // 
            this.gViewer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gViewer1.ArrowheadLength = 10D;
            this.gViewer1.AsyncLayout = false;
            this.gViewer1.AutoScroll = true;
            this.gViewer1.BackwardEnabled = false;
            this.gViewer1.BuildHitTree = true;
            this.gViewer1.ContextMenuStrip = this.contextMenuStrip1;
            this.gViewer1.CurrentLayoutMethod = Microsoft.Msagl.GraphViewerGdi.LayoutMethod.UseSettingsOfTheGraph;
            this.gViewer1.EdgeInsertButtonVisible = true;
            this.gViewer1.FileName = "";
            this.gViewer1.ForwardEnabled = false;
            this.gViewer1.Graph = null;
            this.gViewer1.IncrementalDraggingModeAlways = false;
            this.gViewer1.InsertingEdge = false;
            this.gViewer1.LayoutAlgorithmSettingsButtonVisible = false;
            this.gViewer1.LayoutEditingEnabled = true;
            this.gViewer1.Location = new System.Drawing.Point(152, 12);
            this.gViewer1.LooseOffsetForRouting = 0.25D;
            this.gViewer1.MouseHitDistance = 0.05D;
            this.gViewer1.Name = "gViewer1";
            this.gViewer1.NavigationVisible = false;
            this.gViewer1.NeedToCalculateLayout = true;
            this.gViewer1.OffsetForRelaxingInRouting = 0.6D;
            this.gViewer1.PaddingForEdgeRouting = 8D;
            this.gViewer1.PanButtonPressed = false;
            this.gViewer1.SaveAsImageEnabled = true;
            this.gViewer1.SaveAsMsaglEnabled = true;
            this.gViewer1.SaveButtonVisible = true;
            this.gViewer1.SaveGraphButtonVisible = true;
            this.gViewer1.SaveInVectorFormatEnabled = true;
            this.gViewer1.Size = new System.Drawing.Size(855, 422);
            this.gViewer1.TabIndex = 0;
            this.gViewer1.TightOffsetForRouting = 0.125D;
            this.gViewer1.ToolBarIsVisible = true;
            this.gViewer1.Transform = ((Microsoft.Msagl.Core.Geometry.Curves.PlaneTransformation)(resources.GetObject("gViewer1.Transform")));
            this.gViewer1.UndoRedoButtonsVisible = true;
            this.gViewer1.WindowZoomButtonPressed = false;
            this.gViewer1.ZoomF = 1D;
            this.gViewer1.ZoomWindowThreshold = 0.05D;
            this.gViewer1.ObjectUnderMouseCursorChanged += new System.EventHandler<Microsoft.Msagl.Drawing.ObjectUnderMouseCursorChangedEventArgs>(this.GViewer1_ObjectUnderMouseCursorChanged);
            this.gViewer1.CustomOpenButtonPressed += new System.EventHandler<System.ComponentModel.HandledEventArgs>(this.GViewer1_CustomOpenButtonPressed);
            this.gViewer1.GraphSavingEnded += new System.EventHandler(this.GViewer1_GraphSavingEnded);
            this.gViewer1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.GViewer1_MouseMove);
            this.gViewer1.DoubleClick += new System.EventHandler(this.GViewer1_DoubleClick);
            this.gViewer1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.GViewer1_MouseClick);
            this.gViewer1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.GViewer1_MouseDown);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.hideStateToolStripMenuItem,
            this.hideLinksToolStripMenuItem,
            this.unhideAllToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(133, 70);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenuStrip1_Opening);
            // 
            // hideStateToolStripMenuItem
            // 
            this.hideStateToolStripMenuItem.Name = "hideStateToolStripMenuItem";
            this.hideStateToolStripMenuItem.Size = new System.Drawing.Size(132, 22);
            this.hideStateToolStripMenuItem.Text = "Hide state";
            this.hideStateToolStripMenuItem.Click += new System.EventHandler(this.HideStateToolStripMenuItem_Click);
            // 
            // hideLinksToolStripMenuItem
            // 
            this.hideLinksToolStripMenuItem.Name = "hideLinksToolStripMenuItem";
            this.hideLinksToolStripMenuItem.Size = new System.Drawing.Size(132, 22);
            this.hideLinksToolStripMenuItem.Text = "Hide links";
            this.hideLinksToolStripMenuItem.Click += new System.EventHandler(this.HideLinksToolStripMenuItem_Click);
            // 
            // unhideAllToolStripMenuItem
            // 
            this.unhideAllToolStripMenuItem.Name = "unhideAllToolStripMenuItem";
            this.unhideAllToolStripMenuItem.Size = new System.Drawing.Size(132, 22);
            this.unhideAllToolStripMenuItem.Text = "Un-hide all";
            this.unhideAllToolStripMenuItem.Click += new System.EventHandler(this.UnhideAllToolStripMenuItem_Click);
            // 
            // button_loadWorkFlow
            // 
            this.button_loadWorkFlow.Location = new System.Drawing.Point(12, 12);
            this.button_loadWorkFlow.Name = "button_loadWorkFlow";
            this.button_loadWorkFlow.Size = new System.Drawing.Size(134, 23);
            this.button_loadWorkFlow.TabIndex = 1;
            this.button_loadWorkFlow.Text = "Load workflow";
            this.button_loadWorkFlow.UseVisualStyleBackColor = true;
            this.button_loadWorkFlow.Click += new System.EventHandler(this.Button_LoadStates_Click);
            // 
            // checkBox_useShelf
            // 
            this.checkBox_useShelf.AutoSize = true;
            this.checkBox_useShelf.Location = new System.Drawing.Point(12, 41);
            this.checkBox_useShelf.Name = "checkBox_useShelf";
            this.checkBox_useShelf.Size = new System.Drawing.Size(84, 19);
            this.checkBox_useShelf.TabIndex = 2;
            this.checkBox_useShelf.Text = "SHELF json";
            this.checkBox_useShelf.UseVisualStyleBackColor = true;
            this.checkBox_useShelf.CheckedChanged += new System.EventHandler(this.CheckBox_UseShelf_CheckedChanged);
            // 
            // checkBox_showStateElements
            // 
            this.checkBox_showStateElements.AutoSize = true;
            this.checkBox_showStateElements.Location = new System.Drawing.Point(12, 66);
            this.checkBox_showStateElements.Name = "checkBox_showStateElements";
            this.checkBox_showStateElements.Size = new System.Drawing.Size(134, 19);
            this.checkBox_showStateElements.TabIndex = 2;
            this.checkBox_showStateElements.Text = "Show state elements";
            this.checkBox_showStateElements.UseVisualStyleBackColor = true;
            this.checkBox_showStateElements.CheckedChanged += new System.EventHandler(this.CheckBox_showStateElements_CheckedChanged);
            // 
            // checkBox_useVsCode
            // 
            this.checkBox_useVsCode.AutoSize = true;
            this.checkBox_useVsCode.Location = new System.Drawing.Point(12, 91);
            this.checkBox_useVsCode.Name = "checkBox_useVsCode";
            this.checkBox_useVsCode.Size = new System.Drawing.Size(89, 19);
            this.checkBox_useVsCode.TabIndex = 2;
            this.checkBox_useVsCode.Text = "Use VSCode";
            this.checkBox_useVsCode.UseVisualStyleBackColor = true;
            this.checkBox_useVsCode.CheckedChanged += new System.EventHandler(this.CheckBox_UseVsCode_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 113);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(80, 15);
            this.label1.TabIndex = 3;
            this.label1.Text = "Layout mode:";
            // 
            // comboBox_layoutMode
            // 
            this.comboBox_layoutMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_layoutMode.FormattingEnabled = true;
            this.comboBox_layoutMode.Location = new System.Drawing.Point(12, 131);
            this.comboBox_layoutMode.Name = "comboBox_layoutMode";
            this.comboBox_layoutMode.Size = new System.Drawing.Size(134, 23);
            this.comboBox_layoutMode.TabIndex = 4;
            this.comboBox_layoutMode.SelectedIndexChanged += new System.EventHandler(this.ComboBox_layoutMode_SelectedIndexChanged);
            // 
            // textBox_tag
            // 
            this.textBox_tag.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_tag.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.textBox_tag.Location = new System.Drawing.Point(12, 440);
            this.textBox_tag.Multiline = true;
            this.textBox_tag.Name = "textBox_tag";
            this.textBox_tag.ReadOnly = true;
            this.textBox_tag.Size = new System.Drawing.Size(995, 71);
            this.textBox_tag.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 422);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(85, 15);
            this.label2.TabIndex = 6;
            this.label2.Text = "Element notes:";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1019, 523);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox_tag);
            this.Controls.Add(this.comboBox_layoutMode);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.checkBox_useVsCode);
            this.Controls.Add(this.checkBox_showStateElements);
            this.Controls.Add(this.checkBox_useShelf);
            this.Controls.Add(this.button_loadWorkFlow);
            this.Controls.Add(this.gViewer1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Microsoft.Msagl.GraphViewerGdi.GViewer gViewer1;
        private FolderBrowserDialog folderBrowserDialog1;
        private Button button_loadWorkFlow;
        private CheckBox checkBox_useShelf;
        private CheckBox checkBox_showStateElements;
        private CheckBox checkBox_useVsCode;
        private Label label1;
        private ComboBox comboBox_layoutMode;
        private TextBox textBox_tag;
        private Label label2;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem hideStateToolStripMenuItem;
        private ToolStripMenuItem hideLinksToolStripMenuItem;
        private ToolStripMenuItem unhideAllToolStripMenuItem;
        private SaveFileDialog saveFileDialog1;
        private OpenFileDialog openFileDialog1;
    }
}