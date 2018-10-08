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
using System.Linq;
using System.Reflection;
using DHaven.Faux.Compiler;
using DHaven.Faux.DependencyInjection;
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
    public static class FauxExtensions
    {
        /// <summary>
        /// Register an Interface for Faux to generate the actual instance.
        /// </summary>
        /// <param name="services"></param>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public static IServiceCollection AddFaux(this IServiceCollection services, IConfiguration configuration, Action<IFauxRegistrar> registrations = null)
        {
            services.AddDiscoveryClient(new DiscoveryOptions(configuration) { ClientType = DiscoveryClientType.EUREKA });
            services.Configure<CompilerConfig>(configuration.GetSection("Faux"));
            services.AddSingleton<IWebServiceClassGenerator, CoreWebServiceClassGenerator>();

            var registrar = new Registrar();
            registrations?.Invoke(registrar);

            services.AddSingleton<IFauxRegistrar>(registrar);
            services.AddSingleton<WebServiceCompiler>();
            services.AddSingleton<IFauxFactory, FauxFactory>();

            services.AddSingleton<IHttpClient>(provider => 
            {
                IDiscoveryClient client = provider.GetService<IDiscoveryClient>();
                ILogger<DiscoveryHttpClientHandler> logger = provider.GetRequiredService<ILogger<DiscoveryHttpClientHandler>>();
                
                return new HttpClientWrapper(new DiscoveryHttpClientHandler(client, logger));
            });

            foreach (var info in registrar.GetRegisteredServices())
            {
                services.AddSingleton(info, (provider) =>
                {
                    IFauxFactory factory = provider.GetRequiredService<IFauxFactory>();
                    return factory.Create(info);
                });
            }

            return services;
        }
    }
}