using System;
using System.Linq;
using System.Reflection;

namespace Tests
{
    public class SetUpFixtureRunner
    {
        readonly Type setupFixture;
        readonly object obj;
        readonly MethodInfo setup;
        readonly MethodInfo tearDown;

        public SetUpFixtureRunner(Type setupFixture)
        {
            if (setupFixture == null)
                throw new ArgumentNullException(nameof(setupFixture));
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
            StdErr.Info($"Running setup fixture: {setupFixture}");
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