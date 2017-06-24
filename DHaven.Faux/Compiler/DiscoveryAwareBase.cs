#region Copyright 2017 D-Haven.org

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
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Steeltoe.Discovery.Client;

namespace DHaven.Faux.Compiler
{
    public class DiscoveryAwareBase
    {
        private readonly Uri baseUri = new Uri("http://serviceName/api/base");
        private readonly DiscoveryHttpClientHandler handler;

        public DiscoveryAwareBase(IDiscoveryClient client)
        {
            handler = new DiscoveryHttpClientHandler(client);
        }

        protected async Task<TResponse> SendAsJsonAsync<TRequest, TResponse>(
            HttpMethod method, string endPoint, TRequest data)
        {
            using (var client = GetClient())
            {
                var requestMessage = new HttpRequestMessage(method, GetServiceUri(endPoint));

                if (data != null)
                {
                    var json = JsonConvert.SerializeObject(data);
                    requestMessage.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }

                var responseMessage = await client.SendAsync(requestMessage);

                if (responseMessage.IsSuccessStatusCode)
                {
                    switch (responseMessage.StatusCode)
                    {
                        case HttpStatusCode.NoContent:
                            return default(TResponse);

                        default:
                            return JsonConvert.DeserializeObject<TResponse>(await responseMessage.Content.ReadAsStringAsync());
                    }
                }

                throw new HttpRequestException(
                    $"Unsuccessful response status: {responseMessage.StatusCode} {responseMessage.ReasonPhrase}");
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