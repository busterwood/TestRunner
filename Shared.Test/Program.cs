﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using BusterWood.Collections;

namespace Tests
{
    class Program
    {
        private const string TestDomain = "test-runner";

        static int Main(string[] argv)
        {
            var sw = new Stopwatch();
            sw.Start();

            Console.CancelKeyPress += (sender, e) => Exit(99);

            List<string> args = argv.ToList();
            bool list = args.Remove("--list");
            bool debug = args.Remove("--debug");
            string testToRun = args.StringValue("--run");
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

            List<Fixture> fixtures = CreateFixtures(fixtureTypes);

            if (list)
            {
                ListAllTests(fixtures);
                return Exit(0);
            }

            if (debug)
                WaitForDebuggerToAttach();

            var setupFixture = CreateSetUpFixtureRunner(asm);
            if (setupFixture != null)
            {
                setupFixture.SetUp();
                if (setupFixture.Failed)
                    return Exit(4);
            }

            if (testToRun != null)
                RunOneTest(testToRun, fixtures);
            else
            {
                StdErr.Info($"{fixtures.Sum(f => f.Tests().Count())} test, {fixtures.Count} fixtures" + (debug ? ", debug" : ""));
                RunAllTests(sw, fixtures);
            }

#if DEBUG
            if (Debugger.IsAttached)
                Debugger.Break();
#endif
            return Exit(0);
        }

        private static void WaitForDebuggerToAttach()
        {
            if (Debugger.IsAttached)
            {
                StdErr.Info("Debugger is attached");
                return;
            }

            StdErr.Info("Waiting for debugger to attach...");
            while (!Debugger.IsAttached)
                Thread.Sleep(100);
        }

        private static void ListAllTests(List<Fixture> fixtures)
        {
            foreach (var fr in fixtures)
            {
                foreach (var test in fr.Tests())
                {
                    Console.WriteLine(test.Name);
                }
            }
        }

        private static void RunOneTest(string testToRun, List<Fixture> fixtures)
        {
            var bits = testToRun.Split('.');

            IEnumerable<Fixture> fix = fixtures;
            string testName;
            if (bits.Length == 2)
            {
                fix = fix.Where(f => string.Equals(f.Type.Name, bits[0], StringComparison.OrdinalIgnoreCase));
                testName = bits[1];
            }
            else
            {
                testName = bits[0];
            }
            fix = fix.Where(f => f.Tests().Any(t => t.Name.IndexOf(testName, StringComparison.OrdinalIgnoreCase) >= 0));
            foreach (var f in fix)
            {
                var fr = new FixtureRunner(f);
                fr.RunSingleTest(testName);
            }
        }

        private static void RunAllTests(Stopwatch sw, List<Fixture> fixtures)
        {
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

        private static List<Fixture> CreateFixtures(IList<Type> fixtureTypes)
        {
            var runners = new List<Fixture>(fixtureTypes.Count);
            foreach (var type in fixtureTypes)
            {
                try
                {
                    runners.Add(new Fixture(type));
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

        private static Stats RunFixture(Fixture fixture)
        {
            try
            {
                var fr = new FixtureRunner(fixture);
                fr.RunTests();
                return fr.Statistics;
            }
            catch (Exception ex)
            {
                StdErr.Error($"Cannot run fixture '{fixture.Name}': {ex}");
                return Stats.Zero;
            }
        }

        private static Assembly LoadTestAssembly(string asmName)
        {
            try
            {
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
                var path = Path.Combine(Directory.GetCurrentDirectory(), asmName + ".dll");
                return Assembly.LoadFrom(path);
                
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

    static class Extensions
    {
        public static string StringValue(this List<string> list, string argName, string @default = null)
        {
            var idx = list.IndexOf(argName);
            if (idx < 0)
                return @default;

            list.RemoveAt(idx); // remove argname

            if (idx == list.Count) // is the value missing?  i.e. argName was the last value in the list?
                return @default;

            var result = list[idx];
            list.RemoveAt(idx); // remove value
            return result;
        }
    }
}
