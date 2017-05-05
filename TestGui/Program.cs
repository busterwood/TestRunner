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
            var tests = new Tests() { Runner = new TestdRunner(args) };
            Application.Run(tests);
        }
    }
}
