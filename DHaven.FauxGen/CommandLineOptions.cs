using System;
using System.Collections.Generic;
using System.Text;
using CommandLine;

namespace DHaven.FauxGen
{
    class CommandLineOptions
    {
        [Option('a', "assembly", Required = true, HelpText = "Assembly to scan for Faux interfaces")]
        public string InputAssemblyPath { get; set; }

        [Option('o', "output", Required = false, HelpText = "Output assembly path and name")]
        public string OutputAssemblyName { get; set; }
    }
}
