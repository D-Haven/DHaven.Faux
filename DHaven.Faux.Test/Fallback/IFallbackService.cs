using System.Threading.Tasks;

namespace DHaven.Faux.Test.Fallback
{
    [HystrixFauxClient("fallback", fallback: typeof(DummyFallback))]
    public interface IFallbackService
    {
        [HttpGet("fortune")]
        [return:Body]Task<string> GetFortune();

        [HttpPost]
        [return: ResponseHeader("Location")] Task<string> AddFortune(string newFortune);
    }
}
