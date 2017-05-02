namespace TestGui
{
    partial class Tests
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
            System.Windows.Forms.ListViewGroup listViewGroup1 = new System.Windows.Forms.ListViewGroup("Failed", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup2 = new System.Windows.Forms.ListViewGroup("Passed", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup3 = new System.Windows.Forms.ListViewGroup("Ignored", System.Windows.Forms.HorizontalAlignment.Left);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Tests));
            System.Windows.Forms.ListViewGroup listViewGroup4 = new System.Windows.Forms.ListViewGroup("Filters", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem("Passed", 0);
            System.Windows.Forms.ListViewItem listViewItem2 = new System.Windows.Forms.ListViewItem("Failed", 1);
            System.Windows.Forms.ListViewItem listViewItem3 = new System.Windows.Forms.ListViewItem("Ignored", 3);
            System.Windows.Forms.ListViewItem listViewItem4 = new System.Windows.Forms.ListViewItem("Slow", 2);
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.testsList = new System.Windows.Forms.ListView();
            this.TestHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.TimeHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.testIcons16 = new System.Windows.Forms.ImageList(this.components);
            this.categoriesList = new System.Windows.Forms.ListView();
            this.outputText = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
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
            this.splitContainer1.Size = new System.Drawing.Size(967, 598);
            this.splitContainer1.SplitterDistance = 612;
            this.splitContainer1.TabIndex = 0;
            // 
            // testsList
            // 
            this.testsList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.TestHeader,
            this.TimeHeader});
            this.testsList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.testsList.Font = new System.Drawing.Font("Trebuchet MS", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.testsList.FullRowSelect = true;
            listViewGroup1.Header = "Failed";
            listViewGroup1.Name = "failedGroup";
            listViewGroup2.Header = "Passed";
            listViewGroup2.Name = "passedGroup";
            listViewGroup3.Header = "Ignored";
            listViewGroup3.Name = "ignoredGroup";
            this.testsList.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup1,
            listViewGroup2,
            listViewGroup3});
            this.testsList.HideSelection = false;
            this.testsList.Location = new System.Drawing.Point(141, 0);
            this.testsList.Margin = new System.Windows.Forms.Padding(4);
            this.testsList.MultiSelect = false;
            this.testsList.Name = "testsList";
            this.testsList.Size = new System.Drawing.Size(471, 598);
            this.testsList.SmallImageList = this.testIcons16;
            this.testsList.TabIndex = 1;
            this.testsList.UseCompatibleStateImageBehavior = false;
            this.testsList.View = System.Windows.Forms.View.Details;
            this.testsList.SelectedIndexChanged += new System.EventHandler(this.testsList_SelectedIndexChanged);
            // 
            // TestHeader
            // 
            this.TestHeader.Text = "Test";
            this.TestHeader.Width = 378;
            // 
            // TimeHeader
            // 
            this.TimeHeader.Text = "Time (MS)";
            this.TimeHeader.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.TimeHeader.Width = 80;
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
            listViewGroup4.Header = "Filters";
            listViewGroup4.Name = "listViewGroup1";
            listViewGroup4.Tag = "Filters";
            this.categoriesList.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup4});
            this.categoriesList.HideSelection = false;
            listViewItem1.Group = listViewGroup4;
            listViewItem2.Group = listViewGroup4;
            listViewItem3.Group = listViewGroup4;
            listViewItem4.Group = listViewGroup4;
            this.categoriesList.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1,
            listViewItem2,
            listViewItem3,
            listViewItem4});
            this.categoriesList.Location = new System.Drawing.Point(0, 0);
            this.categoriesList.Margin = new System.Windows.Forms.Padding(4);
            this.categoriesList.Name = "categoriesList";
            this.categoriesList.Size = new System.Drawing.Size(141, 598);
            this.categoriesList.SmallImageList = this.testIcons16;
            this.categoriesList.TabIndex = 0;
            this.categoriesList.UseCompatibleStateImageBehavior = false;
            this.categoriesList.View = System.Windows.Forms.View.SmallIcon;
            // 
            // outputText
            // 
            this.outputText.BackColor = System.Drawing.SystemColors.Window;
            this.outputText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.outputText.Location = new System.Drawing.Point(0, 0);
            this.outputText.Margin = new System.Windows.Forms.Padding(4);
            this.outputText.Multiline = true;
            this.outputText.Name = "outputText";
            this.outputText.ReadOnly = true;
            this.outputText.Size = new System.Drawing.Size(351, 598);
            this.outputText.TabIndex = 0;
            // 
            // Tests
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(967, 598);
            this.Controls.Add(this.splitContainer1);
            this.Font = new System.Drawing.Font("Trebuchet MS", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Tests";
            this.Text = "Tests";
            this.Load += new System.EventHandler(this.Tests_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ListView testsList;
        private System.Windows.Forms.ListView categoriesList;
        private System.Windows.Forms.TextBox outputText;
        private System.Windows.Forms.ImageList testIcons16;
        private System.Windows.Forms.ColumnHeader TestHeader;
        private System.Windows.Forms.ColumnHeader TimeHeader;
    }
}

