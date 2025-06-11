using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Api.Controllers.Administrate;

[Route("api/[controller]")]
[ApiController]
public class AdministrateActuators : ControllerBase
{
    private readonly Services.Administrate.IActuator _AdministrateActuator;
    private readonly Services.SensorsAndActuators.IActuators _Actuator;
    public AdministrateActuators(Services.Administrate.IActuator AdministrateActuator, Services.SensorsAndActuators.IActuators Actuator)
    {
        _AdministrateActuator = AdministrateActuator;
        _Actuator = Actuator;
    }

    [HttpGet]
    public Entities.Actuator GetActuator(int Id)
    {
        return _AdministrateActuator.GetActuator(Id).FirstOrDefault() ?? new();
    }

    [HttpPost]
    public void CreateActuator([FromBody] CreateActuatorClass InputActuator)
    {
        var actuator = new Entities.Actuator
        {
            Name = InputActuator.Name,
            LocationId = InputActuator.LocationId,
            OwnerId = InputActuator.OwnerId,
            Status = InputActuator.Status,
            GroupId = InputActuator.GroupId,
            AssignedVariableId = InputActuator.AssignedVariableId,
            CreatedAt = DateTime.Now,
            LastReferenceChange = DateTime.Now,
            Token = "This Shuld never be seen"
        };
        _AdministrateActuator.CreateActuator(actuator);
    }

    [HttpPut]
    public void UpdateActuator(Entities.Actuator actuator)
    {
        _AdministrateActuator.UpdateActuator(actuator);
    }

    [HttpDelete]
    public void DeleteActuator(int ActuatorId)
    {
        _AdministrateActuator.DeleteActuator(ActuatorId);
    }

    public class CreateActuatorClass
    {
        public string Name { get; set; }
        public int LocationId { get; set; }
        public int OwnerId { get; set; }
        public string Status { get; set; }
        public int GroupId { get; set; }
        public int AssignedVariableId { get; set; }
    }
}
