using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Threading.Tasks;

namespace Test
{
    public class TestRunner
    {
        readonly string fixtureName;
        readonly string testName;
        readonly object obj;
        readonly MethodInfo setup;
        readonly MethodInfo tearDown;
        readonly MethodInfo test;
        readonly object fixtureTimeoutMs;
        readonly Stopwatch watch;
        readonly object[] args;
        int testFinished;

        public TestRunner(string fixtureName, string testName, object obj, MethodInfo setup, MethodInfo tearDown, MethodInfo test, object fixtureTimeoutMs, Stopwatch watch, object[] args)
        {
            this.fixtureName = fixtureName;
            this.testName = testName;
            this.obj = obj;
            this.setup = setup;
            this.tearDown = tearDown;
            this.test = test;
            this.fixtureTimeoutMs = fixtureTimeoutMs;
            this.watch = watch;
            this.args = args;
        }

        public bool Ignored => test.IsIgnored();

        public bool Passed { get; private set; }

        public bool Failed { get; private set; }

        public void Run()
        {
            StdOut.Start($"{fixtureName}.{testName} ");
            ChangeTypeOfArguments();
            SetNunitContext();
            watch.Reset();
            watch.Start();
            if (!SetUp())
                return;
            object timeout = TestTimeout() ?? fixtureTimeoutMs;
            if (timeout != null)
                RunTestWithTimeout(args, timeout);
            else
                RunTestMethod();
            TearDown();
        }

        private void SetNunitContext()
        {
            var context = new Hashtable
            {
                { "Test.Name", testName },
                { "Test.FullName", fixtureName + "." + testName },
                { "WorkDirectory", Environment.CurrentDirectory }
            };
            CallContext.SetData("NUnit.Framework.TestContext", context);
        }

        private object TestTimeout()
        {
            return test.CustomAttributes.FirstOrDefault(a => a.IsTimeout())?.ConstructorArguments?.First().Value;
        }

        private void RunTestWithTimeout(object[] args, object timeoutMs)
        {
            Thread testThread = new Thread(RunTestMethod)
            {IsBackground = true};
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
            Fail($"Timed-out after {(int)timeoutMs} MS");
        }

        private void RunTestMethod()
        {
            try
            {
                var result = test.Invoke(obj, args) as Task; // support async test methods
                if (result != null)
                    WaitForAsyncTestToComplete(result);
                else
                    Pass();
            }
            catch (TargetInvocationException ex)
            {
                Fail(ex.InnerException);
            }
            catch (Exception ex)
            {
                Fail(ex);
            }
        }

        private void ChangeTypeOfArguments()
        {
            if (args != null)
            {
                int i = 0;
                foreach (var p in test.GetParameters())
                {
                    var arg = args[i];
                    if (arg != null && p.ParameterType != arg.GetType() && !p.ParameterType.IsEnum)
                        args[i] = Convert.ChangeType(arg, p.ParameterType);
                    i++;
                }
            }
        }

        private void WaitForAsyncTestToComplete(Task result)
        {
            try
            {
                result.Wait();
                Pass();
            }
            catch (AggregateException ex)when (ex.InnerException is TargetInvocationException)
            {
                Fail(ex.InnerException.InnerException);
            }
            catch (AggregateException ex)
            {
                Fail(ex.InnerException);
            }
        }

        private void Pass()
        {
            if (EndOfTestAlreadyReported())
                return;
            StdOut.Passed($"{fixtureName}.{testName} in {watch.ElapsedMilliseconds} MS");
            Passed = true;
        }

        private void Fail(Exception ex)
        {
            if (EndOfTestAlreadyReported())
                return;
            Console.WriteLine(ex); // write out the message before the fail line so the Gui can parse the result
            StdOut.Fail($"{fixtureName}.{testName} in {watch.ElapsedMilliseconds} MS");
            Failed = true;
        }

        private void Fail(string message)
        {
            if (EndOfTestAlreadyReported())
                return;
            Console.WriteLine(message); // write out the message before the fail line so the Gui can parse the result
            StdOut.Fail($"{fixtureName}.{testName} in {watch.ElapsedMilliseconds} MS");
            Failed = true;
        }

        private bool EndOfTestAlreadyReported() => Interlocked.CompareExchange(ref testFinished, 1, 0) == 1;

        private bool SetUp()
        {
            try
            {
                setup?.Invoke(obj, null);
                return true;
            }
            catch (TargetInvocationException ex)
            {
                Fail($"SetUp failed: {ex.InnerException}");
                return false;
            }
            catch (Exception ex)
            {
                Fail($"SetUp failed: {ex}");
                return false;
            }
        }

        private bool TearDown()
        {
            try
            {
                tearDown?.Invoke(obj, null);
                return true;
            }
            catch (TargetInvocationException ex)
            {
                Fail($"TearDown failed: {ex.InnerException}");
                return false;
            }
            catch (Exception ex)
            {
                Fail($"TearDown failed: {ex}");
                return false;
            }
        }
    }

    public class SetUpFixtureRunner
    {
        readonly Type setupFixture;
        readonly object obj;
        readonly MethodInfo setup;
        readonly MethodInfo tearDown;

        public SetUpFixtureRunner(Type setupFixture)
        {
            this.setupFixture = setupFixture;
            var methods = setupFixture.GetMethods();
            this.setup = methods.FirstOrDefault(m => m.IsSetup());
            this.tearDown = methods.FirstOrDefault(m => m.IsTearDown());
            this.obj = Activator.CreateInstance(setupFixture);
        }

        public bool Failed { get; private set; }

        private void Fail(string message)
        {
            Console.WriteLine(message); // write out the message before the fail line so the Gui can parse the result
            StdOut.Fail($"{setupFixture.FullName}");
            Failed = true;
        }

        public bool SetUp()
        {
            try
            {
                setup?.Invoke(obj, null);
                return true;
            }
            catch (TargetInvocationException ex)
            {
                Fail($"SetUp failed: {ex.InnerException}");
                return false;
            }
            catch (Exception ex)
            {
                Fail($"SetUp failed: {ex}");
                return false;
            }
        }

        public bool TearDown()
        {
            try
            {
                tearDown?.Invoke(obj, null);
                return true;
            }
            catch (TargetInvocationException ex)
            {
                Fail($"TearDown failed: {ex.InnerException}");
                return false;
            }
            catch (Exception ex)
            {
                Fail($"TearDown failed: {ex}");
                return false;
            }
        }
    }

}