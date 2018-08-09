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
using DHaven.Faux.Test.HttpMethods;
using DHaven.Faux.Test.ReturnTypes;
using Moq;
using Xunit;

// Force single threaded execution for XUnit.  While the app itself
// is threadsafe, we have to share the global reference for the HttpClient.
// That's due to the way that HttpClient is designed to work.  Since the
// tests mock the IHttpClient and add validations, we don't want the test
// to fail just because 2 of them are running at the same time.
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace DHaven.Faux.Test
{
    public static class Test
    {
        static Test()
        {
            Compiler.WebServiceClassGenerator.OutputSourceFiles = true;
            Compiler.WebServiceClassGenerator.SourceFilePath = "./dhaven-faux";
            FauxTodo = new Faux<ITodoService>();
            FauxReturn = new Faux<IReturnService>();
        }
        
        public static readonly Faux<ITodoService> FauxTodo;
        public static readonly Faux<IReturnService> FauxReturn;

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