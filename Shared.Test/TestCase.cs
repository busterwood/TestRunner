using System;
using System.Linq;
using System.Reflection;
namespace Tests
{
    public class TestCase : Test
    {
        readonly CustomAttributeData testCase;
        object[] args;
        string name;

        public TestCase(MethodInfo test, ILookup<string, CustomAttributeData> attrsByName, CustomAttributeData testCase): base (test, attrsByName)
        {
            this.testCase = testCase;
        }

        public override object[] Args => args = args ?? TestCaseArgs();

        object[] TestCaseArgs()
        {
            var args = testCase.ConstructorArguments.Select(arg => arg.Value).ToArray();
            int i = 0;
            foreach (var p in test.GetParameters())
            {
                var arg = args[i];
                if (arg != null && p.ParameterType != arg.GetType() && !p.ParameterType.IsEnum)
                    args[i] = Convert.ChangeType(arg, p.ParameterType);
                i++;
            }
            return args;
        }

        public override string Name => name = name ?? TestCaseName();

        string TestCaseName() => test.Name + "(" + string.Join(",", Args.Select(a => a is string ? '"' + ReplaceSpecialChars(a.ToString()) + '"' : a)) + ")";

        static string ReplaceSpecialChars(string s) => s.Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");
    }
}