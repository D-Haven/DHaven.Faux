using System.Threading.Tasks;

namespace DHaven.Faux.Test.Fallback
{
    public class DummyFallback : IFallbackService
    {
        [return: ResponseHeader("Location")]
        public Task<string> AddFortune(string newFortune)
        {
            return Task.FromResult("http://null.bitbucket.it/0");
        }

        [return: Body]
        public Task<string> GetFortune()
        {
            return Task.FromResult("Fortune unavailable.  Have a nice day!");
        }
    }
}
