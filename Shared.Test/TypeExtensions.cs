using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Tests
{
    static class TypeExtensions
    {
        public static bool IsSetUpFixture(this Type type)
        {
            return type.GetCustomAttributes().Any(a => string.Equals(a.GetType().Name, "SetUpFixtureAttribute", StringComparison.Ordinal));
        }

        public static bool IsTestFixture(this Type type)
        {
            return type.GetCustomAttributes().Any(a => string.Equals(a.GetType().Name, "TestFixtureAttribute", StringComparison.Ordinal));
        }

        public static bool IsIgnored(this Type type)
        {
            return HasIgnoredAttribute(type) || HasTestFixtureIgnoredProperty(type);
        }

        private static bool HasIgnoredAttribute(Type type)
        {
            return type.GetCustomAttributes().Any(a => string.Equals(a.GetType().Name, "IgnoreAttribute", StringComparison.Ordinal));
        }

        public static bool HasTestFixtureIgnoredProperty(this Type type)
        {
            var fixture = type.GetCustomAttributesData()
                .FirstOrDefault(a => string.Equals(a.AttributeType.Name, "TestFixtureAttribute", StringComparison.Ordinal));
            var ignored = fixture?.NamedArguments.FirstOrDefault(na => string.Equals(na.MemberName, "Ignore")).TypedValue.Value;
            return ignored?.Equals(true) ?? false;
        }

        /// <summary>Only run when check on the UI or included via a category</summary>
        public static bool IsExplicit(this Type type)
        {
            return type.GetCustomAttributes().Any(a => string.Equals(a.GetType().Name, "ExplicitAttribute", StringComparison.Ordinal));
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

        //public static bool IsTest(this MethodInfo method)
        //{
        //    return method.GetCustomAttributes().Any(a => string.Equals(a.GetType().Name, "TestAttribute", StringComparison.Ordinal));
        //}

        public static bool IsTest(this ILookup<string, CustomAttributeData> attrs) => attrs.Contains("TestAttribute");

        //public static bool IsTestCase(this MethodInfo method)
        //{
        //    return method.GetCustomAttributes().Any(a => string.Equals(a.GetType().Name, "TestCaseAttribute", StringComparison.Ordinal));
        //}

        public static bool IsTimeout(this CustomAttributeData attr)
        {
            return string.Equals(attr.AttributeType.Name, "TimeoutAttribute", StringComparison.Ordinal);
        }

        public static bool IsIgnored(this MethodInfo method)
        {
            return method.GetCustomAttributes().Any(a => string.Equals(a.GetType().Name, "IgnoreAttribute", StringComparison.Ordinal));
        }

        /// <summary>Only run when check on the UI or included via a category</summary>
        public static bool IsExplicit(this MethodInfo method)
        {
            return method.GetCustomAttributes().Any(a => string.Equals(a.GetType().Name, "ExplicitAttribute", StringComparison.Ordinal));
        }

        public static bool IsTestCase(this CustomAttributeData attr)
        {
            return string.Equals(attr.AttributeType.Name, "TestCaseAttribute", StringComparison.Ordinal);
        }

        public static bool IsSuccessException(this Exception ex)
        {
            return string.Equals(ex?.GetType()?.Name, "SuccessException", StringComparison.Ordinal) || IsSuccessException(ex as TargetInvocationException);
        }

        public static bool IsSuccessException(this TargetInvocationException ex)
        {
            return string.Equals(ex?.InnerException?.GetType()?.Name, "SuccessException", StringComparison.Ordinal);
        }

        // category lookup is the slowest part of code when profiling 
        readonly static Dictionary<object, string> categoryCache = new Dictionary<object, string>(250);

        public static string Category(this Type type)
        {
            string category;
            if (categoryCache.TryGetValue(type, out category))
                return category;

            category = type.GetCustomAttributesData()
                .FirstOrDefault(a => string.Equals(a.AttributeType.Name, "CategoryAttribute", StringComparison.Ordinal))
                ?.ConstructorArguments
                ?.FirstOrDefault().Value?.ToString();
            categoryCache.Add(type, category);
            return category;
        }

        public static string Category(this ILookup<string, CustomAttributeData> method)
        {
            return method["CategoryAttribute"].FirstOrDefault()?.ConstructorArguments?.FirstOrDefault().Value?.ToString();
        }

    }
}
