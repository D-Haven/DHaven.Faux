using DHaven.Faux.Compiler;
using DHaven.Faux.DependencyInjection;
using DHaven.Faux.HttpSupport;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Reflection;
using System.Threading;

namespace DHaven.Faux
{
    /// <summary>
    /// Registers and creates all the Faux generated services in your application.
    /// </summary>
    /// <remarks>
    /// NOTE: Do not use this class if you are already using Dependency Injection (i.e. in
    /// an Net Core Web API).  Only use this if you need to access your implementations
    /// without that kind of set up.
    /// </remarks>
    public class FauxCollection
    {
        private static readonly Type[] ConstructorTypes = new Type[] { typeof(IHttpClient) };
        private readonly IServiceProvider serviceProvider;
        private readonly IFauxFactory factory;

        public FauxCollection(Action<IFauxRegistrar> register = null)
        {
            var collection = new FauxServiceCollection();
            serviceProvider = ConfigureServices(collection, register).BuildFauxServiceProvider();

            factory = serviceProvider.GetRequiredService<IFauxFactory>();
        }

        internal void RegisterInterface<TService>()
            where TService : class
        {
            factory.RegisterInterface<TService>();
        }

        public TService CreateInstance<TService>()
            where TService : class
        {
            return serviceProvider.GetService<TService>() as TService;
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

        private IServiceCollection ConfigureServices(IServiceCollection services, Action<IFauxRegistrar> register = null)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            services.AddSingleton(new LoggerFactory().AddDebug());
            services.AddLogging();
            services.AddOptions();

            services.AddFaux(builder.Build(), register);

            return services;
        }
    }
}
