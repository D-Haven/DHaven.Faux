﻿using CommandLine;
using DHaven.Faux;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace DHaven.FauxGen
{
    /// <summary>
    /// Specify the options available for the command line tool.
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class CommandLineOptions
    {
        [Option('a', "assembly", Required = true, HelpText = "Assembly to scan for Faux interfaces")]
        public string InputAssemblyPath { get; set; }

        [Option('o', "output", Required = false, HelpText = "Output assembly path and name")]
        public string OutputAssemblyName { get; set; }
        
        [Option('n', "namespace", Required = false, HelpText = "Namespace to use for the generated classes.  Defaults to DHaven.Feign.Wrapper", Default = "DHaven.Feign.Wrapper")]
        public string RootNameSapce { get; set; }
        
        [Option('w', "write-source", Required = false, HelpText = "Output the generated source code to files")]
        public bool OutputSourceCode { get; set; }
        
        [Option('s', "source-path", Required = false, HelpText = "Override the directory to write the generated source to.  Defaults to './dhaven-faux'")]
        public string OutputSourcePath { get; set; }
        
        // For booleans, they default to false and their presence makes it true.  That's why this is for unsealed which
        // is opposite of the default behavior.
        [Option('u', "unsealed", HelpText = "Generate classes that are not sealed.  They are sealed by default.")]
        public bool UseUnsealedClasses { get; set; }

        internal void ApplyToConfig(CompilerConfig config)
        {
            config.RootNamespace = RootNameSapce ?? config.RootNamespace;
            config.OutputSourceFiles = OutputSourceCode;
            config.SourceFilePath = OutputSourcePath ?? config.SourceFilePath;
            config.GenerateSealedClasses = !UseUnsealedClasses;
        }
    }
}
