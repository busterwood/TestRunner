using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Tests
{
    class FixtureRunner
    {
        readonly Type fixture;
        object obj;
        readonly MethodInfo[] methods;
        readonly MethodInfo fixtureSetup;
        readonly MethodInfo fixtureTearDown;
        readonly MethodInfo setup;
        readonly MethodInfo tearDown;
        object fixtureTimeoutMs;
        int testCount;
        int passed;
        int failed;
        int ignored;
        Stopwatch watch;
        bool fixtureSetupFailed;

        public Stats Statistics => new Stats(testCount, passed, failed, ignored);

        public Type Type => fixture;

        public FixtureRunner(Type fixture)
        {
            this.fixture = fixture;
            methods = fixture.GetMethods();
            fixtureSetup = methods.FirstOrDefault(m => m.IsTestFixtureSetUp());
            fixtureTearDown = methods.FirstOrDefault(m => m.IsTestFixtureTearDown());
            setup = methods.FirstOrDefault(m => m.IsSetup());
            tearDown = methods.FirstOrDefault(m => m.IsTearDown());
            fixtureTimeoutMs = fixture.CustomAttributes.FirstOrDefault(a => a.IsTimeout())?.ConstructorArguments?.First().Value;
            watch = new Stopwatch();
        }

        public int CountTests() => Tests().Where(test => !test.Ignored && !test.Explicit).Count();

        public IEnumerable<string> TestNames() => Tests().Select(t => $"{fixture.Name}.{t.Name}");

        private IEnumerable<TestRunner> Tests()
        {
            foreach (var method in methods)
            {
                var attrsByName = method.CustomAttributes.ToLookup(a => a.AttributeType.Name);
                if (attrsByName.IsTest())
                {
                    yield return CreateTest(method, attrsByName);
                }
                else
                {
                    foreach (var testCase in attrsByName["TestCaseAttribute"])
                    {
                        yield return CreateTestCase(method, testCase, attrsByName);
                    }
                }
            }
        }

        public void RunTests()
        {
            obj = Activator.CreateInstance(fixture);
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
                    RunTest(test);
                }
            }
            FixtureTearDown();
        }

        internal void RunTest(string testName)
        {
            var test = Tests().FirstOrDefault(t => t.Name.StartsWith(testName, StringComparison.Ordinal));
            if (test != null)
                SetupRunTestTearDown(test);
        }

        private void SetupRunTestTearDown(TestRunner test)
        {
            obj = Activator.CreateInstance(fixture);
            fixtureSetupFailed = FixtureSetUp() == false;
            if (fixtureSetupFailed)
                return;
            test.Obj = obj;
            RunTest(test);
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
                if (fixtureSetup != null)
                    fixtureSetup.Invoke(obj, null);
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

        TestRunner CreateTest(MethodInfo testMethod, ILookup<string, CustomAttributeData> attrsByName)
        {
            return new TestRunner(fixture.Name, testMethod.Name, obj, setup, tearDown, testMethod, watch, null, attrsByName);
        }

        TestRunner CreateTestCase(MethodInfo testMethod, CustomAttributeData testCase, ILookup<string, CustomAttributeData> attrsByName)
        {
            var args = TestCaseArgs(testCase);
            var testName = TestCaseName(testMethod, args);
            return new TestRunner(fixture.Name, testName, obj, setup, tearDown, testMethod, watch, args, attrsByName);
        }

        static object[] TestCaseArgs(CustomAttributeData testCase)
        {
            return testCase.ConstructorArguments.Select(arg => arg.Value).ToArray();
        }

        static string TestCaseName(MethodInfo test, object[] args)
        {
            return test.Name + "(" + string.Join(",", args.Select(a => a is string ? '"' + ReplaceSpecialChars(a.ToString()) + '"' : a)) + ")";
        }

        private static string ReplaceSpecialChars(string s)
        {
            return s.Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");
        }

        bool FixtureTearDown()
        {
            try
            {
                if (fixtureTearDown != null)
                    fixtureTearDown.Invoke(obj, null);
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
