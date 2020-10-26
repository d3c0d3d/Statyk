using System;

namespace Statyk.IntegratedTests
{
    [AttributeUsage(AttributeTargets.Method)]
    public class TestMethodAttribute : Attribute
    {
        public string TestMethodName { get; set; }
        public TestMethodAttribute(string methodName)
        {
            TestMethodName = methodName;
        }
    }
}
