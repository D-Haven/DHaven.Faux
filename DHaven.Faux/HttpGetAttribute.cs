using System;
using System.Net.Http;

namespace DHaven.Faux
{
    /// <summary>
    /// Make a GET service call.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class HttpGetAttribute : HttpMethodAttribute
    {
        public HttpGetAttribute(string path = null) : base(HttpMethod.Get, path) { }
    }
}
