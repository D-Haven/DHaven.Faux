using System;
using System.Net.Http;

namespace DHaven.Faux
{
    /// <summary>
    /// Make a PUT service call.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class HttpPutAttribute : HttpMethodAttribute
    {
        public HttpPutAttribute(string path = null) : base(HttpMethod.Put, path) { }
    }
}
