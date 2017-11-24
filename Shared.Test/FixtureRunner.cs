using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Test
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

        public int CountTests()
        {
            int count = 0;
            foreach (var test in methods)
            {
                if (test.IsIgnored() || test.IsExplicit())
                    continue;
                if (test.IsTest())
                    count++;
                else
                    count += test.CustomAttributes.Where(a => a.IsTestCase()).Count();
            }
            return count;
        }

        public IEnumerable<string> TestNames()
        {
            foreach (var testMethod in methods)
            {
                if (testMethod.IsIgnored() || testMethod.IsExplicit())
                {
                    yield return $"IGNORED: {fixture.Name}.{testMethod.Name}";
                }
                else if (testMethod.IsTest())
                {
                    yield return $"{fixture.Name}.{testMethod.Name}";
                }
                else foreach (var testCase in testMethod.CustomAttributes.Where(a => a.IsTestCase()))
                {
                    var args = TestCaseArgs(testCase);
                    var testName = TestCaseName(testMethod, args);
                    yield return $"{fixture.Name}.{testName}";
                }
            }
        }

        public void RunTests()
        {
            obj = Activator.CreateInstance(fixture);
            fixtureSetupFailed = FixtureSetUp() == false;

            foreach (var testMethod in methods)
            {
                if (testMethod.IsIgnored() || testMethod.IsExplicit())
                {
                    StdOut.Ignore($"{fixture.Name}.{testMethod.Name}");
                    ignored++;
                    testCount++;
                }
                else if (testMethod.IsTest())
                {
                    testCount++;
                    var test = CreateTest(testMethod);
                    RunTest(test);
                }
                else foreach (var testCase in testMethod.CustomAttributes.Where(a => a.IsTestCase()))
                {
                    testCount++;
                    var test = CreateTestCase(testMethod, testCase);
                    RunTest(test);
                }
            }
            FixtureTearDown();
        }

        internal void RunTest(string testName)
        {
            foreach (var testMethod in methods)
            {
                if (testMethod.IsTest() && testMethod.Name.Equals(testName, StringComparison.OrdinalIgnoreCase))
                {
                    var test = CreateTest(testMethod);
                    SetupRunTestTearDown(test);
                    break;
                }
                foreach (var testCase in testMethod.CustomAttributes.Where(a => a.IsTestCase()))
                {
                    var test = CreateTestCase(testMethod, testCase);
                    if (test.Name.Equals(testName, StringComparison.OrdinalIgnoreCase))
                    {
                        SetupRunTestTearDown(test);
                        break;
                    }
                }
            }
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

        TestRunner CreateTest(MethodInfo testMethod)
        {
            return new TestRunner(fixture.Name, testMethod.Name, obj, setup, tearDown, testMethod, watch, null);
        }

        TestRunner CreateTestCase(MethodInfo testMethod, CustomAttributeData testCase)
        {
            var args = TestCaseArgs(testCase);
            var testName = TestCaseName(testMethod, args);
            return new TestRunner(fixture.Name, testName, obj, setup, tearDown, testMethod, watch, args);
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
