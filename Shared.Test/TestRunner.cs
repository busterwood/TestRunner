using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Threading.Tasks;

namespace Tests
{    
    public class TestRunner
    {
        readonly string fixtureName;
        readonly string testName;
        object obj;
        readonly MethodInfo setup;
        readonly MethodInfo tearDown;
        readonly MethodInfo test;
        readonly Stopwatch watch;
        readonly object[] args;
        readonly string category;
        int testFinished;
        readonly ILookup<string, CustomAttributeData> attrsByName;

        public TestRunner(string fixtureName, string testName, object obj, MethodInfo setup, MethodInfo tearDown, MethodInfo test, Stopwatch watch, object[] args, ILookup<string, CustomAttributeData> attrsByName)
        {
            this.fixtureName = fixtureName;
            this.testName = testName;
            this.obj = obj;
            this.setup = setup;
            this.tearDown = tearDown;
            this.test = test;
            this.watch = watch;
            this.args = args;
            this.attrsByName = attrsByName;
            this.category = this.attrsByName.Category() ?? test.DeclaringType.Category();
        }

        public string Name => testName;

        public bool Ignored => attrsByName.Contains("IgnoredAttribute");

        public bool Explicit => attrsByName.Contains("ExplicitAttribute");

        public bool Passed { get; private set; }

        public bool Failed { get; private set; }

        public object Obj
        {
            get { return obj; }
            set { obj = value; }
        }

        public void FixtureSetupFailed()
        {
            LogStartOfTest();
            Fail("FixtureSetUp failed, test not run");
        }

        public void Run()
        {
            LogStartOfTest();
            ChangeTypeOfArguments();
            SetNunitContext();
            watch.Reset();
            watch.Start();
            if (!SetUp())
                return;
            object timeout = TestTimeout() ?? FixtureTimeout();
            if (timeout != null && !Debugger.IsAttached) // ignore timeout when debugger is attached
                RunTestWithTimeout(args, timeout);
            else
                RunTestMethod();
            TearDown();
        }

        private void LogStartOfTest()
        {
            StdOut.Start($"{fixtureName}.{testName}{(category != null ? ": " : "")}{category}");
        }

        private void SetNunitContext()
        {
            IDictionary context = new Hashtable
            {
                { "Test.Name", testName },
                { "Test.FullName", fixtureName + "." + testName },
                { "WorkDirectory", Environment.CurrentDirectory },
                { "TestDirectory", Environment.CurrentDirectory },
            };
            CallContext.LogicalSetData("NUnit.Framework.TestContext", context);
        }

        private object TestTimeout()
        {
            return attrsByName["TimeoutAttribute"].FirstOrDefault()?.ConstructorArguments?.First().Value;
        }

        private object FixtureTimeout()
        {
            return test.DeclaringType.CustomAttributes.FirstOrDefault(a => a.IsTimeout())?.ConstructorArguments?.First().Value;
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
            catch (Exception ex) when (ex.IsSuccessException())
            {
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
            catch (AggregateException ex) when (ex.InnerException.IsSuccessException())
            {
                Pass();
            }
            catch (AggregateException ex) when (ex.InnerException is TargetInvocationException)
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

}