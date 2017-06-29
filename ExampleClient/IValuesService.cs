using System;
using System.Collections.Generic;
using System.Text;
using DHaven.Faux;

namespace ExampleClient
{
    [Route("api/values", ServiceName = "values")]
    interface IValuesService
    {
        [HttpGet]
        IEnumerable<string> Get();

        [HttpGet("{id}")]
        string Get(int id);

        [HttpPost]
        void Post([FromBody] string value);

        [HttpPut("{id}")]
        void Put(int id, [FromBody] string value);

        [HttpDelete("{id}")]
        void Delete(int id);
    }
}
