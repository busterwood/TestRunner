using System;
using System.Linq;
using System.Reflection;

namespace Test
{
    static class TypeExtensions
    {
        public static bool IsTestFixture(this Type type)
        {
            return type.GetCustomAttributes().Any(a => string.Equals(a.GetType().Name, "TestFixtureAttribute", StringComparison.Ordinal));
        }

        public static bool IsIgnored(this Type type)
        {
            return type.GetCustomAttributes().Any(a => string.Equals(a.GetType().Name, "IgnoreAttribute", StringComparison.Ordinal));
        }

        public static bool IsTestFixtureSetUp(this MethodInfo method)
        {
            return method.GetCustomAttributes().Any(a => string.Equals(a.GetType().Name, "TestFixtureSetUpAttribute", StringComparison.Ordinal));
        }

        public static bool IsSetup(this MethodInfo method)
        {
            return method.GetCustomAttributes().Any(a => string.Equals(a.GetType().Name, "SetUpAttribute", StringComparison.Ordinal));
        }

        public static bool IsTearDown(this MethodInfo method)
        {
            return method.GetCustomAttributes().Any(a => string.Equals(a.GetType().Name, "TearDownAttribute", StringComparison.Ordinal));
        }

        public static bool IsTestFixtureTearDown(this MethodInfo method)
        {
            return method.GetCustomAttributes().Any(a => string.Equals(a.GetType().Name, "TestFixtureTearDownAttribute", StringComparison.Ordinal));
        }

        public static bool IsTest(this MethodInfo method)
        {
            return method.GetCustomAttributes().Any(a => string.Equals(a.GetType().Name, "TestAttribute", StringComparison.Ordinal));
        }

        public static bool IsTestCase(this MethodInfo method)
        {
            return method.GetCustomAttributes().Any(a => string.Equals(a.GetType().Name, "TestCaseAttribute", StringComparison.Ordinal));
        }

        public static bool IsTimeout(this CustomAttributeData attr)
        {
            return string.Equals(attr.AttributeType.Name, "TimeoutAttribute", StringComparison.Ordinal);
        }

        public static bool IsIgnored(this MethodInfo method)
        {
            return method.GetCustomAttributes().Any(a => string.Equals(a.GetType().Name, "IgnoreAttribute", StringComparison.Ordinal));
        }

        public static bool IsTestCase(this CustomAttributeData attr)
        {
            return string.Equals(attr.AttributeType.Name, "TestCaseAttribute", StringComparison.Ordinal);
        }
    }
}
