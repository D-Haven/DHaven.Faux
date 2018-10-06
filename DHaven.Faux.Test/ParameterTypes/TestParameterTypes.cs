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

using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using DHaven.Faux.HttpSupport;
using FluentAssertions;
using Xunit;

namespace DHaven.Faux.Test.ParameterTypes
{
    public class TestParameterTypes
    {
        [Fact]
        public async Task GeneratesRequestHeadersWhenAllValuesArePresent()
        {
            var service = Test.FauxBlob.GenerateService(Test.MockRequest(
                req =>
                {
                    req.Method.Should().BeEquivalentTo(HttpMethod.Post);
                    req.RequestUri.ToString().Should().BeEquivalentTo("http://blob-service/api/storage/");
                    req.Content.Headers.ContentLength.Should().Be(10240);
                    req.Content.Headers.ContentType.MediaType.Should().Be("image/png");
                    req.Content.Headers.ContentDisposition.FileName.Should().Be("\"test.png\"");
                    req.Content.Headers.ContentDisposition.DispositionType.Should().Be("attachment");
                },
                new HttpResponseMessage(HttpStatusCode.NoContent)
                {
                    Headers = { ETag = new EntityTagHeaderValue("\"1234567890abcdef\"") }
                }));
            var content = new byte[10240];

            using (var stream = new MemoryStream(content))
            {
                (await service.Upload(stream, "image/png", content.Length, "attachment; filename=\"test.png\""))
                    .Should().Be("\"1234567890abcdef\"");
            }
        }
        
        [Fact]
        public async Task GeneratesFullUriWhenPathVariablesArePresent()
        {
            var service = Test.FauxBlob.GenerateService(Test.MockRequest(
                req =>
                {
                    req.Method.Should().BeEquivalentTo(HttpMethod.Get);
                    req.RequestUri.ToString().Should().BeEquivalentTo("http://blob-service/api/storage/1234567890abcdef");
                    req.Headers.GetValues("X-Content-Disposition").First().Should()
                        .Be("attachment; filename=\"test2.png\"");
                },
                new HttpResponseMessage(HttpStatusCode.NoContent)
                {
                    Content = new StreamContent(new MemoryStream(new byte[10240]))
                }));

            using (var stream = await service.Get("1234567890abcdef", "attachment; filename=\"test2.png\""))
            {
                stream.Length.Should().Be(10240);
            }
        }
    }
}