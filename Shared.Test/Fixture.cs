using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Tests
{
    public class Fixture
    {
        readonly Type type;
        readonly ILookup<string, CustomAttributeData> attrsByName;
        Lazy<string> category;
        Lazy<object> timeout;
        Lazy<MethodInfo> fixtureSetup;
        Lazy<MethodInfo> fixtureTearDown;
        Lazy<MethodInfo> setup;
        Lazy<MethodInfo> tearDown;

        public Fixture(Type type)
        {
            this.type = type;
            attrsByName = type.CustomAttributes.ToLookup(a => a.AttributeType.Name);
            category = new Lazy<string>(() => attrsByName["CategoryAttribute"].FirstOrDefault()?.ConstructorArguments?.FirstOrDefault().Value?.ToString());
            timeout = new Lazy<object>(() => attrsByName["TimeoutAttribute"].FirstOrDefault()?.ConstructorArguments?.First().Value);
            var methods = type.GetMethods()
                .SelectMany(mi => mi.CustomAttributes, (Method, Attribute) => new { Method, Attribute })
                .ToLookup(m => m.Attribute.AttributeType.Name);
            fixtureSetup = new Lazy<MethodInfo>(() => methods["TestFixtureSetUpAttribute"].FirstOrDefault()?.Method);
            fixtureTearDown = new Lazy<MethodInfo>(() => methods["TestFixtureTearDownAttribute"].FirstOrDefault()?.Method);
            setup = new Lazy<MethodInfo>(() => methods["SetUpAttribute"].FirstOrDefault()?.Method);
            tearDown = new Lazy<MethodInfo>(() => methods["TearDownAttribute"].FirstOrDefault()?.Method);
        }

        public string Name => type.Name;

        public string Category => category.Value;
        public object Timeout => timeout.Value;
        public MethodInfo FixtureSetup => fixtureSetup.Value;
        public MethodInfo FixtureTearDown => fixtureTearDown.Value;
        public MethodInfo Setup => setup.Value;
        public MethodInfo TearDown => tearDown.Value;    
        public Type Type => type;

        public IEnumerable<Test> Tests() => type.GetMethods().SelectMany(Test.Tests);
    }

}