using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
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
            //var tests = new Tests() { Runner = new TestdRunner(args) };
            var projects = new ProjectsForm();
            Application.Run(projects);
        }

        [DllImport("shlwapi.dll", CharSet = CharSet.Auto)]
        public static extern bool PathRelativePathTo(
            [Out] StringBuilder pszPath,
            [In] string pszFrom,
            [In] FileAttributes dwAttrFrom,
            [In] string pszTo,
            [In] FileAttributes dwAttrTo
        );
    }
}
