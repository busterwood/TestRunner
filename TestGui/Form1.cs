using System;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.Generic;

namespace TestGui
{
    public partial class Tests : Form
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


        public Tests()
        {
            InitializeComponent();
        }

        internal TestdRunner Runner { get; set; }

        private void Tests_Load(object sender, EventArgs e)
        {
            this.Text = Runner.AsmName;
            SetDoubleBuffer(testsList);

            passedGrp = testsList.Groups.Cast<ListViewGroup>().First(grp => grp.Name.Equals("passedGroup", StringComparison.OrdinalIgnoreCase));
            failedGrp = testsList.Groups.Cast<ListViewGroup>().First(grp => grp.Name.Equals("failedGroup", StringComparison.OrdinalIgnoreCase));
            ignoredGrp = testsList.Groups.Cast<ListViewGroup>().First(grp => grp.Name.Equals("ignoredGroup", StringComparison.OrdinalIgnoreCase));
            slowGrp = testsList.Groups.Cast<ListViewGroup>().First(grp => grp.Name.Equals("SlowGroup", StringComparison.OrdinalIgnoreCase));

            allFilter = categoriesList.Items.Cast<ListViewItem>().First(item => Equals(item.Tag, "all"));
            passedFilter = categoriesList.Items.Cast<ListViewItem>().First(item => Equals(item.Tag, "passedGroup"));
            failedFilter = categoriesList.Items.Cast<ListViewItem>().First(item => Equals(item.Tag, "failedGroup"));
            ignoredFilter = categoriesList.Items.Cast<ListViewItem>().First(item => Equals(item.Tag, "ignoredGroup"));
            slowFilter = categoriesList.Items.Cast<ListViewItem>().First(item => Equals(item.Tag, "slowGroup"));

            Runner.RunStarted += RunStarted;
            Runner.RunFinished += RunFinished;
            Runner.Tested += Tested;
            Runner.Start();
        }

        private void SetDoubleBuffer(Control ctrl)
        {
            ctrl
               .GetType()
               .GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic)
               .SetValue(ctrl, true, null);
        }

        private void RunStarted(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke((EventHandler)RunStarted, sender, e);
                return;
            }
            testsList.Cursor = Cursors.AppStarting;
            passedFilter.Text = $"Passed";
            failedFilter.Text = $"Failed";
            ignoredFilter.Text = $"Ignored";
            slowFilter.Text = $"Slow";

            testsList.Items.Clear();
            slowCount = 0;
        }

        private void RunFinished(object sender, RunFinishedEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke((EventHandler<RunFinishedEventArgs>)RunFinished, sender, e);
                return;
            }
            passedFilter.Text = $"Passed ({e.Passed})";
            failedFilter.Text = $"Failed ({e.Failed})";
            ignoredFilter.Text = $"Ignored ({e.Ignored})";
            slowFilter.Text = $"Slow ({slowCount})";
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
        }

        private void AddTestItem(TestEventArgs e)
        {
            var li = new ListViewItem(e.TestName);
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
            if (e.Elapsed.HasValue)
                li.SubItems.Add(e.Elapsed.Value.TotalMilliseconds.ToString("N0"));
            li.Tag = e.Output;
            testsList.Items.Add(li);
        }

        private void AddSlowItem(TestEventArgs e)
        {
            var li = new ListViewItem(e.TestName);
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
            li.Group = slowGrp;
            li.SubItems.Add(e.Elapsed.Value.TotalMilliseconds.ToString("N0"));
            li.Tag = e.Output;
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
                if (grp.Name.Equals(groupName, StringComparison.OrdinalIgnoreCase))
                    grp.Expand();
                else
                    grp.Collapse();

            }
        }

    }
}
