using System;

namespace DHaven.Faux
{
    /// <summary>
    /// Marks a service with it's name.  Use this with a discovery
    /// service.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface)]
    public class ServiceAttribute : Attribute
    {
        public ServiceAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }
    }
}
