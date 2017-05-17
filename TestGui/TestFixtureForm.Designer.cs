namespace TestGui
{
    partial class TestFixtureForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.ColumnHeader categoryColumn;
            System.Windows.Forms.ListViewGroup listViewGroup1 = new System.Windows.Forms.ListViewGroup("Failed", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup2 = new System.Windows.Forms.ListViewGroup("Passed", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup3 = new System.Windows.Forms.ListViewGroup("Ignored", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup4 = new System.Windows.Forms.ListViewGroup("Slow Tests", System.Windows.Forms.HorizontalAlignment.Left);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TestFixtureForm));
            System.Windows.Forms.ListViewGroup listViewGroup5 = new System.Windows.Forms.ListViewGroup("Filters", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem("All");
            System.Windows.Forms.ListViewItem listViewItem2 = new System.Windows.Forms.ListViewItem("Passed", 0);
            System.Windows.Forms.ListViewItem listViewItem3 = new System.Windows.Forms.ListViewItem("Failed", 1);
            System.Windows.Forms.ListViewItem listViewItem4 = new System.Windows.Forms.ListViewItem("Ignored", 3);
            System.Windows.Forms.ListViewItem listViewItem5 = new System.Windows.Forms.ListViewItem("Slow", 2);
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.testsList = new System.Windows.Forms.ListView();
            this.TestHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.fixtureColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.TimeHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.outputColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.detailsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.listMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.runTestsAgainMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.testIcons32 = new System.Windows.Forms.ImageList(this.components);
            this.testIcons16 = new System.Windows.Forms.ImageList(this.components);
            this.categoriesList = new System.Windows.Forms.ListView();
            this.outputText = new System.Windows.Forms.RichTextBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.statusProgress = new System.Windows.Forms.ToolStripProgressBar();
            this.statusText = new System.Windows.Forms.ToolStripStatusLabel();
            categoryColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.contextMenu.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // categoryColumn
            // 
            categoryColumn.Text = "Category";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(4);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.testsList);
            this.splitContainer1.Panel1.Controls.Add(this.categoriesList);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.outputText);
            this.splitContainer1.Panel2Collapsed = true;
            this.splitContainer1.Size = new System.Drawing.Size(666, 625);
            this.splitContainer1.SplitterDistance = 641;
            this.splitContainer1.TabIndex = 0;
            // 
            // testsList
            // 
            this.testsList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.TestHeader,
            this.fixtureColumn,
            this.TimeHeader,
            this.outputColumn,
            categoryColumn});
            this.testsList.ContextMenuStrip = this.contextMenu;
            this.testsList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.testsList.Font = new System.Drawing.Font("Trebuchet MS", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.testsList.FullRowSelect = true;
            listViewGroup1.Header = "Failed";
            listViewGroup1.Name = "failedGroup";
            listViewGroup1.Tag = "failedGroup";
            listViewGroup2.Header = "Passed";
            listViewGroup2.Name = "passedGroup";
            listViewGroup2.Tag = "passedGroup";
            listViewGroup3.Header = "Ignored";
            listViewGroup3.Name = "ignoredGroup";
            listViewGroup3.Tag = "ignoredGroup";
            listViewGroup4.Header = "Slow Tests";
            listViewGroup4.Name = "slowGroup";
            listViewGroup4.Tag = "slowGroup";
            this.testsList.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup1,
            listViewGroup2,
            listViewGroup3,
            listViewGroup4});
            this.testsList.HideSelection = false;
            this.testsList.LargeImageList = this.testIcons32;
            this.testsList.Location = new System.Drawing.Point(120, 0);
            this.testsList.Margin = new System.Windows.Forms.Padding(4);
            this.testsList.MultiSelect = false;
            this.testsList.Name = "testsList";
            this.testsList.ShowItemToolTips = true;
            this.testsList.Size = new System.Drawing.Size(546, 625);
            this.testsList.SmallImageList = this.testIcons16;
            this.testsList.TabIndex = 1;
            this.testsList.UseCompatibleStateImageBehavior = false;
            this.testsList.View = System.Windows.Forms.View.Details;
            this.testsList.SelectedIndexChanged += new System.EventHandler(this.testsList_SelectedIndexChanged);
            this.testsList.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.testsList_MouseDoubleClick);
            // 
            // TestHeader
            // 
            this.TestHeader.Text = "Test";
            this.TestHeader.Width = 262;
            // 
            // fixtureColumn
            // 
            this.fixtureColumn.Text = "Fixture";
            this.fixtureColumn.Width = 127;
            // 
            // TimeHeader
            // 
            this.TimeHeader.Text = "Time (MS)";
            this.TimeHeader.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.TimeHeader.Width = 67;
            // 
            // outputColumn
            // 
            this.outputColumn.Text = "Output?";
            // 
            // contextMenu
            // 
            this.contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.detailsMenuItem,
            this.listMenuItem,
            this.toolStripMenuItem1,
            this.runTestsAgainMenuItem});
            this.contextMenu.Name = "contextMenu";
            this.contextMenu.Size = new System.Drawing.Size(155, 76);
            // 
            // detailsMenuItem
            // 
            this.detailsMenuItem.Name = "detailsMenuItem";
            this.detailsMenuItem.Size = new System.Drawing.Size(154, 22);
            this.detailsMenuItem.Text = "Details";
            this.detailsMenuItem.Click += new System.EventHandler(this.detailsMenuItem_Click);
            // 
            // listMenuItem
            // 
            this.listMenuItem.Name = "listMenuItem";
            this.listMenuItem.Size = new System.Drawing.Size(154, 22);
            this.listMenuItem.Text = "List";
            this.listMenuItem.Click += new System.EventHandler(this.listMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(151, 6);
            // 
            // runTestsAgainMenuItem
            // 
            this.runTestsAgainMenuItem.Name = "runTestsAgainMenuItem";
            this.runTestsAgainMenuItem.Size = new System.Drawing.Size(154, 22);
            this.runTestsAgainMenuItem.Text = "Run tests again";
            this.runTestsAgainMenuItem.Click += new System.EventHandler(this.runTestsAgainMenuItem_Click);
            // 
            // testIcons32
            // 
            this.testIcons32.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("testIcons32.ImageStream")));
            this.testIcons32.TransparentColor = System.Drawing.Color.Transparent;
            this.testIcons32.Images.SetKeyName(0, "tick-32.ico");
            this.testIcons32.Images.SetKeyName(1, "cross-32.ico");
            this.testIcons32.Images.SetKeyName(2, "slow-32.ico");
            // 
            // testIcons16
            // 
            this.testIcons16.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("testIcons16.ImageStream")));
            this.testIcons16.TransparentColor = System.Drawing.Color.Transparent;
            this.testIcons16.Images.SetKeyName(0, "tick-16.ico");
            this.testIcons16.Images.SetKeyName(1, "cross-16.ico");
            this.testIcons16.Images.SetKeyName(2, "slow-16.ico");
            this.testIcons16.Images.SetKeyName(3, "ignored.png");
            // 
            // categoriesList
            // 
            this.categoriesList.BackColor = System.Drawing.SystemColors.Control;
            this.categoriesList.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.categoriesList.Dock = System.Windows.Forms.DockStyle.Left;
            this.categoriesList.FullRowSelect = true;
            listViewGroup5.Header = "Filters";
            listViewGroup5.Name = "listViewGroup1";
            listViewGroup5.Tag = "Filters";
            this.categoriesList.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup5});
            this.categoriesList.HideSelection = false;
            listViewItem1.Group = listViewGroup5;
            listViewItem1.Tag = "all";
            listViewItem2.Group = listViewGroup5;
            listViewItem2.Tag = "passedGroup";
            listViewItem3.Group = listViewGroup5;
            listViewItem3.Tag = "failedGroup";
            listViewItem4.Group = listViewGroup5;
            listViewItem4.Tag = "ignoredGroup";
            listViewItem5.Group = listViewGroup5;
            listViewItem5.Tag = "slowGroup";
            this.categoriesList.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1,
            listViewItem2,
            listViewItem3,
            listViewItem4,
            listViewItem5});
            this.categoriesList.Location = new System.Drawing.Point(0, 0);
            this.categoriesList.Margin = new System.Windows.Forms.Padding(4);
            this.categoriesList.Name = "categoriesList";
            this.categoriesList.Size = new System.Drawing.Size(120, 625);
            this.categoriesList.SmallImageList = this.testIcons16;
            this.categoriesList.TabIndex = 0;
            this.categoriesList.UseCompatibleStateImageBehavior = false;
            this.categoriesList.View = System.Windows.Forms.View.SmallIcon;
            this.categoriesList.SelectedIndexChanged += new System.EventHandler(this.categoriesList_SelectedIndexChanged);
            // 
            // outputText
            // 
            this.outputText.BackColor = System.Drawing.SystemColors.Window;
            this.outputText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.outputText.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.outputText.Location = new System.Drawing.Point(0, 0);
            this.outputText.Name = "outputText";
            this.outputText.ReadOnly = true;
            this.outputText.Size = new System.Drawing.Size(96, 100);
            this.outputText.TabIndex = 0;
            this.outputText.Text = "";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusProgress,
            this.statusText});
            this.statusStrip1.Location = new System.Drawing.Point(0, 629);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(666, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // statusProgress
            // 
            this.statusProgress.Name = "statusProgress";
            this.statusProgress.Size = new System.Drawing.Size(120, 16);
            // 
            // statusText
            // 
            this.statusText.Name = "statusText";
            this.statusText.Size = new System.Drawing.Size(529, 17);
            this.statusText.Spring = true;
            this.statusText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // TestFixtureForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(666, 651);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.splitContainer1);
            this.Font = new System.Drawing.Font("Trebuchet MS", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "TestFixtureForm";
            this.Text = "Tests";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TestFixtureForm_FormClosing);
            this.Load += new System.EventHandler(this.Tests_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.contextMenu.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ListView testsList;
        private System.Windows.Forms.ListView categoriesList;
        private System.Windows.Forms.ImageList testIcons16;
        private System.Windows.Forms.ColumnHeader TestHeader;
        private System.Windows.Forms.ColumnHeader TimeHeader;
        private System.Windows.Forms.RichTextBox outputText;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripProgressBar statusProgress;
        private System.Windows.Forms.ToolStripStatusLabel statusText;
        private System.Windows.Forms.ColumnHeader outputColumn;
        private System.Windows.Forms.ContextMenuStrip contextMenu;
        private System.Windows.Forms.ToolStripMenuItem detailsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem listMenuItem;
        private System.Windows.Forms.ColumnHeader fixtureColumn;
        private System.Windows.Forms.ImageList testIcons32;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem runTestsAgainMenuItem;
    }
}

