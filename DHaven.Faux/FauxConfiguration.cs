#region Copyright 2018 D-Haven.org
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using DHaven.Faux.Compiler;
using DHaven.Faux.HttpSupport;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Steeltoe.Discovery.Client;

namespace DHaven.Faux
{
    /// <summary>
    /// Handle integration with built in dependency injection.
    /// </summary>
    public static class FauxConfiguration
    {
        /// <summary>
        /// Default configuration if "UseFaux" is never called.
        /// </summary>
        static FauxConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            UseFaux(null, builder.Build());
        }

        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public static IApplicationBuilder UseFaux(this IApplicationBuilder app, IConfiguration configuration)
        {
            Configuration = configuration;

            var logFactory = new LoggerFactory();
            logFactory.AddDebug(LogLevel.Trace);
            LogFactory = logFactory;

            var factory = new DiscoveryClientFactory(new DiscoveryOptions(Configuration));
            var handler = new DiscoveryHttpClientHandler(factory.CreateClient() as IDiscoveryClient,
                LogFactory.CreateLogger<DiscoveryHttpClientHandler>());

            Client = new HttpClientWrapper(handler);

            ClassGenerator = new WebServiceClassGenerator(Configuration, LogFactory.CreateLogger<WebServiceClassGenerator>());

            return app;
        }

        /// <summary>
        /// Register an Interface for Faux to generate the actual instance.
        /// </summary>
        /// <param name="services"></param>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public static IServiceCollection AddFaux(this IServiceCollection services, Action<IFauxRegistrar> registrations)
        {
            services.AddSingleton<IHttpClient>(provider => 
            {
                IDiscoveryClient client = provider.GetRequiredService<IDiscoveryClient>();
                ILogger<DiscoveryHttpClientHandler> logger = provider.GetRequiredService<ILogger<DiscoveryHttpClientHandler>>();

                return new HttpClientWrapper(new DiscoveryHttpClientHandler(client, logger));
            });

            var registrar = new Registrar();
            registrations(registrar);

            foreach (var info in registrar.GetRegisteredServices())
            {
                services.AddSingleton(info, (provider) =>
                {
                    IHttpClient client = provider.GetRequiredService<IHttpClient>();
                    return TypeFactory.CreateInstance(info, Client);
                });
            }

            return services;
        }

        public static IWebServiceClassGenerator ClassGenerator { get; private set; }

        public static IHttpClient Client { get; private set; }

        internal static IConfiguration Configuration { get; private set; }

        internal static ILoggerFactory LogFactory { get; private set; }

        private class Registrar : IFauxRegistrar
        {
            private readonly IList<TypeInfo> services = new List<TypeInfo>();

            public IFauxRegistrar Register<TService>() where TService : class
            {
                TypeFactory.RegisterInterface<TService>();
                services.Add(typeof(TService).GetTypeInfo());
                return this;
            }

            public IEnumerable<TypeInfo> GetRegisteredServices()
            {
                foreach(var info in services)
                {
                    yield return info;
                }
            }
        }
    }
}