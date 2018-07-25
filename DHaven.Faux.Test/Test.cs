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
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading.Tasks;
using DHaven.Faux.HttpSupport;
using DHaven.Faux.Test.HttpMethods;
using DHaven.Faux.Test.ReturnTypes;
using Moq;

namespace DHaven.Faux.Test
{
    public static class Test
    {
        public static readonly Faux<ITodoService> FauxTodo = new Faux<ITodoService>();
        public static readonly Faux<IReturnService> FauxReturn = new Faux<IReturnService>();

        public static IHttpClient MockRequest(Expression<Func<HttpRequestMessage, bool>> verifyRequest,
            HttpResponseMessage response)
        {
            var mockClient = new Mock<IHttpClient>();

            mockClient.Setup(m => m.SendAsync(It.Is(verifyRequest))).Returns(Task.FromResult(response));

            return mockClient.Object;
        }
    }
}