#region Copyright 2018 D-Haven.org

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

using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Steeltoe.Discovery.Client;

namespace DHaven.Faux.HttpSupport
{
    /// <summary>
    /// Manage the HttpClient for hte application.  NOTE: HttpClient is designed to be a singleton even though it implements IDisposable.
    /// See https://aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/ for more details
    /// </summary>
    public static class DiscoverySupport
    {
        static DiscoverySupport()
        {
            var builder = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
               .AddEnvironmentVariables();

            Configuration = builder.Build();

            var logFactory = new LoggerFactory();
            logFactory.AddDebug(LogLevel.Trace);
            LogFactory = logFactory;

            var factory = new DiscoveryClientFactory(new DiscoveryOptions(Configuration));
            var handler = new DiscoveryHttpClientHandler(factory.CreateClient() as IDiscoveryClient, logFactory.CreateLogger<DiscoveryHttpClientHandler>());
            
            Client = new HttpClientWrapper(handler);
        }

        internal static IConfiguration Configuration { get; }

        internal static ILoggerFactory LogFactory { get; }

        public static IHttpClient Client { get; set; }
    }
}
