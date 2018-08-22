using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace ExampleApi.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        private readonly Dictionary<int, string> values = new Dictionary<int, string>
        {
            { 1, "one" },
            { 2, "two" }
        };

        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return values.Values.OrderBy(v=>v);
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (values.TryGetValue(id, out var value))
            {
                return Ok(value);
            }

            return NotFound();
        }

        // POST api/values
        [HttpPost]
        public IActionResult Post([FromBody]string value)
        {
            if (values.Values.Contains(value))
            {
                return BadRequest();
            }

            var newId = values.Count + 1;

            var location = Url.Action("Get", new {id = newId});

            return Created(location, value);
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody]string value)
        {
            if (!values.TryGetValue(id, out _))
            {
                return NotFound();
            }

            values[id] = value;
            return Ok(value);

        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            return values.Remove(id) ? (IActionResult)NoContent() : NotFound();
        }
    }
}
