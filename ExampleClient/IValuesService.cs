using System.Collections.Generic;
using System.Threading.Tasks;
using DHaven.Faux;

namespace ExampleClient
{
    [FauxClient("valueService")]
    [Route("api/values")]
    public interface IValuesService
    {
        [HttpGet]
        Task<IEnumerable<string>> Get([RequestParameter] int page, [RequestParameter("page-size")] int size);

        [HttpGet("{id}")]
        Task<string> Get([PathValue] int id);

        [HttpPost]
        void Post([Body] string value);

        [HttpPut("{id}")]
        void Put([PathValue] int id, [Body] string value);

        [HttpDelete("{id}")]
        void Delete([PathValue] int id);
    }
}
