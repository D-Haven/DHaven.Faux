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

            foreach (var iface in assembly.GetExportedTypes())
            {
                if (iface.IsInterface && iface.GetCustomAttribute<FauxClientAttribute>() != null)
                {
                    compiler.RegisterInterface(iface.GetTypeInfo());
                }
            }

            using (var stream = new FileStream(outputAssembly, FileMode.Create))
            {
                compiler.Compile(stream);
            }
        }
    }
}
