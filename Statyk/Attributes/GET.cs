using System;

namespace Statyk
{
    [AttributeUsage(AttributeTargets.Method)]
    public class GET : Attribute
    {
        public string Router { get; private set; }

        public GET(string router)
        {
            Router = router ?? throw new ArgumentNullException(nameof(router));
        }
    }
}