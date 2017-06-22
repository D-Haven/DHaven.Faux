using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Steeltoe.Discovery;
using Steeltoe.Discovery.Client;

namespace DHaven.Faux.Compiler
{
    public class DiscoveryAwareBase
    {
        private readonly DiscoveryHttpClientHandler handler;
        private readonly Uri baseUri = new Uri("http://serviceName/api/base");

        public DiscoveryAwareBase(IDiscoveryClient client)
        {
            handler = new DiscoveryHttpClientHandler(client);
        }

        protected async Task<HttpResponseMessage> SendAsJsonAsync<TObject>(HttpMethod method, string endPoint, TObject data)
        {
            using (var client = GetClient())
            {
                var requestMessage = new HttpRequestMessage(method, GetServiceUri(endPoint));

                return await client.SendAsync(requestMessage);
            }
        }

        private Uri GetServiceUri(string endPoint)
        {
            return new Uri(baseUri, endPoint);
        }

        private HttpClient GetClient()
        {
            return new HttpClient(handler, false);
        }
    }
}
