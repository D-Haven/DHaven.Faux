using System;
using System.Net.Http;

namespace DHaven.Faux
{
    /// <summary>
    /// Use this to mark service calls with non-standard HTTP methods.
    /// This is also the base class for all the standard HTTP methods.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class HttpMethodAttribute : Attribute
    {
        public HttpMethodAttribute(HttpMethod method, string path)
        {
            Method = method;
            Path = path ?? string.Empty;
        }

        public string Path { get; private set; }
        public HttpMethod Method { get; private set; }
    }
}
