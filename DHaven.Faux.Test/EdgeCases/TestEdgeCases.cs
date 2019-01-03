using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace DHaven.Faux.Test.EdgeCases
{
    public class TestEdgeCases
    {
        [Fact]
        public void CanCallNormalServiceWithNoReturn()
        {
            var service = Test.GenerateService<INoReturn>(Test.MockRequest(
                req =>
                {
                    req.Method.Should().BeEquivalentTo(HttpMethod.Get);
                    req.RequestUri.ToString().Should().BeEquivalentTo("http://no-return/");
                },
                new HttpResponseMessage(HttpStatusCode.NoContent)));

            service.Ping();
        }

        [Fact]
        public void CanCallHystrixServiceWithNoReturn()
        {
            var service = Test.GenerateService<IHystrixNoReturn>(Test.MockRequest(
                req =>
                {
                    req.Method.Should().BeEquivalentTo(HttpMethod.Get);
                    req.RequestUri.ToString().Should().BeEquivalentTo("http://hystrix-no-return/");
                },
                new HttpResponseMessage(HttpStatusCode.NoContent)));

            service.Ping();           
        }
        
        [Fact]
        public async Task CanCallNormalAsyncServiceWithNoReturn()
        {
            var service = Test.GenerateService<INoReturn>(Test.MockRequest(
                req =>
                {
                    req.Method.Should().BeEquivalentTo(HttpMethod.Post);
                    req.RequestUri.ToString().Should().BeEquivalentTo("http://no-return/");
                },
                new HttpResponseMessage(HttpStatusCode.NoContent)));

            await service.PingAsync();
        }

        [Fact]
        public async Task CanCallHystrixAsyncServiceWithNoReturn()
        {
            var service = Test.GenerateService<IHystrixNoReturn>(Test.MockRequest(
                req =>
                {
                    req.Method.Should().BeEquivalentTo(HttpMethod.Post);
                    req.RequestUri.ToString().Should().BeEquivalentTo("http://hystrix-no-return/");
                },
                new HttpResponseMessage(HttpStatusCode.NoContent)));

            await service.PingAsync();           
        }
    }
}