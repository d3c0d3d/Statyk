using System;

namespace Statyk
{
    [AttributeUsage(AttributeTargets.Method)]
    public class DELETE : Attribute
    {
        public string Router { get; private set; }

        public DELETE(string router)
        {
            Router = router ?? throw new ArgumentNullException(nameof(router));
        }
    }
}
