using Xunit;
using FluentAssertions;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;

namespace DHaven.Faux.Test.Fallback
{
    public class TestFallback
    {
        [Fact]
        public async Task ReturnsDefaultValueFromFallbackClass()
        {
            var fallback = new DummyFallback();
            var service = Test.GenerateService<IFallbackService>();

            var fortune = await service.GetFortune();
            fortune.Should().BeEquivalentTo(await fallback.GetFortune());
        }

        [Fact]
        public async Task ShouldMatchMethodForMethod()
        {
            var fallback = new DummyFallback();
            var service = Test.GenerateService<IFallbackService>();

            var fortune = await service.AddFortune("ignored");
            fortune.Should().BeEquivalentTo(await fallback.AddFortune("ignored"));
        }
    }
}
