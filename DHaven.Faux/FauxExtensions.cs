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
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using DHaven.Faux.Compiler;
using DHaven.Faux.HttpSupport;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Steeltoe.Discovery.Client;

namespace DHaven.Faux
{
    /// <summary>
    /// Handle integration with built in dependency injection.
    /// </summary>
    public static class FauxExtensions
    {

        /// <summary>
        /// Register an Interface for Faux to generate the actual instance.
        /// </summary>
        /// <param name="services">the IServiceCollection we are populating</param>
        /// <param name="configuration">the IConfiguration root object for services</param>
        /// <param name="starterType">a type in the highest level assembly you want searched</param>
        /// <returns>the configured service collection</returns>       
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        [SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global")]
        public static IServiceCollection AddFaux(this IServiceCollection services, IConfiguration configuration,
            Type starterType = null)
        {
            return AddFaux(services, configuration, starterType?.Assembly ?? Assembly.GetEntryAssembly());
        }
        
        /// <summary>
        /// Register an Interface for Faux to generate the actual instance.
        /// </summary>
        /// <param name="services">the IServiceCollection we are populating</param>
        /// <param name="configuration">the IConfiguration root object for services</param>
        /// <param name="starterAssembly">the highest level assembly you want searched</param>
        /// <returns>the configured service collection</returns>       
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        [SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global")]
        public static IServiceCollection AddFaux(this IServiceCollection services, IConfiguration configuration,
            Assembly starterAssembly)
        {
            services.AddLogging();
            services.AddOptions();

            // Really need to handle the faux discovery adding new services at runtime
            var logFactory = new LoggerFactory().AddDebug(LogLevel.Trace);
            var fauxDiscovery = new FauxDiscovery(starterAssembly ?? Assembly.GetEntryAssembly(), logFactory.CreateLogger<FauxDiscovery>());
            services.AddDiscoveryClient(new DiscoveryOptions(configuration) { ClientType = DiscoveryClientType.EUREKA });
            services.Configure<CompilerConfig>(configuration.GetSection("Faux"));
            services.AddSingleton<IWebServiceClassGenerator, CoreWebServiceClassGenerator>();
            services.AddSingleton<IMethodClassGenerator, HystrixCommandClassGenerator>();
            services.AddSingleton<IWebServiceClassGenerator, HystrixWebServiceClassGenerator>();
           
            services.AddSingleton(fauxDiscovery);

            services.AddSingleton<WebServiceCompiler>();
            services.AddSingleton<IFauxFactory, FauxFactory>();

            services.AddSingleton<IHttpClient>(provider => 
            {
                var client = provider.GetService<IDiscoveryClient>();
                var logger = provider.GetRequiredService<ILogger<DiscoveryHttpClientHandler>>();
                
                return new HttpClientWrapper(new DiscoveryHttpClientHandler(client, logger));
            });

            foreach (var info in fauxDiscovery.GetAllFauxInterfaces())
            {
                services.AddSingleton(info, provider =>
                {
                    var factory = provider.GetRequiredService<IFauxFactory>();
                    return factory.Create(info);
                });
            }

            return services;
        }
    }
}