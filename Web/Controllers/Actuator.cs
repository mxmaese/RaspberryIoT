using Entities;
using Microsoft.AspNetCore.Mvc;
using Services.Administrate;
using Services.SensorsAndActuators;
using Services.Web.Auth;
using Microsoft.AspNetCore.Authorization;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = "Devices")]
public class Actuator : ControllerBase
{
    private readonly IActuators _Actuators;
    public Actuator (IActuators actuators)
    {
        _Actuators = actuators;
    }
    /*
    // GET: api/<Actuator>
    [HttpGet]
    public IEnumerable<string> Get()
    {
        return new string[] { "value1", "value2" };
    }

    // GET api/<Actuator>/5
    [HttpGet("{id}")]
    public string Get(int id)
    {
        return "value";
    }

    // POST api/<Actuator>
    [HttpPost]
    public void Post([FromBody] string value)
    {
    }

    // PUT api/<Actuator>/5
    [HttpPut("{id}")]
    public void Put(int id, [FromBody] string value)
    {
    }

    // DELETE api/<Actuator>/5
    [HttpDelete("{id}")]
    public void Delete(int id)
    {
    }*/
}
