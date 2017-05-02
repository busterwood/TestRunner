using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    class Program
    {
        static int Main(string[] argv)
        {
            var sw = new Stopwatch();
            sw.Start();

            List<string> args = argv.ToList();
            //TODO: remove switches
            if (args.Count == 0)
            {
                StdErr.Error("pass the assembly to test on the command line");
                return 1;
            }

            SetCustomConfigFile(args[0]);

            var asm = LoadTestAssembly(args[0]);
            if (asm == null)
                return 2;

            List<Type> fixtures = FindFixutres(asm);
            if (fixtures == null)
                return 3;

            StdErr.Info($"Found {fixtures.Count} test fixtures to run");

            Stats totals = Stats.Zero;    
            foreach (var fixture in fixtures)
            {
                totals += RunFixture(fixture);
            }
            sw.Stop();
            StdErr.Info($"Totals: {totals.Tests} tests, {totals.Passed} passed, {totals.Failed} failed, in {sw.Elapsed.TotalSeconds:N1} seconds");

            if (Debugger.IsAttached)
                Debugger.Break();
            return 0;
        }

        private static void SetCustomConfigFile(string asmName)
        {
            var configFilePath = Path.Combine(Directory.GetCurrentDirectory(), asmName, ".dll.config");
            if (File.Exists("configFilePath"))
                AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", configFilePath);
        }

        private static Stats RunFixture(Type fixture)
        {
            try
            {
                var f = new FixtureRunner(fixture);
                f.RunTests();
                return f.Statistics;
            }
            catch (Exception ex)
            {
                StdErr.Error($"Cannot run fixture '{fixture.FullName}': {ex}");
                return Stats.Zero;
            }
        }

        private static Assembly LoadTestAssembly(string asmName)
        {
            try
            {
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
                return Assembly.LoadFile(Path.Combine(Directory.GetCurrentDirectory(), asmName + ".dll"));
            }
            catch (Exception ex)
            {
                StdErr.Error($"Failed to load test assembly '{asmName}': {ex}");
                return null;
            }
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var simpleName = args.Name.Split(',').First();
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), simpleName + ".dll");
            Assembly loaded = null;
            if (File.Exists(fullPath))
                loaded = Assembly.LoadFile(fullPath);
            if (loaded == null)
                StdErr.Info($"Failed to load assembly '{args.Name}' requested by {args.RequestingAssembly.FullName}");
            return loaded;
        }

        private static List<Type> FindFixutres(Assembly testAsm)
        {
            try
            {
                return testAsm
                    .GetExportedTypes()
                    .Where(t => t.IsTestFixture() && !t.IsIgnored())
                    .ToList();
            }
            catch (Exception ex)
            {
                StdErr.Error($"Failed to find test fixtures: {ex}");
                return null;
            }
        }

    }
}
