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
using System.Reflection;
using DHaven.Faux.Compiler;
using DHaven.Faux.HttpSupport;
using DHaven.Faux.Test.HttpMethods;
using DHaven.Faux.Test.ParameterTypes;
using DHaven.Faux.Test.ReturnTypes;
using Moq;

namespace DHaven.Faux.Test
{
    public static class Test
    {
        static Test()
        {
            FauxConfiguration.ClassGenerator.OutputSourceFiles = true;
            FauxConfiguration.ClassGenerator.SourceFilePath = "./dhaven-faux";
            Compiler = new WebServiceCompiler(FauxConfiguration.ClassGenerator);
            Compiler.RegisterInterface<ITodoService>();
            Compiler.RegisterInterface<IReturnService>();
            Compiler.RegisterInterface<IBlobStore>();
            Assembly = Compiler.Compile(null);
        }

        private static WebServiceCompiler Compiler { get; }
        private static Assembly Assembly { get; }

        public static TService GenerateService<TService>(IHttpClient client)
            where TService : class
        {
            var service = typeof(TService).GetTypeInfo();
            var className = Compiler.GetImplementationName(service);
            var type = Assembly.GetType(className);
            var constructor = type?.GetConstructor(new Type[] { typeof(IHttpClient) });
            return constructor?.Invoke(new object[] { client }) as TService;
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