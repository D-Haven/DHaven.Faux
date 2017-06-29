using System;
using System.Collections.Generic;
using System.Text;

namespace DHaven.Faux
{
    [AttributeUsage(AttributeTargets.Method)]
    public class HttpGetAttribute : Attribute
    {
        public HttpGetAttribute(string path = "")
        {
            Path = path ?? string.Empty;
        }

        public string Path { get; set; }
    }
}
