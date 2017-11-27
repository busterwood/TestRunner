using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Tests
{
    class FixtureRunner
    {
        readonly Fixture fixture;
        object obj;
        int testCount;
        int passed;
        int failed;
        int ignored;
        Stopwatch watch;
        bool fixtureSetupFailed;

        public Stats Statistics => new Stats(testCount, passed, failed, ignored);

        public string Name => fixture.Name;

        public FixtureRunner(Fixture fixture)
        {
            this.fixture = fixture;
            watch = new Stopwatch();
        }

        private IEnumerable<Test> Tests() => fixture.Tests();

        public void RunTests()
        {
            obj = Activator.CreateInstance(fixture.Type);
            fixtureSetupFailed = FixtureSetUp() == false;

            foreach (var test in Tests())
            {
                if (test.Ignored || test.Explicit)
                {
                    StdOut.Ignore($"{fixture.Name}.{test.Name}");
                    ignored++;
                    testCount++;
                }
                else 
                {
                    var t = new TestRunner(test, fixture, obj, watch);
                    RunTest(t);
                }
            }
            FixtureTearDown();
        }

        internal void RunSingleTest(string testName)
        {
            var test = Tests().FirstOrDefault(t => t.Name.StartsWith(testName, StringComparison.Ordinal));
            if (test != null)
                SetupRunTestTearDown(test);
        }

        private void SetupRunTestTearDown(Test test)
        {
            obj = Activator.CreateInstance(fixture.Type);
            fixtureSetupFailed = FixtureSetUp() == false;
            if (fixtureSetupFailed)
                return;
            var t = new TestRunner(test, fixture, obj, watch);
            RunTest(t);
            FixtureTearDown();
        }

        private void RunTest(TestRunner test)
        {
            if (fixtureSetupFailed)
            {
                test.FixtureSetupFailed();
                failed++;
                return;
            }

            test.Run();

            if (test.Passed)
                passed++;
            else if (test.Failed)
                failed++;
        }

        private bool FixtureSetUp()
        {
            try
            {
                if (fixture.FixtureSetup!= null)
                    fixture.FixtureSetup.Invoke(obj, null);
                return true;
            }
            catch (TargetInvocationException ex)
            {
                ReportError(ex.InnerException);
                return false;
            }
            catch (Exception ex)
            {
                ReportError(ex);
                return false;
            }
        }

        private void ReportError(Exception ex)
        {
            var tlex = ex as ReflectionTypeLoadException;
            if (tlex != null)
            {
                foreach (var e in tlex.LoaderExceptions)
                {
                    StdErr.Info(e.ToString());
                }
            }

            StdErr.Error($"{fixture.Name}: Failed to setup fixture: {ex}");
        }

        bool FixtureTearDown()
        {
            try
            {
                if (fixture.FixtureTearDown != null)
                    fixture.FixtureTearDown.Invoke(obj, null);
                return true;
            }
            catch (TargetInvocationException ex)
            {
                StdErr.Error($"{fixture.Name}: Failed to tear down fixture: {ex.InnerException}");
                return false;
            }
            catch (Exception ex)
            {
                StdErr.Error($"{fixture.Name}: Failed to tear down fixture: {ex}");
                return false;
            }
        }
    }
}
