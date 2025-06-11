using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services.SensorsAndActuators;

namespace Web.Pages.Devices.Edit;

public class ActuatorsModel : PageModel
{
    private readonly Services.SensorsAndActuators.IActuators _actuator;
    public ActuatorsModel(Services.SensorsAndActuators.IActuators actuator)
    {
        _actuator = actuator;
    }

    [BindProperty]
    public IActuators.ActuatorFormModel ActuatorFormModel { get; set; }
    [BindProperty]
    public Entities.Actuator Actuator { get; set; }


    public void OnGet(int ActuatorId)
    {
        var actuator = _actuator.GetActuator(ActuatorId);

        Actuator = actuator.FirstOrDefault() ?? new Entities.Actuator();
        ActuatorFormModel = IActuators.ActuatorFormModel.GetActuatorFormModel(Actuator);
    }

    public async Task<IActionResult> OnPost()
    {
        var actuator = Actuator;
        var response = await _actuator.EditActuator(ActuatorFormModel, actuator);
        if (response != default)
        {
            foreach (var item in response)
            {
                ModelState.AddModelError(item.Error.ToString(), item.message);
            }

            actuator = _actuator.GetActuator(actuator.ActuatorId).FirstOrDefault();

            Actuator = actuator ?? new Entities.Actuator();
            return Page();
        }
        return RedirectToPage("/Devices/Actuators");
    }
}
