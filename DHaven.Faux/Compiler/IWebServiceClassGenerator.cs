using System.Reflection;

namespace DHaven.Faux.Compiler
{
    public interface IWebServiceClassGenerator
    {
        /// <summary>
        /// Gets the compiler configuration
        /// </summary>
        CompilerConfig Config { get; }

        /// <summary>
        /// Generate the source code for the interface represented by the type.
        /// </summary>
        /// <param name="typeInfo">the interface to generate an implementation for.</param>
        /// <param name="fullClassName">the implentation fully qualified class name.</param>
        /// <returns>the source code for the class as a string.</returns>
        string GenerateSource(TypeInfo typeInfo, out string fullClassName);
    }
}
