using BusterWood.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
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
            projectsList.SetDoubleBuffer();
            FindProjects();
        }

        private void FindProjects()
        {
            projectsList.Cursor = Cursors.AppStarting;
            statusLabel.Text = "Loading...";
            projectsList.Items.Clear();
            projectFinder = new ProjectFinder();
            projectFinder.ProjectFound += ProjectFinder_ProjectFound;
            Task.Run(() => projectFinder.Find(Environment.CurrentDirectory))
                .ContinueWith(FindProjectsFinished);
        }

        private void FindProjectsFinished(Task obj)
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action<Task>)FindProjectsFinished, obj);
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
                var lastChanged = b.LastChangedUtc.ToString("u").TrimEnd('Z');
                var relPath = new StringBuilder(255);
                Program.PathRelativePathTo(relPath, Environment.CurrentDirectory, FileAttributes.Directory, b.Folder, FileAttributes.Directory);
                var li = new ListViewItem(new string[] { lastChanged, args.Project, relPath.ToString() });
                li.Tag = b;
                li.Group = GroupByAge(b.LastChangedUtc);
                li.ForeColor = ForeColorByAge(b.LastChangedUtc);
                b.Built += ProjectBuilt;
                projectsList.Items.Add(li);
            }
        }

        private ListViewGroup GroupByAge(DateTime lastChangedUtc)
        { 
            var age = DateTime.UtcNow - lastChangedUtc;
            if (age < TimeSpan.FromDays(4))
                return projectsList.Groups[0];
            if (age < TimeSpan.FromDays(4 * 7))
                return projectsList.Groups[1];
            return projectsList.Groups[2];
        }

        private Color ForeColorByAge(DateTime lastChangedUtc)
        {
            var age = DateTime.UtcNow - lastChangedUtc;
            if (age < TimeSpan.FromDays(4))
                return Color.Black;
            if (age < TimeSpan.FromDays(4 * 7))
                return Color.Gray;
            return Color.DarkGray;
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
            itemBuilt.Text = build.LastChangedUtc.ToString("u").TrimEnd('Z');
            itemBuilt.Group = GroupByAge(build.LastChangedUtc);
            itemBuilt.ForeColor = Color.Black;
            projectsList.Sort(); // modification time will have changed, force re-sorting
        }

        private void projectsList_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            foreach (ListViewItem selected in projectsList.SelectedItems)
                StartTesting(selected);
        }

        private static void StartTesting(ListViewItem selected)
        {
            Build b = (Build)selected.Tag;
            var args = new List<string>();
            if (b.X64)
                args.Add("--x64");
            else if (b.X86)
                args.Add("--x86");
            var asmName = selected.SubItems[1].Text;
            args.Add(asmName);
            var tests = new TestFixtureForm() { Runner = new TestdRunner(b.Folder, args.ToArray()) };
            tests.Show();
        }

        private void refreshMenu_Click(object sender, EventArgs e)
        {
            RefreshProjectsList();
        }

        private void RefreshProjectsList()
        {
            if (projectsList.Cursor != Cursors.Default)
                return; // still running
            FindProjects();
        }

        private void runConsoleMenu_Click(object sender, EventArgs e)
        {
            if (projectsList.SelectedItems.Count == 0)
                return;
            var selected = projectsList.SelectedItems[0];
            RunConsole(selected);
        }

        private void debugConsoleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (projectsList.SelectedItems.Count == 0)
                return;
            var selected = projectsList.SelectedItems[0];
            RunConsole(selected, "--debug");
        }

        private void RunConsole(ListViewItem selected, string extraArg = null)
        {
            Build b = (Build)selected.Tag;
            var args = new UniqueList<string>();
            if (b.X64)
                args.Add("--x64");
            else if (b.X86)
                args.Add("--x86");
            args.Add(extraArg);
            var asmName = selected.SubItems[1].Text;
            args.Add(asmName);

            var asm = Assembly.GetExecutingAssembly();
            var location = Path.GetDirectoryName(asm.Location);
            var si = new ProcessStartInfo
            {
                FileName = Path.Combine(location, "Testd.exe"),
                Arguments = string.Join(" ", args),
                WorkingDirectory = b.Folder,
            };
            Process.Start(si);
        }

        private void runSelectedMenu_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem selected in projectsList.SelectedItems)
                StartTesting(selected);
        }

    }
}
