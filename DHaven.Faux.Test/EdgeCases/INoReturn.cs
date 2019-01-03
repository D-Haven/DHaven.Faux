using System.Threading.Tasks;

namespace DHaven.Faux.Test.EdgeCases
{
    [FauxClient("no-return")]
    public interface INoReturn
    {
        [HttpGet]
        void Ping();

        [HttpPost]
        Task PingAsync();
    }
}