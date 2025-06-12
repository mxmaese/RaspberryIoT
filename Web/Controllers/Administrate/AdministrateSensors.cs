using Microsoft.AspNetCore.Mvc;
using Services.Web.Auth;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Api.Controllers.Administrate;

[Route("api/[controller]")]
[ApiController]
[ApiTokenAuthentication]
public class AdministrateSensors : ControllerBase
{
    private readonly Services.Administrate.ISensor _sensor;
    public AdministrateSensors(Services.Administrate.ISensor sensor)
    {
        _sensor = sensor;
    }

    [HttpGet]
    public Entities.Sensor GetSensor(int Id)
    {
        return _sensor.GetSensor(Id).FirstOrDefault() ?? new ();
    }

    [HttpPost]
    public void CreateSensor([FromBody] CreateSensorClass Inputsensor)
    {
        var sensor = new Entities.Sensor
        {
            Name = Inputsensor.Name,
            LocationId = Inputsensor.LocationId,
            OwnerId = Inputsensor.OwnerId,
            Status = Inputsensor.Status,
            GroupId = Inputsensor.GroupId,
            AssignedVariableId = Inputsensor.AssignedVariableId,
            CreatedAt = DateTime.Now,
            LastReferenceChange = DateTime.Now,
            Token = "This Shuld never be seen"
        };
        _sensor.CreateSensor(sensor);
    }

    [HttpPut]
    public void UpdateSensor(Entities.Sensor sensor)
    {
        _sensor.UpdateSensor(sensor);
    }

    [HttpDelete]
    public void DeleteSensor(int SensorId)
    {
        _sensor.DeleteSensor(SensorId);
    }

    /*// GET: api/<AdministrateSensors>
    [HttpGet]
    public IEnumerable<string> Get()
    {
        return new string[] { "value1", "value2" };
    }

    // GET api/<AdministrateSensors>/5
    [HttpGet("{id}")]
    public string Get(int id)
    {
        return "value";
    }

    // POST api/<AdministrateSensors>
    [HttpPost]
    public void Post([FromBody] string value)
    {
    }

    // PUT api/<AdministrateSensors>/5
    [HttpPut("{id}")]
    public void Put(int id, [FromBody] string value)
    {
    }

    // DELETE api/<AdministrateSensors>/5
    [HttpDelete("{id}")]
    public void Delete(int id)
    {
    }*/

    public class CreateSensorClass
    {
        public string Name { get; set; }
        public int LocationId { get; set; }
        public int OwnerId { get; set; }
        public string Status { get; set; }
        public int GroupId { get; set; }
        public int AssignedVariableId { get; set; }
    }
}
