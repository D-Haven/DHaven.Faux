using System;
using System.Runtime.Serialization;

namespace DHaven.Faux
{
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