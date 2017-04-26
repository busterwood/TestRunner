using System;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Test
{
    class FixtureRunner
    {
        readonly Type fixture;
        object obj;
        readonly MethodInfo[] methods;
        readonly MethodInfo setup;
        readonly MethodInfo tearDown;
        object fixtureTimeoutMs;
        int testCount;
        int passed;
        int failed;

        public Stats Statistics => new Stats(testCount, passed, failed);

        public FixtureRunner(Type fixture)
        {
            this.fixture = fixture;
            methods = fixture.GetMethods();
            setup = methods.FirstOrDefault(m => m.IsSetup());
            tearDown = methods.FirstOrDefault(m => m.IsTearDown());
            fixtureTimeoutMs = fixture.CustomAttributes.FirstOrDefault(a => a.IsTimeout())?.ConstructorArguments?.First().Value;
        }

        public void RunTests()
        {
            foreach (var test in methods)
            {
                if (test.IsTest())
                {
                    obj = Activator.CreateInstance(fixture);
                    testCount++;
                    RunTestLifeCycle(test, null, test.Name);
                    continue;
                }
                foreach (var testCase in test.CustomAttributes.Where(a => a.IsTestCase()))
                {
                    obj = Activator.CreateInstance(fixture);
                    testCount++;
                    var args = TestCaseArgs(testCase);
                    var testName = TestCaseName(test, args);
                    RunTestLifeCycle(test, args, testName);
                }
            }
        }

        private static object[] TestCaseArgs(CustomAttributeData testCase)
        {
            return testCase.ConstructorArguments.Select(arg => arg.Value).ToArray();
        }

        private static string TestCaseName(MethodInfo test, object[] args)
        {
            return test.Name + "(" + string.Join(",", args.Select(a => a is string ? '"' + a.ToString() + '"' : a)) + ")";
        }

        private void RunTestLifeCycle(MethodInfo test, object[] args, string testName)
        {
            if (!SetUp(test))
                return;
            object timeout = TestTimeout(test) ?? fixtureTimeoutMs;
            if (timeout != null)
                RunTestWithTimeout(test, args, testName, timeout);
            else
                RunTest(test, args, testName);

            TearDown(test);
        }

        private object TestTimeout(MethodInfo test)
        {
            return test.CustomAttributes
                .FirstOrDefault(a => a.IsTimeout())
                ?.ConstructorArguments
                ?.First().Value;
        }

        private void RunTestWithTimeout(MethodInfo test, object[] args, string testName, object timeoutMs)
        {
            Thread testThread = new Thread(() => RunTest(test, args, testName));
            testThread.Start();

            if (testThread.Join((int)timeoutMs))
                return; // all good
            
            testThread.Interrupt();
            if (testThread.Join(100))
                return; // stopped after interuption

            // last chance - aborting may leave unrealased locks 
            testThread.Abort();
            testThread.Join(100);
        }

        private void RunTest(MethodInfo test, object[] args, string testName)
        {
            try
            {
                test.Invoke(obj, args);
                Pass(testName);
            }
            catch (TargetInvocationException ex) when (ex.InnerException.GetType().Name.Equals("AssertionException", StringComparison.Ordinal))
            {
                Fail(testName, ex.InnerException.Message);
            }
            catch (TargetInvocationException ex)
            {
                Fail(testName, ex.InnerException);
            }
            catch (Exception ex)
            {
                Fail(testName, ex);
            }
        }

        private void Pass(string testName)
        {
            StdOut.Passed($"{fixture.Name}.{testName}");
            passed++;
        }

        private void Fail(string testName, Exception ex)
        {
            StdOut.Fail($"{fixture.Name}.{testName}: {ex}");
            failed++;
        }

        private void Fail(string testName, string message)
        {
            StdOut.Fail($"{fixture.Name}.{testName}: {message}");
            failed++;
        }

        private bool SetUp(MethodInfo test)
        {
            try
            {
                if (setup != null)
                    setup.Invoke(obj, null);
                return true;
            }
            catch (TargetInvocationException ex)
            {
                StdErr.Error($"{fixture.Name}: Failed to setup {test.Name}: {ex.InnerException}");
                return false;
            }
            catch (Exception ex)
            {
                StdErr.Error($"{fixture.Name}: Failed to setup {test.Name}: {ex}");
                return false;
            }
        }

        private bool TearDown(MethodInfo test)
        {
            try
            {
                if (tearDown != null)
                    tearDown.Invoke(obj, null);
                return true;
            }
            catch (TargetInvocationException ex)
            {
                StdErr.Error($"{fixture.Name}: Failed to tear down {test.Name}: {ex.InnerException}");
                return false;
            }
            catch (Exception ex)
            {
                StdErr.Error($"{fixture.Name}: Failed to tear down {test.Name}: {ex}");
                return false;
            }
        }

    }
}
