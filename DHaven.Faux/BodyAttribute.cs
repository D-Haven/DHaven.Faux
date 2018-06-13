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

    /// <summary>
    /// Format that the body will be used to serialize the body.
    /// </summary>
    public enum Format
    {
        Auto,
        Json,
        Raw
    }
}
