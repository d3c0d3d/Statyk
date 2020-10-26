using System;

namespace Statyk
{
    [AttributeUsage(AttributeTargets.Method)]
    public class POST : Attribute
    {
        public string Router { get; private set; }

        public POST(string router)
        {
            Router = router ?? throw new ArgumentNullException(nameof(router));
        }
    }
}