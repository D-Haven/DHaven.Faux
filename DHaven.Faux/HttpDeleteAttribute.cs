using System;

namespace DHaven.Faux
{
    /// <summary>
    /// Make a DELETE service call.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class HttpDeleteAttribute : HttpMethodAttribute
    {
        public HttpDeleteAttribute(string path = null) : base("DELETE", path) { }
    }
}
