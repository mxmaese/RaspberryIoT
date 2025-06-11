using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services.SensorsAndActuators;

namespace Web.Pages.Devices.Edit;

public class SensorsEditModel : PageModel
{
    private readonly ISensors _sensors;

    public SensorsEditModel(ISensors actuator)
    {
        _sensors = actuator;
    }
    [BindProperty]
    public Entities.Sensor Sensor { get; set; }


    public void OnGet(int SensorId)
    {
        var sensor = _sensors.GetSensor(SensorId);

        Sensor = sensor ?? new Entities.Sensor();
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        var response = await _sensors.EditSensor(Sensor);
        if (response != default)
        {
            foreach (var item in response)
            {
                ModelState.AddModelError(item.Error.ToString(), item.message);
            }

            Sensor = _sensors.GetSensor(Sensor.SensorId) ?? new Entities.Sensor();

            Sensor = Sensor ?? new Entities.Sensor();
            return Page();
        }
        return RedirectToPage("/Devices/Sensors");
    }

    public async Task<IActionResult> OnPostRegenerateAsync()
    {
        var sensor = _sensors.GetSensor(Sensor.SensorId);
        if (sensor is null) return NotFound();

        sensor.Token = _sensors.UpdateSensorToken(sensor.SensorId);

        LoadSensor(sensor.SensorId);
        TempData["Msg"] = "Token regenerado correctamente ✔️";
        return Page();
    }
    private void LoadSensor(int id)
    {
        var s = _sensors.GetSensor(id) ?? new();
        Sensor = s;
    }
}
