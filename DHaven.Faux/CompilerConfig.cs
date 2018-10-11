using Microsoft.Extensions.Configuration;

namespace DHaven.Faux
{
    public class CompilerConfig
    {
        private const string DefaultNamespace = "DHaven.Faux.Wrapper";

        public CompilerConfig()
        {
            OutputSourceFiles = false;
            RootNamespace = DefaultNamespace;
            GenerateSealedClasses = true;
        }

        public CompilerConfig(IConfiguration config)
        {
            var section = config.GetSection("Faux");
            SourceFilePath = section.GetValue<string>(nameof(SourceFilePath));
            OutputSourceFiles = section.GetValue<bool>(nameof(OutputSourceFiles));
            RootNamespace = section.GetValue(nameof(RootNamespace), DefaultNamespace);
            GenerateSealedClasses = section.GetValue(nameof(GenerateSealedClasses), true);
        }

        /// <summary>
        /// Gets or sets the location where source files are written to disk.
        /// </summary>
        public string SourceFilePath { get; set; }

        /// <summary>
        /// Gets or sets the flag to write source files to disk.
        /// </summary>
        public bool OutputSourceFiles { get; set; }

        /// <summary>
        /// Gets or sets the root namespace to use for generated classes.
        /// Default is "DHaven.Feign.Wrapper"
        /// </summary>
        public string RootNamespace { get; set; }

        /// <summary>
        /// Gets or sets the flag to ensure the generated classes are sealed (cannot be inherited).
        /// Default is true.
        /// </summary>
        public bool GenerateSealedClasses { get; set; }
    }
}
