using System;

namespace DHaven.Faux
{
    /// <summary>
    /// Make a GET service call.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class HttpGetAttribute : HttpMethodAttribute
    {
        public HttpGetAttribute(string path = null) : base("GET", path) { }
    }
}
