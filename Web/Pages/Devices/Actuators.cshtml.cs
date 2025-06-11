using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Services.Administrate;
using Services.Variables;
using System.Security.Claims;

namespace Web.Pages.Devices;

public class ActuatorsModel : PageModel
{
    private readonly IActuator _actuator;
    private readonly IGeneralVariables _generalVariables;
    public ActuatorsModel(IActuator actuator, IGeneralVariables generalVariables)
    {
        _actuator = actuator;
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
        List<(Entities.Actuator Actuators, Entities.Variable Variable)> Output = new();
        var actuators = _actuator.GetActuatorByUserId(userId).Select(a => new { Actuator = a, Variable = _generalVariables.GetVaraible(a.AssignedVariableId)}).ToList();
        foreach (var actuator in actuators)
        {
            Output.Add((actuator.Actuator, actuator.Variable));
        }
        ViewData["Actuators"] = Output;
    }
}
