using System;
using System.Collections.Generic;
using System.Text;

namespace DHaven.Faux
{
    /// <summary>
    /// Used to mark the method parameter as the value for the path
    /// variable in the HttpMethodAttribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class PathValueAttribute : Attribute
    {
        public PathValueAttribute(string param = null)
        {
            Variable = param;
        }

        public string Variable { get; private set; }
    }
}
