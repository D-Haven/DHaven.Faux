using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Steeltoe.Discovery;
using Steeltoe.Discovery.Client;

namespace DHaven.Faux.Compiler
{
    public class DiscoveryAwareBase
    {
        private readonly DiscoveryHttpClientHandler handler;

        public DiscoveryAwareBase(IDiscoveryClient client)
        {
            handler = new DiscoveryHttpClientHandler(client);
        }

        protected async Task<String> GetString(string url)
        {
            var client = GetClient();
            return await client.GetStringAsync(url);
        }

        protected HttpClient GetClient()
        {
            return new HttpClient(handler, false);
        }
    }
}
