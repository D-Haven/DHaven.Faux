using System;

namespace DHaven.Faux
{
    /// <summary>
    /// Thrown when the service interface can't be understood.
    /// </summary>
    [Serializable]
    internal class WebServiceCompileException : ApplicationException
    {
        public WebServiceCompileException()
        {
        }

        public WebServiceCompileException(string message) : base(message)
        {
        }

        public WebServiceCompileException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}