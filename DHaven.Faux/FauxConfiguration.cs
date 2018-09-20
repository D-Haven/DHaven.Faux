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

using System.Diagnostics.CodeAnalysis;
using System.IO;
using DHaven.Faux.Compiler;
using DHaven.Faux.HttpSupport;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
            
            DiscoverySupport.Configure();
            WebServiceClassGenerator.Configure();

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
        public static IServiceCollection AddFaux<TService>(this IServiceCollection services)
            where TService : class
        {
            var className = TypeFactory.RegisterInterface<TService>();
            services.AddSingleton(provider => TypeFactory.CreateInstance<TService>(className));
            
            return services;
        }

        internal static IConfiguration Configuration { get; private set; }
        internal static ILoggerFactory LogFactory { get; private set; }
    }
}