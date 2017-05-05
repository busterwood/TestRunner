using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

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
        int testFinished;

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
                if (test.IsIgnored())
                    continue;
                if (test.IsTest())
                    count++;
                else
                    count += test.CustomAttributes.Where(a => a.IsTestCase()).Count();
            }
            return count;
        }

        public void RunTests()
        {
            obj = Activator.CreateInstance(fixture);
            FixtureSetUp();
            foreach (var test in methods)
            {
                watch.Reset();
                watch.Start();
                if (test.IsIgnored())
                {
                    StdOut.Ignore($"{fixture.Name}.{test.Name}");
                    ignored++;
                    testCount++;
                }
                else if (test.IsTest())
                {
                    testCount++;
                    RunTestLifeCycle(test, null, test.Name);
                    continue;
                }
                else foreach (var testCase in test.CustomAttributes.Where(a => a.IsTestCase()))
                {
                    testCount++;
                    var args = TestCaseArgs(testCase);
                    var testName = TestCaseName(test, args);
                    RunTestLifeCycle(test, args, testName);
                }
            }
            FixtureTearDown();
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
                StdErr.Error($"{fixture.Name}: Failed to setup fixture: {ex.InnerException}");
                return false;
            }
            catch (Exception ex)
            {
                StdErr.Error($"{fixture.Name}: Failed to setup fixture: {ex}");
                return false;
            }
        }

        private bool FixtureTearDown()
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
            testFinished = 0;
            Thread testThread = new Thread(() => RunTest(test, args, testName));
            testThread.Start();

            if (testThread.Join((int)timeoutMs))
                return; // all good

            testThread.Interrupt();
            if (!testThread.Join(50))
            {
                // last chance - aborting may leave unrealased locks 
                testThread.Abort();
                testThread.Join();
            }
            Fail(testName, $"Timed-out after {(int)timeoutMs:N0} MS");
        }

        private void RunTest(MethodInfo test, object[] args, string testName)
        {
            Interlocked.Exchange(ref testFinished, 0);
            try
            {
                var result = test.Invoke(obj, args) as Task; // support for asyn test methods
                if (result != null)
                    WaitForAsyncTestToComplete(testName, result);
                else
                    Pass(testName);
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

        private void WaitForAsyncTestToComplete(string testName, Task result)
        {
            try
            {
                result.Wait(); 
                Pass(testName);
            }
            catch (AggregateException ex) when (ex.InnerException is TargetInvocationException)
            {
                Fail(testName, ex.InnerException.InnerException);
            }
            catch (AggregateException ex)
            {
                Fail(testName, ex.InnerException);
            }
        }

        private void Pass(string testName)
        {
            if (EndOfTestAlreadyReported()) return;
            StdOut.Passed($"{fixture.Name}.{testName} in {watch.ElapsedMilliseconds:D0} MS");
            passed++;
        }

        private void Fail(string testName, Exception ex)
        {
            if (EndOfTestAlreadyReported()) return;
            Console.WriteLine(ex);
            StdOut.Fail($"{fixture.Name}.{testName} in {watch.ElapsedMilliseconds:D0} MS");
            failed++;
        }

        private void Fail(string testName, string message)
        {
            if (EndOfTestAlreadyReported()) return;
            Console.WriteLine(message);
            StdOut.Fail($"{fixture.Name}.{testName} in {watch.ElapsedMilliseconds:D0} MS");
            failed++;
        }

        private bool EndOfTestAlreadyReported() => Interlocked.CompareExchange(ref testFinished, 1, 0) == 1;

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
