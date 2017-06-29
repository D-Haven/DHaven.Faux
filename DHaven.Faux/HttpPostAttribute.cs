using System;
using System.Collections.Generic;
using System.Text;

namespace DHaven.Faux
{
    [AttributeUsage(AttributeTargets.Method)]
    public class HttpPostAttribute : Attribute
    {
        public HttpPostAttribute(string path = "")
        {
            Path = path ?? string.Empty;
        }

        public string Path { get; set; }
    }
}
