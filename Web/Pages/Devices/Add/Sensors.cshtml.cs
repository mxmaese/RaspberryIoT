using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services.Administrate;
using Services.Web.Cookies;

namespace Web.Pages.Devices.Add;

public class SensorsAddModel : PageModel
{
    private readonly ISensor _sensor;
    private readonly IAuthCookiesManager _authCookiesManager;

    public SensorsAddModel(ISensor sensor, IAuthCookiesManager authCookiesManager)
    {
        _sensor = sensor;
        _authCookiesManager = authCookiesManager;
    }
    
    [BindProperty]
    public Entities.Sensor Sensor { get; set; }

    public void OnGet()
    {
        Sensor = new Entities.Sensor();
    }

    public async Task<IActionResult> OnPost()
    {
        Sensor.OwnerId = int.Parse(_authCookiesManager.GetUserIdByCookie());
        _sensor.CreateSensor(Sensor);
        return RedirectToPage("/Devices/Sensors");
    }
}
