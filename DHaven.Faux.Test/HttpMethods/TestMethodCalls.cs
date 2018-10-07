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

using System.Net;
using System.Net.Http;
using System.Text;
using Xunit;
using FluentAssertions;
using System;

namespace DHaven.Faux.Test.HttpMethods
{
    public class TestMethodCalls
    {
        [Fact]
        public void FauxGeneratesCallWithHttpGet()
        {
            var service = Test.GenerateService<ITodoService>(Test.MockRequest(
                req =>
                {
                    req.Method.Should().BeEquivalentTo(HttpMethod.Get);
                    req.RequestUri.ToString().Should().BeEquivalentTo("http://todo/");
                },
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("[]", Encoding.UTF8, "application/json")
                }));

            var result = service.List();
            result.Should().BeEmpty();
        }

        [Fact]
        public void FauxGeneratesDeleteWithPathValue()
        {
            var service = Test.GenerateService<ITodoService>(Test.MockRequest(
                req =>
                {
                    req.Method.Should().BeEquivalentTo(HttpMethod.Delete);
                    req.RequestUri.ToString().Should().BeEquivalentTo("http://todo/21");
                },
                new HttpResponseMessage(HttpStatusCode.NoContent)));

            service.Delete(21);
        }

        [Fact]
        public void FauxGeneratesPutWithObject()
        {
            var service = Test.GenerateService<ITodoService>(Test.MockRequest(
                req =>
                {
                    req.Method.Should().BeEquivalentTo(HttpMethod.Put);
                    req.RequestUri.ToString().Should().BeEquivalentTo("http://todo/");
                },
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Headers = { Location = new Uri("http://todo/22") },
                    Content = new StringContent("22")
                }));

            var id = service.Add("Another thing to do");
            id.Should().Be(22);
        }

        [Fact]
        public void GetByIdGetsObject()
        {
            var service = Test.GenerateService<ITodoService>(Test.MockRequest(
                request =>
                {
                    request.Method.Should().BeEquivalentTo(HttpMethod.Get);
                    request.RequestUri.ToString().Should().BeEquivalentTo("Http://todo/21");
                },
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Headers = { Location = new Uri("http://todo/21") },
                    Content = new StringContent("'This is a todo item'", Encoding.UTF8, "application/json")
                }));

            service.Get(21).Should().BeEquivalentTo("This is a todo item");
        }
    }
}