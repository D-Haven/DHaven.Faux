﻿using System;

namespace DHaven.Faux
{
    /// <inheritdoc />
    /// <summary>
    /// The out parameter or return value marked with this attribute will be assigned
    /// the value from the response.
    /// </summary>
    [AttributeUsage(AttributeTargets.ReturnValue | AttributeTargets.Parameter)]
    public class ResponseHeaderAttribute : Attribute
    {
        public ResponseHeaderAttribute(string headerName)
        {
            Header = headerName;
        }

        public string Header { get; }
    }
}
