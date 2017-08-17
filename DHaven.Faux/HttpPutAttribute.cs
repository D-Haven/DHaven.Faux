using System;

namespace DHaven.Faux
{
    /// <summary>
    /// Make a PUT service call.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class HttpPutAttribute : HttpMethodAttribute
    {
        public HttpPutAttribute(string path = null) : base("PUT", path) { }
    }
}
