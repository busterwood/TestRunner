using System;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.Generic;
using static System.StringComparison;

namespace TestGui
{
    public partial class TestFixtureForm : Form
    {
        const int TickImageIdx = 0;
        const int CrossImageIdx = 1;
        const int IgnoredImageIdx = 3;

        ListViewGroup passedGrp;
        ListViewGroup failedGrp;
        ListViewGroup ignoredGrp;
        ListViewGroup slowGrp;
        ListViewItem allFilter;
        ListViewItem passedFilter;
        ListViewItem failedFilter;
        ListViewItem ignoredFilter;
        ListViewItem slowFilter;
        int slowCount;

        public TestFixtureForm()
        {
            InitializeComponent();
        }

        internal TestdRunner Runner { get; set; }

        private void Tests_Load(object sender, EventArgs e)
        {
            this.Text = Runner.AsmName;
            testsList.SetDoubleBuffer();

            passedGrp = testsList.Groups.Cast<ListViewGroup>().First(grp => grp.Name.Equals("passedGroup", OrdinalIgnoreCase));
            failedGrp = testsList.Groups.Cast<ListViewGroup>().First(grp => grp.Name.Equals("failedGroup", OrdinalIgnoreCase));
            ignoredGrp = testsList.Groups.Cast<ListViewGroup>().First(grp => grp.Name.Equals("ignoredGroup", OrdinalIgnoreCase));
            slowGrp = testsList.Groups.Cast<ListViewGroup>().First(grp => grp.Name.Equals("SlowGroup", OrdinalIgnoreCase));

            allFilter = categoriesList.Items.Cast<ListViewItem>().First(item => string.Equals((string)item.Tag, "all", OrdinalIgnoreCase));
            passedFilter = categoriesList.Items.Cast<ListViewItem>().First(item => string.Equals((string)item.Tag, "passedGroup", OrdinalIgnoreCase));
            failedFilter = categoriesList.Items.Cast<ListViewItem>().First(item => string.Equals((string)item.Tag, "failedGroup", OrdinalIgnoreCase));
            ignoredFilter = categoriesList.Items.Cast<ListViewItem>().First(item => string.Equals((string)item.Tag, "ignoredGroup", OrdinalIgnoreCase));
            slowFilter = categoriesList.Items.Cast<ListViewItem>().First(item => string.Equals((string)item.Tag, "slowGroup", OrdinalIgnoreCase));

            Runner.RunStarted += RunStarted;
            Runner.RunFinished += RunFinished;
            Runner.TestStarted += TestStarted;
            Runner.Tested += Tested;
            Runner.Start();

            statusText.Text = "Monitoring " + Runner.Folder;
        }

        private void TestStarted(object sender, TestEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke((EventHandler<TestEventArgs>)TestStarted, sender, e);
                return;
            }
            statusText.Text = $"Running {e.TestFixure}.{e.TestName}...";
        }

