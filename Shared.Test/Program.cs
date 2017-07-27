using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using BusterWood.Collections;

namespace Test
{
    class Program
    {
        private const string TestDomain = "test-runner";

        static int Main(string[] argv)
        {
            var sw = new Stopwatch();
            sw.Start();

            List<string> args = argv.ToList();
            bool debug = args.Remove("--debug");
            if (args.Count == 0)
            {
                StdErr.Error("pass the assembly to test on the command line");
                return Exit(1);
            }

            SetCustomConfigFile(args[0]);

            var asm = LoadTestAssembly(args[0]);
            if (asm == null)
                return Exit(2);

            var fixtureTypes = FindFixutres(asm);
            if (fixtureTypes == null)
                return Exit(3);

            List<FixtureRunner> fixtures = CreateFixtureRunners(fixtureTypes);

            StdErr.Info($"{fixtures.Sum(f => f.CountTests())} test, {fixtures.Count} fixtures" + (debug ? ", debug" : ""));

            if (debug && !Debugger.IsAttached)
            {
                StdErr.Info("Waiting for debugger to attach...");
                while (!Debugger.IsAttached)
                    Thread.Sleep(100);
            }
            else
                StdErr.Info("Debugger is attached");

            var setupFixture = CreateSetUpFixtureRunner(asm);
            if (setupFixture != null)
            {
                setupFixture.SetUp();
                if (setupFixture.Failed)
                    return Exit(4);
            }

            Stats totals = Stats.Zero;
            foreach (var fixture in fixtures)
            {
                totals += RunFixture(fixture);
            }
            sw.Stop();

            //setupFixture.TearDown();
            //if (setupFixture.Failed)
            //    return 5;

            StdErr.Info($"Totals: {totals.Tests} tests, {totals.Passed} passed, {totals.Failed} failed, {totals.Ignored} ignored, in {sw.Elapsed.TotalSeconds:N1} seconds");
#if DEBUG
            if (Debugger.IsAttached)
                Debugger.Break();
#endif
            return Exit(0);
        }

        private static int Exit(int code)
        {
            Environment.Exit(code); // force exit, even if there are still running threads
            return code;
        }

        private static SetUpFixtureRunner CreateSetUpFixtureRunner(Assembly testAsm)
        {
            var setupFixture = testAsm.GetExportedTypes()
                    .Where(t => !t.IsAbstract && t.IsSetUpFixture())
                    .FirstOrDefault();
            return setupFixture == null ? null : new SetUpFixtureRunner(setupFixture);
        }

        static int RunMainInTestDomain(string[] args)
        {
            var setup = new AppDomainSetup { ApplicationBase = Environment.CurrentDirectory, ShadowCopyFiles = "true" };

            var configFilePath = Path.Combine(Directory.GetCurrentDirectory(), args[0] + ".dll.config");
            if (File.Exists(configFilePath))
                setup.ConfigurationFile = configFilePath;

            var testDomain = AppDomain.CreateDomain(TestDomain, AppDomain.CurrentDomain.Evidence, setup);
            return testDomain.ExecuteAssembly(Assembly.GetExecutingAssembly().Location, args);
        }

        private static List<FixtureRunner> CreateFixtureRunners(IList<Type> fixtureTypes)
        {
            var runners = new List<FixtureRunner>(fixtureTypes.Count);
            foreach (var type in fixtureTypes)
            {
                try
                {
                    runners.Add(new FixtureRunner(type));
                }
                catch (Exception ex)
                {
                    StdErr.Error($"Cannot create fixture runner for '{type.FullName}': {ex}");
                }
            }
            return runners;
        }

        private static void SetCustomConfigFile(string asmName)
        {
            var configFilePath = Path.Combine(Directory.GetCurrentDirectory(), asmName + ".dll.config");
            if (File.Exists(configFilePath))
                AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", configFilePath);
        }

        private static Stats RunFixture(FixtureRunner fixture)
        {
            try
            {
                fixture.RunTests();
                return fixture.Statistics;
            }
            catch (Exception ex)
            {
                StdErr.Error($"Cannot run fixture '{fixture.Type.FullName}': {ex}");
                return Stats.Zero;
            }
        }

        private static Assembly LoadTestAssembly(string asmName)
        {
            try
            {
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
                var path = Path.Combine(Directory.GetCurrentDirectory(), asmName + ".dll");
                return Assembly.LoadFile(path);
                
            }
            catch (Exception ex)
            {
                StdErr.Error($"Failed to load test assembly '{asmName}': {ex}");
                return null;
            }
        }

        static readonly Dictionary<string, bool> previousLoadAttempt = new Dictionary<string, bool>();

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var simpleName = args.Name.Split(',').First();
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), simpleName + ".dll");
            Assembly loaded = null;
            if (File.Exists(fullPath))
                loaded = Assembly.LoadFile(fullPath);
            else
                fullPath = Path.Combine(Directory.GetCurrentDirectory(), simpleName + ".exe");
            if (File.Exists(fullPath))
                loaded = Assembly.LoadFile(fullPath);
            if (loaded == null && !previousLoadAttempt.ContainsKey(simpleName))
                StdErr.Info($"Failed to load assembly '{args.Name}' requested by {args.RequestingAssembly?.FullName}");
            previousLoadAttempt[simpleName] = loaded != null;
            return loaded;
        }

        private static UniqueList<Type> FindFixutres(Assembly testAsm)
        {
            try
            {
                return testAsm
                    .GetExportedTypes()
                    .Where(type => !type.IsAbstract && type.IsTestFixture() && !type.IsIgnored() && !type.IsExplicit())
                    .ToUniqueList();
            }
            catch (Exception ex)
            {
                StdErr.Error($"Failed to find test fixtures: {ex}");
                return null;
            }
        }

    }
}
