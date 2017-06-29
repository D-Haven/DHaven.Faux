using System;
using System.Collections.Generic;
using System.Text;

namespace DHaven.Faux
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class RouteAttribute : Attribute
    {
        public RouteAttribute(string baseRoute)
        {
            BaseRoute = baseRoute;
        }

        public string BaseRoute { get; set; }

        public string ServiceName { get; set; }
    }
}
