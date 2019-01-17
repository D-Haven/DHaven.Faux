using DHaven.Faux.Compiler;
using DHaven.Faux.HttpSupport;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Reflection;

namespace DHaven.Faux
{
    /// <summary>
    /// Registers and creates all the Faux generated services in your application.  Do not
    /// use this class if you are using Dependency Injection... this is intended for quick
    /// and dirty stand-alone applications where there is no other option.
    /// </summary>
    /// <remarks>
    /// NOTE: Do not use this class if you are already using Dependency Injection (i.e. in
    /// an Net Core Web API).  Only use this if you need to access your implementations
    /// without that kind of set up.
    /// </remarks>
    public class FauxCollection
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IFauxFactory factory;

        public FauxCollection(Type starterType, IConfiguration configuration = null)
        {
            var collection = new ServiceCollection();
            serviceProvider = ConfigureServices(collection, configuration, starterType).BuildServiceProvider();

            factory = serviceProvider.GetRequiredService<IFauxFactory>();
        }

        public TService GetInstance<TService>()
            where TService : class
        {
            return serviceProvider.GetService<TService>();
        }

        public TService CreateInstance<TService>(IHttpClient client)
            where TService : class // really interface
        {
            return CreateInstance(typeof(TService).GetTypeInfo(), client) as TService;
        }

        private object CreateInstance(TypeInfo service, IHttpClient client)
        {
            return factory.Create(service, client);
        }

        private static IServiceCollection ConfigureServices(IServiceCollection services, IConfiguration configuration,
            Type starterType)
        {
            if (configuration == null)
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables();
                configuration = builder.Build();
            }

            services.AddLogging(logger => logger.AddDebug().AddConfiguration(configuration));
            services.AddFaux(configuration, starterType);

            return services;
        }
    }
}
