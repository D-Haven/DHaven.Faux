using System;
using System.Net.Http;

namespace DHaven.Faux
{
    /// <summary>
    /// Make a POST service call.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class HttpPostAttribute : HttpMethodAttribute
    {
        public HttpPostAttribute(string path = null) : base(HttpMethod.Post, path) { }
    }
}
