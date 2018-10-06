using System.Reflection;

namespace DHaven.Faux.Compiler
{
    public interface IWebServiceClassGenerator
    {
        /// <summary>
        /// Gets or sets the location where source files are written to disk.
        /// </summary>
        string SourceFilePath { get; set; }

        /// <summary>
        /// Gets or sets the flag to write source files to disk.
        /// </summary>
        bool OutputSourceFiles { get; set; }

        /// <summary>
        /// Gets or sets the root namespace to use for generated classes.
        /// </summary>
        string RootNamespace { get; set; }

        /// <summary>
        /// Gets or sets the flag to ensure the generated classes are sealed (cannot be inherited).
        /// Default is true.
        /// </summary>
        bool GenerateSealedClasses { get; set; }

        /// <summary>
        /// Generate the source code for the interface represented by the type.
        /// </summary>
        /// <param name="typeInfo">the interface to generate an implementation for.</param>
        /// <param name="fullClassName">the implentation fully qualified class name.</param>
        /// <returns>the source code for the class as a string.</returns>
        string GenerateSource(TypeInfo typeInfo, out string fullClassName);
    }
}
