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
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Steeltoe.Discovery.Client;

namespace DHaven.Faux.Compiler
{
    public class DiscoveryAwareBase
    {
        private readonly Uri baseUri;
        private readonly DiscoveryHttpClientHandler handler;

        public DiscoveryAwareBase(IDiscoveryClient client, string serviceName, string baseRoute)
        {
            baseUri = new Uri($"http://{serviceName}/{baseRoute}");
            handler = new DiscoveryHttpClientHandler(client);
        }

        protected HttpRequestMessage CreateRequest(HttpMethod method, string endpoint)
        {
            return new HttpRequestMessage(method, GetServiceUri(endpoint));
        }

        protected HttpResponseMessage Invoke(HttpRequestMessage message)
        {
            return InvokeAsync(message).Result;
        }

        protected async Task<HttpResponseMessage> InvokeAsync(HttpRequestMessage message)
        {
            using (var client = GetClient())
            {
                var response = await client.SendAsync(message);

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException(
                        $"Unsuccessful response status: {response.StatusCode} {response.ReasonPhrase}");
                }

                return response;
            }
        }

        protected StringContent ConvertToJson(object data)
        {
            var json = JsonConvert.SerializeObject(data);
            return  new StringContent(json, Encoding.UTF8, "application/json");
        }

        protected TResponse ConvertToObject<TResponse>(HttpResponseMessage responseMessage)
        {
            return ConvertToObjectAsync<TResponse>(responseMessage).Result;
        }

        protected async Task<TResponse> ConvertToObjectAsync<TResponse>(HttpResponseMessage responseMessage)
        {
            return responseMessage.StatusCode == HttpStatusCode.NoContent
                ? default(TResponse)
                : JsonConvert.DeserializeObject<TResponse>(await responseMessage.Content.ReadAsStringAsync());
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