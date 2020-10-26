using System;

namespace Statyk
{
    [AttributeUsage(AttributeTargets.Method)]
    public class PUT : Attribute
    {
        public string Router { get; private set; }

        public PUT(string router)
        {
            Router = router ?? throw new ArgumentNullException(nameof(router));
        }
    }
}
