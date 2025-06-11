using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services.SensorsAndActuators;

namespace Web.Pages.Devices.Edit;

public class ActuatorsEditModel : PageModel
{
    private readonly Services.SensorsAndActuators.IActuators _actuator;
    public ActuatorsEditModel(Services.SensorsAndActuators.IActuators actuator)
    {
        _actuator = actuator;
    }
    [BindProperty]
    public Entities.Actuator Actuator { get; set; }


    public void OnGet(int ActuatorId)
    {
        var actuator = _actuator.GetActuator(ActuatorId).FirstOrDefault();

        Actuator = actuator ?? new Entities.Actuator();
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        var response = await _actuator.EditActuator(Actuator);
        if (response != default)
        {
            foreach (var item in response)
            {
                ModelState.AddModelError(item.Error.ToString(), item.message);
            }

            Actuator = _actuator.GetActuator(Actuator.ActuatorId).FirstOrDefault() ?? new Entities.Actuator();

            Actuator = Actuator ?? new Entities.Actuator();
            return Page();
        }
        return RedirectToPage("/Devices/Actuators");
    }

    public async Task<IActionResult> OnPostRegenerateAsync()
    {
        var actuator = _actuator.GetActuator(Actuator.ActuatorId).FirstOrDefault();
        if (actuator is null) return NotFound();

        actuator.Token = _actuator.UpdateActuatorToken(actuator.ActuatorId);

        LoadSensor(actuator.ActuatorId);
        TempData["Msg"] = "Token regenerado correctamente ✔️";
        return Page();
    }
    private void LoadSensor(int id)
    {
        var s = _actuator.GetActuator(id).FirstOrDefault() ?? new();
        Actuator = s;
    }
}
