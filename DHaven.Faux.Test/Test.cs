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
using System.Net.Http;
using DHaven.Faux.HttpSupport;
using Moq;
using Steeltoe.Discovery.Client;

namespace DHaven.Faux.Test
{
    public static class Test
    {
        private static readonly FauxCollection Collection = new FauxCollection(typeof(Test));

        public static TService GenerateService<TService>()
            where TService : class
        {
            return Collection.GetInstance<TService>();
        }
        
        public static TService GenerateService<TService>(IHttpClient client)
            where TService : class
        {
            return Collection.CreateInstance<TService>(client);
        }


        public static IHttpClient MockRequest(Action<HttpRequestMessage> verifyRequest,
            HttpResponseMessage response)
        {
            var mockClient = new Mock<IHttpClient>();

            mockClient.Setup(m => m.SendAsync(It.IsAny<HttpRequestMessage>()))
                .ReturnsAsync(response)
                .Callback(verifyRequest);

            return mockClient.Object;
        }
    }
}