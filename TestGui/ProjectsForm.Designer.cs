namespace TestGui
{
    partial class ProjectsForm
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
            System.Windows.Forms.ListViewGroup listViewGroup1 = new System.Windows.Forms.ListViewGroup("Days old", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup2 = new System.Windows.Forms.ListViewGroup("Weeks old", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup3 = new System.Windows.Forms.ListViewGroup("Older", System.Windows.Forms.HorizontalAlignment.Left);
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.projectsList = new System.Windows.Forms.ListView();
            this.modifiedColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.projectColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.buildColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.refreshMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.runSelectedMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.runConsoleMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.debugConsoleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 347);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(543, 22);
            this.statusStrip1.TabIndex = 0;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // statusLabel
            // 
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(528, 17);
            this.statusLabel.Spring = true;
            this.statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // projectsList
            // 
            this.projectsList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.modifiedColumn,
            this.projectColumn,
            this.buildColumn});
            this.projectsList.ContextMenuStrip = this.contextMenuStrip1;
            this.projectsList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.projectsList.FullRowSelect = true;
            listViewGroup1.Header = "Days old";
            listViewGroup1.Name = "recentGroup";
            listViewGroup2.Header = "Weeks old";
            listViewGroup2.Name = "midGroup";
            listViewGroup3.Header = "Older";
            listViewGroup3.Name = "oldGroup";
            this.projectsList.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup1,
            listViewGroup2,
            listViewGroup3});
            this.projectsList.Location = new System.Drawing.Point(0, 0);
            this.projectsList.Name = "projectsList";
            this.projectsList.Size = new System.Drawing.Size(543, 347);
            this.projectsList.Sorting = System.Windows.Forms.SortOrder.Descending;
            this.projectsList.TabIndex = 1;
            this.projectsList.UseCompatibleStateImageBehavior = false;
            this.projectsList.View = System.Windows.Forms.View.Details;
            this.projectsList.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.projectsList_MouseDoubleClick);
            // 
            // modifiedColumn
            // 
            this.modifiedColumn.DisplayIndex = 2;
            this.modifiedColumn.Text = "Last Built";
            this.modifiedColumn.Width = 130;
            // 
            // projectColumn
            // 
            this.projectColumn.DisplayIndex = 0;
            this.projectColumn.Text = "Project";
            this.projectColumn.Width = 155;
            // 
            // buildColumn
            // 
            this.buildColumn.DisplayIndex = 1;
            this.buildColumn.Text = "Build";
            this.buildColumn.Width = 250;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.refreshMenu,
            this.runSelectedMenu,
            this.runConsoleMenu,
            this.debugConsoleToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(184, 114);
            // 
            // refreshMenu
            // 
            this.refreshMenu.Name = "refreshMenu";
            this.refreshMenu.ShortcutKeys = System.Windows.Forms.Keys.F5;
            this.refreshMenu.Size = new System.Drawing.Size(183, 22);
            this.refreshMenu.Text = "Refresh";
            this.refreshMenu.Click += new System.EventHandler(this.refreshMenu_Click);
            // 
            // runSelectedMenu
            // 
            this.runSelectedMenu.Name = "runSelectedMenu";
            this.runSelectedMenu.Size = new System.Drawing.Size(183, 22);
            this.runSelectedMenu.Text = "Run &Selected...";
            this.runSelectedMenu.Click += new System.EventHandler(this.runSelectedMenu_Click);
            // 
            // runConsoleMenu
            // 
            this.runConsoleMenu.Name = "runConsoleMenu";
            this.runConsoleMenu.Size = new System.Drawing.Size(183, 22);
            this.runConsoleMenu.Text = "Run &Console...";
            this.runConsoleMenu.Click += new System.EventHandler(this.runConsoleMenu_Click);
            // 
            // debugConsoleToolStripMenuItem
            // 
            this.debugConsoleToolStripMenuItem.Name = "debugConsoleToolStripMenuItem";
            this.debugConsoleToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F9;
            this.debugConsoleToolStripMenuItem.Size = new System.Drawing.Size(183, 22);
            this.debugConsoleToolStripMenuItem.Text = "&Debug Console...";
            this.debugConsoleToolStripMenuItem.Click += new System.EventHandler(this.debugConsoleToolStripMenuItem_Click);
            // 
            // ProjectsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(543, 369);
            this.Controls.Add(this.projectsList);
            this.Controls.Add(this.statusStrip1);
            this.Name = "ProjectsForm";
            this.Text = "Test Projects";
            this.Load += new System.EventHandler(this.ProjectsForm_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ListView projectsList;
        private System.Windows.Forms.ColumnHeader projectColumn;
        private System.Windows.Forms.ColumnHeader buildColumn;
        private System.Windows.Forms.ColumnHeader modifiedColumn;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem refreshMenu;
        private System.Windows.Forms.ToolStripMenuItem runConsoleMenu;
        private System.Windows.Forms.ToolStripMenuItem runSelectedMenu;
        private System.Windows.Forms.ToolStripMenuItem debugConsoleToolStripMenuItem;
    }
}