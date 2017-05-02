using System;
using System.Windows.Forms;

namespace TestGui
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            TestdRunner runner = new TestdRunner(args);
            var tests = new Tests() { Runner=runner };
            Application.Run(tests);
        }
    }
}
