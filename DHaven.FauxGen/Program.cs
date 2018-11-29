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
            // create service provider which builds the logger
            var serviceProvider = ConfigureServices(new ServiceCollection()).BuildServiceProvider();

            var outputAssembly = opts.OutputAssemblyName ?? $"Generated.{Path.GetFileName(opts.InputAssemblyPath)}";
            logger.LogInformation($"Converting {opts.InputAssemblyPath} to {outputAssembly}");
            var assembly = Assembly.LoadFile(opts.InputAssemblyPath);

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

        private static IServiceCollection ConfigureServices(IServiceCollection services)
        {
            var logFactory = new LoggerFactory().AddConsole().AddDebug();
            logger = logFactory.CreateLogger(typeof(Program));
            services.AddSingleton(logFactory);
            services.AddLogging();
            services.AddOptions();

            services.AddFaux(Configuration);

            return services;
        }
    }
}
