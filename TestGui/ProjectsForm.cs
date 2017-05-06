﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestGui
{
    public partial class ProjectsForm : Form
    {
        List<FileSystemWatcher> buildWatchers = new List<FileSystemWatcher>();
        ProjectFinder projectFinder;

        public ProjectsForm()
        {
            InitializeComponent();
        }

        private void ProjectsForm_Load(object sender, EventArgs e)
        {
            projectsList.Cursor = Cursors.AppStarting;
            projectsList.SetDoubleBuffer();
            projectFinder = new ProjectFinder(Environment.CurrentDirectory);
            projectFinder.ProjectFound += ProjectFinder_ProjectFound;
            Task.Run(projectFinder.Find)
                .ContinueWith(ResetCursor);
        }

        private void ResetCursor(Task obj)
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action<Task>)ResetCursor, obj);
                return;
            }
            projectsList.Cursor = Cursors.Default;
            statusLabel.Text = $"Found {projectsList.Items.Count} test builds";
        }

        private void ProjectFinder_ProjectFound(object sender, FoundProjectEventArgs args)
        {
            if (InvokeRequired)
            {
                BeginInvoke((EventHandler<FoundProjectEventArgs>)ProjectFinder_ProjectFound, sender, args);
                return;
            }
            foreach(var b in args.Builds)
            {
                var lastChanged = b.LastChanged.ToString("u");
                var relPath = new StringBuilder(255);
                Program.PathRelativePathTo(relPath, Environment.CurrentDirectory, FileAttributes.Directory, b.Folder, FileAttributes.Directory);
                var li = new ListViewItem(new string[] { lastChanged, args.Project, relPath.ToString() });
                li.Tag = b;
                b.Built += ProjectBuilt;
                projectsList.Items.Add(li);
            }
        }

        private void ProjectBuilt(object sender, EventArgs args)
        {
            if (InvokeRequired)
            {
                BeginInvoke((EventHandler)ProjectBuilt, sender, args);
                return;
            }
            var build = (Build)sender;
            var itemBuilt = projectsList.Items.Cast<ListViewItem>().First(li => li.Tag == build);
            itemBuilt.Text = build.LastChanged.ToString("u");
            projectsList.Sort();
        }

        private void projectsList_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (projectsList.SelectedItems.Count == 0)
                return;
            var selected = projectsList.SelectedItems[0];
            Build b = (Build)selected.Tag;
            var args = new List<string>();
            if (b.Folder.IndexOf("x64", StringComparison.OrdinalIgnoreCase) > 0)
                args.Add("--x64");
            else if (b.Folder.IndexOf("x86", StringComparison.OrdinalIgnoreCase) > 0)
                args.Add("--x86");
            var asmName = selected.SubItems[1].Text;
            args.Add(asmName);
            var tests = new TestFixtureForm() { Runner = new TestdRunner(b.Folder, args.ToArray()) };
            tests.Show();
        }
    }
}