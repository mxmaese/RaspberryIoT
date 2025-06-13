using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services.Administrate;
using Services.Events;
using Services.Variables;
using System.Security.Claims;

namespace Web.Pages.Devices;

public class EventsModel : PageModel
{
    private readonly IEvents _events;
    public EventsModel(IEvents events)
    {
        _events = events;
    }
    public void OnGet()
    {
        if (!User.Identity.IsAuthenticated)
        {
            Response.Redirect("/Auth/Login");
            return;
        }
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var Events = _events.GetEventsByUser(userId);
        ViewData["Event"] = Events;
    }
}
