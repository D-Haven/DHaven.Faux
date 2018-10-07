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
        private static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
               .AddEnvironmentVariables();

            Configuration = builder.Build();

            Parser.Default.ParseArguments<CommandLineOptions>(args)
                .WithParsed(GenerateAssembly);

            Console.In.ReadLine();
        }

        private static IConfiguration Configuration { get; set; }

        private static void GenerateAssembly(CommandLineOptions opts)
        {
            var outputAssembly = opts.OutputAssemblyName ?? $"Generated.{Path.GetFileName(opts.InputAssemblyPath)}";
            var assembly = Assembly.LoadFile(opts.InputAssemblyPath);

            // create service provider
            var serviceProvider = ConfigureServices(new ServiceCollection()).BuildServiceProvider();

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
        }

        private static IServiceCollection ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(new LoggerFactory().AddConsole().AddDebug());
            services.AddLogging();

            services.AddFaux(Configuration, (registrar) => { });

            return services;
        }
    }
}
