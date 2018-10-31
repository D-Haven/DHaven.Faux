using System;

namespace DHaven.Faux
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class HystrixFauxClientAttribute : FauxClientAttribute
    {
        public HystrixFauxClientAttribute(string name, string route = "", Type fallback = null) : base(name, route)
        {
            Fallback = fallback;
        }

        public Type Fallback { get; }
    }
}