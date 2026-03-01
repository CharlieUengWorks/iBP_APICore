using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebApplication1.Controllers
{
    [Route("api/metrics")]
    [ApiController]
    public class MetricsController : ControllerBase
    {
        // GET: api/<HelloWorldController>
        [HttpGet]
        public string Get()
        {
            var visitor = new UpdateVisitor();
            var data = visitor.GetMonitorData();
            //return new string[] { $"{gpu.name}", $"{gpu.CoreTemperature}", $"{gpu.CoreLoad}" };
            return JsonSerializer.Serialize(data);
        }

        [HttpGet("{key}")]
        public string Get(string key)
        {
            return key;
        }

        // POST api/<HelloWorldController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<HelloWorldController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<HelloWorldController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
