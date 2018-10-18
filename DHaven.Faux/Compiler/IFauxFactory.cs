using DHaven.Faux.HttpSupport;
using System.Reflection;

namespace DHaven.Faux.Compiler
{
    /// <summary>
    /// Internally used factory to create instances of Faux services.
    /// </summary>
    public interface IFauxFactory
    {
        /// <summary>
        /// Create the requested service.  Uses IHttpClient provided
        /// by the system.
        /// </summary>
        /// <param name="type">the type to create.</param>
        /// <returns>an instance of your service</returns>
        object Create(TypeInfo type);
        
        /// <summary>
        /// Used for testing purposes.
        /// </summary>
        /// <param name="type">the TypeInfo for the service interface</param>
        /// <param name="overrideHttpClient">the IHttpClient instance to inject, used for testing</param>
        /// <returns>an instance of your service</returns>
        object Create(TypeInfo type, IHttpClient overrideHttpClient);
    }
}
