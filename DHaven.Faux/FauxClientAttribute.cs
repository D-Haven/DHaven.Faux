using System;

namespace DHaven.Faux
{
    /// <inheritdoc />
    /// <summary>
    /// Marks a service with it's name.  Use this with a discovery
    /// service.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface)]
    public class FauxClientAttribute : Attribute
    {
        public FauxClientAttribute(string name, string route = "")
        {
            Name = name;
            Route = route;
        }

        public string Name { get; }
        
        public string Route { get; }
    }
}
