using Microsoft.AspNetCore.Mvc;
using Services.Web.Auth;
using Microsoft.AspNetCore.Authorization;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = "Devices")]
public class Sensors : ControllerBase
{
    private readonly Services.SensorsAndActuators.ISensors _sensor;
    public Sensors(Services.SensorsAndActuators.ISensors sensor)
    {
        _sensor = sensor;
    }
    [HttpPost]
    public void SaveInformation(string token, [FromBody]object Value)
    {
        _sensor.SaveInformation(token, Value);
    }

    /*
    // GET: api/<Sensors>
    [HttpGet]
    public IEnumerable<string> Get()
    {
        return new string[] { "value1", "value2" };
    }

    // GET api/<Sensors>/5
    [HttpGet("{id}")]
    public string Get(int id)
    {
        return "value";
    }

    // POST api/<Sensors>
    [HttpPost]
    public void Post([FromBody] string value)
    {
    }

    // PUT api/<Sensors>/5
    [HttpPut("{id}")]
    public void Put(int id, [FromBody] string value)
    {
    }

    // DELETE api/<Sensors>/5
    [HttpDelete("{id}")]
    public void Delete(int id)
    {
    }*/
}
