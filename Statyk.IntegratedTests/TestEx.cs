using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using XR.Std;

namespace Statyk.IntegratedTests
{
    public abstract class TestEx
    {
        private readonly MethodInvoker<TestMethodAttribute>[] _instanceMethodsInvokers;

        public TestEx()
        {
            _instanceMethodsInvokers = GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(x => x.GetCustomAttribute(typeof(TestMethodAttribute)) != null)
              .Select(x =>
                  new MethodInvoker<TestMethodAttribute>(
                      (TestMethodAttribute)x.GetCustomAttribute(typeof(TestMethodAttribute)),
                      x)
              ).ToArray();
        }

        [TestMethod("list")]
        public virtual List<string> ListTestMethods()
        {
            return _instanceMethodsInvokers.Select(m => m.Attribute.TestMethodName).ToList();
        }

        public void Init(string name, string[] parms)
        {
            var instMethodByName = _instanceMethodsInvokers.FirstOrDefault(m => m.Attribute.TestMethodName == name);
            if (instMethodByName != null)
            {
                if (name != "list")
                    Cli.PrintLnC($"Starting TEST '{name}'...", ConsoleColor.White);
                instMethodByName.TargetMethod.Invoke(this, parms.Length > 0 ? new[] { parms } : null);
            }
        }
    }
}
