using System;

namespace DHaven.Faux
{
    /// <summary>
    /// The value of this parameter will be used for the body of
    /// the message.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public class BodyAttribute : Attribute
    {
        public Format Format { get; set; }
    }

    public enum Format
    {
        Json,
        Raw
    }
}
