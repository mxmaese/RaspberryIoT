using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services.Administrate;
using Services.Web.Cookies;

namespace Web.Pages.Devices.Add;

public class ActuatorsAddModel : PageModel
{
    private readonly IActuator _actuator;
    private readonly IAuthCookiesManager _authCookiesManager;

    public ActuatorsAddModel(IActuator actuator, IAuthCookiesManager authCookiesManager)
    {
        _actuator = actuator;
        _authCookiesManager = authCookiesManager;
    }
    
    [BindProperty]
    public Entities.Actuator Actuator { get; set; }

    public void OnGet()
    {
        Actuator = new Entities.Actuator();
    }

    public async Task<IActionResult> OnPost()
    {
        Actuator.OwnerId = int.Parse(_authCookiesManager.GetUserIdByCookie());
        _actuator.CreateActuator(Actuator);
        return RedirectToPage("/Devices/Actuators");
    }
}
