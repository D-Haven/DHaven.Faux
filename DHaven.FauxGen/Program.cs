using System;
using System.IO;
using System.Reflection;
using CommandLine;
using DHaven.Faux;
using DHaven.Faux.Compiler;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DHaven.FauxGen
{
    internal static class Program
    {
        private static ILogger logger;

        private static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
               .AddEnvironmentVariables();

            Configuration = builder.Build();

            Parser.Default.ParseArguments<CommandLineOptions>(args)
                .WithParsed(GenerateAssembly);
        }

        private static IConfiguration Configuration { get; set; }

        private static void GenerateAssembly(CommandLineOptions opts)
        {
            var absoluteAssemblyPath = Path.GetFullPath(opts.InputAssemblyPath);
            var assembly = Assembly.LoadFile(absoluteAssemblyPath);
     
            var services = new ServiceCollection();
            services.AddLogging(logging => logging.AddDebug().AddConsole());
            services.AddOptions();
            services.AddFaux(Configuration, assembly);

            var serviceProvider = services.BuildServiceProvider();
            logger = serviceProvider.GetService<ILogger>();

            var outputAssembly = opts.OutputAssemblyName ?? $"Generated.{Path.GetFileName(opts.InputAssemblyPath)}";
            
            logger.LogInformation($"Converting {absoluteAssemblyPath} to {outputAssembly}");

            var classGenerator = serviceProvider.GetService<IWebServiceClassGenerator>();
            var compiler = serviceProvider.GetService<WebServiceCompiler>();

            opts.ApplyToConfig(classGenerator.Config);

            foreach (var @interface in assembly.GetExportedTypes())
            {
                if (@interface.IsInterface && @interface.GetCustomAttribute<FauxClientAttribute>() != null)
                {
                    compiler.RegisterInterface(@interface.GetTypeInfo());
                }
            }

            compiler.Compile(outputAssembly);
            logger.LogInformation($"Successfully compiled the assembly {outputAssembly}");
        }
    }
}
