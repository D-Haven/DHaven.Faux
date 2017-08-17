using System.Collections.Generic;
using DHaven.Faux;

namespace ExampleClient
{
    [Service("values")]
    [Route("api/values")]
    interface IValuesService
    {
        [HttpGet]
        IEnumerable<string> Get();

        [HttpGet("{id}")]
        string Get([PathValue] int id);

        [HttpPost]
        void Post([Body] string value);

        [HttpPut("{id}")]
        void Put([PathValue] int id, [Body] string value);

        [HttpDelete("{id}")]
        void Delete([PathValue] int id);
    }
}
