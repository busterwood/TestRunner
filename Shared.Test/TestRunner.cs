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
        readonly Fixture fixture;
        readonly Test test;
        object obj;
        readonly Stopwatch watch;
        int testFinished;

        public TestRunner(Test test, Fixture fixture, object obj, Stopwatch watch)
        {
            this.fixture = fixture;
            this.test = test;
            this.obj = obj;
            this.watch = watch;
        }

        public string Name => test.Name;

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
            SetNunitContext();
            watch.Reset();
            watch.Start();
            if (!SetUp())
                return;
            object timeout = test.Timeout ?? fixture.Timeout;
            if (timeout != null && !Debugger.IsAttached) // ignore timeout when debugger is attached
                RunTestWithTimeout(timeout);
            else
                RunTestMethod();
            TearDown();
        }

        private void LogStartOfTest()
        {
            var cat = test.Category;
            StdOut.Start($"{fixture.Name}.{test.Name}{(cat != null ? ": " : "")}{cat}");
        }

        private void SetNunitContext()
        {
            IDictionary context = new Hashtable
            {
                { "Test.Name", test.Name },
                { "Test.FullName", fixture.Name + "." + test.Name },
                { "WorkDirectory", Environment.CurrentDirectory },
                { "TestDirectory", Environment.CurrentDirectory },
            };
            CallContext.LogicalSetData("NUnit.Framework.TestContext", context);
        }

        private void RunTestWithTimeout(object timeoutMs)
        {
            Thread testThread = new Thread(RunTestMethod) { IsBackground = true, Name = "Test" };
            testThread.Start();
            if (testThread.Join((int)timeoutMs))
                return; // all good
            testThread.Interrupt();
            if (!testThread.Join(50))
            {
                // last chance - aborting may leave locks 
                testThread.Abort();
                testThread.Join();
            }
            Fail($"Timed-out after {(int)timeoutMs} MS");
        }

        private void RunTestMethod()
        {
            try
            {
                var result = test.Invoke(obj) as Task; // support async test methods
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
            StdOut.Passed($"{fixture.Name}.{test.Name} in {watch.ElapsedMilliseconds} MS");
            Passed = true;
        }

        private void Fail(Exception ex)
        {
            if (EndOfTestAlreadyReported())
                return;
            Console.WriteLine(ex); // write out the message before the fail line so the Gui can parse the result
            StdOut.Fail($"{fixture.Name}.{test.Name} in {watch.ElapsedMilliseconds} MS");
            Failed = true;
        }

        private void Fail(string message)
        {
            if (EndOfTestAlreadyReported())
                return;
            Console.WriteLine(message); // write out the message before the fail line so the Gui can parse the result
            StdOut.Fail($"{fixture.Name}.{test.Name} in {watch.ElapsedMilliseconds} MS");
            Failed = true;
        }

        private bool EndOfTestAlreadyReported() => Interlocked.CompareExchange(ref testFinished, 1, 0) == 1;

        private bool SetUp()
        {
            try
            {
                fixture.Setup?.Invoke(obj, null);
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
                fixture.TearDown?.Invoke(obj, null);
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