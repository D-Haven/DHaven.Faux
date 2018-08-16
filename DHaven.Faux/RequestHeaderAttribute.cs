using System;

namespace DHaven.Faux
{
    /// <inheritdoc />
    /// <summary>
    /// The value of this method parameter marked with this attribute will
    /// be supplied to the header identified.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class RequestHeaderAttribute : Attribute
    {
        public RequestHeaderAttribute(string headerName)
        {
            Header = headerName;
        }

        public string Header { get; }
    }
}
