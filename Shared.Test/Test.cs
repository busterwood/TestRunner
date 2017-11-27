using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
namespace Tests
{
    public class Test
    {
        protected readonly MethodInfo test;
        protected readonly ILookup<string, CustomAttributeData> attrsByName;

        public Test(MethodInfo test, ILookup<string, CustomAttributeData> attrsByName)
        {
            this.test = test;
            this.attrsByName = attrsByName;
        }

        public virtual string Name => test.Name;
        public virtual object[] Args => null;
        public string Category => attrsByName["CategoryAttribute"].FirstOrDefault()?.ConstructorArguments?.FirstOrDefault().Value?.ToString();
        public bool Ignored => attrsByName.Contains("IgnoredAttribute");
        public bool Explicit => attrsByName.Contains("ExplicitAttribute");
        public object Timeout => attrsByName["TimeoutAttribute"].FirstOrDefault()?.ConstructorArguments?.First().Value;
        public object Invoke(object obj) => test.Invoke(obj, Args);
        public static IEnumerable<Test> Tests(MethodInfo method)
        {
            var attrsByName = method.CustomAttributes.ToLookup(a => a.AttributeType.Name);
            if (attrsByName.IsTest())
            {
                yield return new Test(method, attrsByName);
            }
            else
            {
                foreach (var testCase in attrsByName["TestCaseAttribute"])
                {
                    yield return new TestCase(method, attrsByName, testCase);
                }
            }
        }
    }
}