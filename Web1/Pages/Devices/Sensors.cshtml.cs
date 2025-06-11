using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services.Administrate;
using Services.Variables;
using System.Security.Claims;

namespace Web.Pages.Devices;

public class SensorsModel : PageModel
{
    private readonly ISensor _sensor;
    private readonly IGeneralVariables _generalVariables;
    public SensorsModel(ISensor actuator, IGeneralVariables generalVariables)
    {
        _sensor = actuator;
        _generalVariables = generalVariables;
    }
    public void OnGet()
    {
        if (!User.Identity.IsAuthenticated)
        {
            Response.Redirect("/Auth/Login");
            return;
        }
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        List<(Entities.Sensor sensor, Entities.Variable Variable)> Output = new();
        var sensors = _sensor.GetSensorByUserId(userId).Select(s => new { Sensor = s, Variable = _generalVariables.GetVaraible(s.AssignedVariableId) }).ToList();
        foreach (var sensor in sensors)
        {
            Output.Add((sensor.Sensor, sensor.Variable));
        }
        ViewData["Sensors"] = Output;
    }
}
