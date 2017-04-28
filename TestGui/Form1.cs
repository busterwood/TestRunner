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
        ListViewGroup passedGrp;
        ListViewGroup failedGrp;

        public Tests()
        {
            InitializeComponent();
        }

        internal TestdRunner Runner { get; set; }

        private void Tests_Load(object sender, EventArgs e)
        {
            this.Text = Runner.AsmName;
            DoubleBufferTestsList();
            passedGrp = testsList.Groups[1];
            failedGrp = testsList.Groups[0];

            Runner.RunStarted += RunStarted;
            Runner.RunFinished += RunFinished;
            Runner.Tested += Tested;
            Runner.Start();
        }

        private void DoubleBufferTestsList()
        {
            testsList
               .GetType()
               .GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic)
               .SetValue(testsList, true, null);
        }

        private void RunStarted(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke((EventHandler)RunStarted, sender, e);
                return;
            }
            testsList.Cursor = Cursors.AppStarting;
            testsList.Items.Clear();
        }

        private void RunFinished(object sender, RunFinishedEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke((EventHandler<RunFinishedEventArgs>)RunFinished, sender, e);
                return;
            }
            passedGrp.Header = $"Passed ({e.Passed})";
            failedGrp.Header = $"Failed ({e.Passed})";
            testsList.Cursor = Cursors.Default;
        }

        private void Tested(object sender, TestEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke((EventHandler<TestEventArgs>)Tested, sender, e);
                return;
            }

            const int TickImageIdx = 0;
            const int CrossImageIdx = 1;

            var li = new ListViewItem(e.TestName);
            if (e.Pass)
            {
                li.ImageIndex = TickImageIdx;
                li.Group = passedGrp;
            }
            else
            {
                li.ImageIndex = CrossImageIdx;
                li.Group = failedGrp;
            }
            li.Tag = e.Output;
            testsList.Items.Add(li);
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
    }
}
