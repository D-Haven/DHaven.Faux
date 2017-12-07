using Microsoft.Extensions.Configuration;
using Steeltoe.Discovery.Client;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;

namespace DHaven.Faux.Compiler
{
    /// <summary>
    /// Manage the HttpClient for hte application.  NOTE: HttpClient is designed to be a singleton even though it implements IDisposable.
    /// See https://aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/ for more details
    /// </summary>
    internal static class DiscoverySupport
    {
        static DiscoverySupport()
        {
            var builder = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
               .AddEnvironmentVariables();

            var factory = new DiscoveryClientFactory(new DiscoveryOptions(builder.Build()));

            DiscoveryHttpClientHandler handler = new DiscoveryHttpClientHandler(factory.CreateClient() as IDiscoveryClient);
            Client = new HttpClient(handler, false);
            Client.DefaultRequestHeaders
                  .Accept
                  .Add(new MediaTypeWithQualityHeaderValue("application/json"));

        }

        internal static HttpClient Client { get; }
    }
}
