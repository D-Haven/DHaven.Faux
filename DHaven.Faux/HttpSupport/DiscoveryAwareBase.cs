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
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
// ReSharper disable MemberCanBePrivate.Global

namespace DHaven.Faux.HttpSupport
{
    /// <summary>
    /// Base class for the implementations, helps with otherwise tricky things like
    /// path values, etc.
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public abstract class DiscoveryAwareBase
    {
        private readonly Uri baseUri;

        protected DiscoveryAwareBase(string serviceName, string baseRoute)
        {
            // Strip leading slashes so that the base URI is assembled correctly
            if (baseRoute.StartsWith("/"))
            {
                baseRoute = baseRoute.Substring(1);
            }
            
            var uriString = $"http://{serviceName}/{baseRoute}/";

            // Handle when there is no base route, but there is a service name.
            if (uriString.EndsWith("//"))
            {
                uriString = uriString.Substring(0, uriString.Length - 1);
            }
            
            baseUri = new Uri(uriString);
        }

        protected HttpRequestMessage CreateRequest(HttpMethod method, string endpoint, IDictionary<string,object> pathVariables, IDictionary<string,string> requestParameters)
        {
            var serviceUri = GetServiceUri(endpoint, pathVariables, requestParameters);
            Debug.WriteLine($"Request: {serviceUri}");
            return new HttpRequestMessage(method, serviceUri);
        }

        protected static HttpResponseMessage Invoke(HttpRequestMessage message)
        {
            return InvokeAsync(message).Result;
        }

        protected static async Task<HttpResponseMessage> InvokeAsync(HttpRequestMessage message)
        {
            var response = await DiscoverySupport.Client.SendAsync(message);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"{message.Method} {message.RequestUri} {response.StatusCode} {response.ReasonPhrase}");
            }

            return response;
        }

        protected static StringContent ConvertToJson(object data)
        {
            var json = JsonConvert.SerializeObject(data);
            return  new StringContent(json, Encoding.UTF8, "application/json");
        }

        protected static StreamContent StreamRawContent(Stream stream)
        {
            return new StreamContent(stream);
        }

        protected TResponse ConvertToObject<TResponse>(HttpResponseMessage responseMessage)
        {
            return ConvertToObjectAsync<TResponse>(responseMessage).Result;
        }

        protected static T GetHeaderValue<T>(HttpResponseMessage responseMessage, string headerName)
        {
            // Because Microsoft.  Y U so stupid?
            var value = headerName.StartsWith("Content-") 
                ? responseMessage.Content.Headers.GetValues(headerName).FirstOrDefault()
                : responseMessage.Headers.GetValues(headerName).FirstOrDefault();

            if (typeof(IConvertible).IsAssignableFrom(typeof(T)))
            {
                return (T) Convert.ChangeType(value, typeof(T));
            }

            return JsonConvert.DeserializeObject<T>(value);
        }

        protected static async Task<TResponse> ConvertToObjectAsync<TResponse>(HttpResponseMessage responseMessage)
        {
            return responseMessage.StatusCode == HttpStatusCode.NoContent
                ? default(TResponse)
                : JsonConvert.DeserializeObject<TResponse>(await responseMessage.Content.ReadAsStringAsync());
        }

        private Uri GetServiceUri(string endPoint, IDictionary<string, object> variables, IDictionary<string,string> requestParameters)
        {
            endPoint = variables.Aggregate(endPoint, (current, entry) => current.Replace($"{{{entry.Key}}}", entry.Value.ToString()));

            var query = new StringBuilder();
            foreach (var entry in requestParameters)
            {
                query.Append(query.Length == 0 ? "?" : "&")
                    .Append(WebUtility.UrlEncode(entry.Key))
                    .Append("=")
                    .Append(WebUtility.UrlEncode(entry.Value));
            }

            if (query.Length > 0)
            {
                endPoint += query.ToString();
            }

            if (endPoint.StartsWith("/"))
            {
                endPoint = endPoint.Substring(1);
            }

            return new Uri(baseUri, endPoint);
        }
    }
}