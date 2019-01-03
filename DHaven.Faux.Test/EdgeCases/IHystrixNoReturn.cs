using System.Threading.Tasks;

namespace DHaven.Faux.Test.EdgeCases
{
    [HystrixFauxClient("hystrix-no-return")]
    public interface IHystrixNoReturn
    {
        [HttpPost]
        void Ping();

        [HttpPost]
        Task PingAsync();
    }
}