        private void RunStarted(object sender, RunStartedEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke((EventHandler<RunStartedEventArgs>)RunStarted, sender, e);
                return;
            }
            testsList.Cursor = Cursors.AppStarting;
            runTestsAgainMenuItem.Enabled = false;
            debugTestsToolStripMenuItem.Enabled = false;
            allFilter.Text = $"All";
            passedFilter.Text = $"Passed";
            failedFilter.Text = $"Failed";
            ignoredFilter.Text = $"Ignored";
            slowFilter.Text = $"Slow";
            testsList.Items.Clear();
            statusProgress.Value = 0;
            statusProgress.Maximum = e.Total;
            if (e.Debug)
                statusText.Text = $"Waiting for debugger to attach, {e.Total} tests";
            else
                statusText.Text = $"Running {e.Total} tests";
            slowCount = 0;
            outputText.Text = "";
        }

        private void RunFinished(object sender, RunFinishedEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke((EventHandler<RunFinishedEventArgs>)RunFinished, sender, e);
                return;
            }
            allFilter.Text = $"All ({e.Total})";
            passedFilter.Text = $"Passed ({e.Passed})";
            failedFilter.Text = $"Failed ({e.Failed})";
            ignoredFilter.Text = $"Ignored ({e.Ignored})";
            slowFilter.Text = $"Slow ({slowCount})";
            statusProgress.Value = 0;
            statusText.Text = "Monitoring " + Runner.Folder;
            runTestsAgainMenuItem.Enabled = true;
            debugTestsToolStripMenuItem.Enabled = true;
            testsList.Cursor = Cursors.Default;
        }

        private void Tested(object sender, TestEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke((EventHandler<TestEventArgs>)Tested, sender, e);
                return;
            }

            AddTestItem(e);
            if (e.Elapsed > TimeSpan.FromSeconds(1))
                AddSlowItem(e);
            if (statusProgress.Value < statusProgress.Maximum)
                statusProgress.Value++;
        }

        private void AddTestItem(TestEventArgs e)
        {
            var li = new ListViewItem(e.TestName);
            li.SubItems.Add(e.TestFixure);
            switch (e.Result)
            {
                case TestResult.Pass:
                    li.ImageIndex = TickImageIdx;
                    li.Group = passedGrp;
                    break;
                case TestResult.Fail:
                    li.ImageIndex = CrossImageIdx;
                    li.Group = failedGrp;
                    break;
                case TestResult.Ignored:
                    li.ImageIndex = IgnoredImageIdx;
                    li.Group = ignoredGrp;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            li.SubItems.Add(e.Elapsed.HasValue ? e.Elapsed.Value.TotalMilliseconds.ToString("N0") : "");
            li.SubItems.Add(e.Output.Count > 0 ? "Yes" : "");
            li.SubItems.Add(e.Category ?? "");
            li.Tag = e.Output;
            testsList.Items.Add(li);
        }

        private void AddSlowItem(TestEventArgs e)
        {
            var li = new ListViewItem(e.TestName);
            li.SubItems.Add(e.TestFixure);
            switch (e.Result)
            {
                case TestResult.Pass:
                    li.ImageIndex = TickImageIdx;
                    break;
                case TestResult.Fail:
                    li.ImageIndex = CrossImageIdx;
                    break;
                case TestResult.Ignored:
                    li.ImageIndex = IgnoredImageIdx;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            li.SubItems.Add(e.Elapsed.Value.TotalMilliseconds.ToString("N0"));
            li.Tag = e.Output;
            li.Group = slowGrp;
            testsList.Items.Add(li);
            slowCount++;
        }

        private void testsList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (testsList.SelectedItems.Count == 0)
            {
                outputText.Text = "";
                return;
            }

            var item = testsList.SelectedItems[0];
            var lines = (List<string>)item.Tag;
            outputText.Text = string.Join(Environment.NewLine, lines);
        }

        private void categoriesList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (categoriesList.SelectedItems.Count == 0 || Equals("all", categoriesList.SelectedItems[0].Tag))
            {
                ExpandTestAllGroups();
            }
            else
            {
                ExpandTestGroup(categoriesList.SelectedItems[0].Tag.ToString());
            }
        }

        private void ExpandTestAllGroups()
        {
            foreach(ListViewGroup grp in testsList.Groups)
            {
                grp.Expand();
            }
        }

        private void ExpandTestGroup(string groupName)
        {
            foreach (ListViewGroup grp in testsList.Groups)
            {
                if (grp.Name.Equals(groupName, OrdinalIgnoreCase))
                    grp.Expand();
                else
                    grp.Collapse();
            }
        }

        private void detailsMenuItem_Click(object sender, EventArgs e)
        {
            testsList.View = View.Details;
        }

        private void listMenuItem_Click(object sender, EventArgs e)
        {
            testsList.View = View.Tile;
        }

        private void runTestsAgainMenuItem_Click(object sender, EventArgs e)
        {
            runTestsAgainMenuItem.Enabled = false;
            debugTestsToolStripMenuItem.Enabled = false;
            Runner.RunTests();
        }

        private void TestFixtureForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Runner.Stop();
        }

        private void testsList_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (testsList.SelectedItems.Count == 0)
                return;

            var item = testsList.SelectedItems[0];
            var lines = (List<string>)item.Tag;
            if (lines.Count > 0 && splitContainer1.Panel2Collapsed)
            {
                splitContainer1.Panel2Collapsed = false;
                this.Width *= 2;
            }
        }

        private void debugTestsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            runTestsAgainMenuItem.Enabled = false;
            debugTestsToolStripMenuItem.Enabled = false;
            Runner.DebugTests();
        }
    }
}
