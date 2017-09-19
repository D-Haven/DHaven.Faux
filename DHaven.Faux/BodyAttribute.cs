using System;
using System.Collections.Generic;
using System.Text;

namespace DHaven.Faux
{
    /// <summary>
    /// The value of this parameter will be used for the body of
    /// the message.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class BodyAttribute : Attribute
    {
    }
}
