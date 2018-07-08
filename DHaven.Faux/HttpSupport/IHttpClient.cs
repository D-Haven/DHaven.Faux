﻿#region Copyright 2018 D-Haven.org

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

using System.Net.Http;
using System.Threading.Tasks;

namespace DHaven.Faux.HttpSupport
{
    /// <summary>
    /// Allows the global HttpClient to be global
    /// </summary>
    public interface IHttpClient
    {
        /// <summary>
        /// Send the Http request and asynchronously get a response.
        /// </summary>
        /// <param name="message">the request</param>
        /// <returns>the HttpResponse</returns>
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage message);
    }
}