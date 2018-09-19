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
            var compiler = new WebServiceCompiler();

            WebServiceClassGenerator.RootNamespace = opts.RootNameSapce ?? WebServiceClassGenerator.RootNamespace;
            WebServiceClassGenerator.OutputSourceFiles = opts.OutputSourceCode;
            WebServiceClassGenerator.SourceFilePath = opts.OutputSourcePath ?? WebServiceClassGenerator.SourceFilePath;
            WebServiceClassGenerator.GenerateSealedClasses = !opts.UseUnsealedClasses;

            foreach (var @interface in assembly.GetExportedTypes())
            {
                if (@interface.IsInterface && @interface.GetCustomAttribute<FauxClientAttribute>() != null)
                {
                    compiler.RegisterInterface(@interface.GetTypeInfo(), out _);
                }
            }

            compiler.Compile(outputAssembly);
        }
    }
}
