using System;
using System.IO;
using System.Reflection;
using CommandLine;
using DHaven.Faux;
using DHaven.Faux.Compiler;

namespace DHaven.FauxGen
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            Parser.Default.ParseArguments<CommandLineOptions>(args)
                .WithParsed(GenerateAssembly);

            Console.In.ReadLine();
        }

        private static void GenerateAssembly(CommandLineOptions opts)
        {
            var outputAssembly = opts.OutputAssemblyName ?? $"Generated.{Path.GetFileName(opts.InputAssemblyPath)}";
            var assembly = Assembly.LoadFile(opts.InputAssemblyPath);
            var webserviceGenerator = FauxConfiguration.ClassGenerator;
            var compiler = new WebServiceCompiler(webserviceGenerator);

            webserviceGenerator.RootNamespace = opts.RootNameSapce ?? webserviceGenerator.RootNamespace;
            webserviceGenerator.OutputSourceFiles = opts.OutputSourceCode;
            webserviceGenerator.SourceFilePath = opts.OutputSourcePath ?? webserviceGenerator.SourceFilePath;
            webserviceGenerator.GenerateSealedClasses = !opts.UseUnsealedClasses;

            foreach (var @interface in assembly.GetExportedTypes())
            {
                if (@interface.IsInterface && @interface.GetCustomAttribute<FauxClientAttribute>() != null)
                {
                    compiler.RegisterInterface(@interface.GetTypeInfo());
                }
            }

            compiler.Compile(outputAssembly);
        }
    }
}
