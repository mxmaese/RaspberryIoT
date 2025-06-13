using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Services.Administrate;
using Services.Events;
using Services.Web.Cookies;

namespace Web.Pages.Devices.Add;

public class EventsAddModel : PageModel
{
    private readonly IEvent _event;
    private readonly IAuthCookiesManager _authCookiesManager;

    public EventsAddModel(IEvent events, IAuthCookiesManager authCookiesManager)
    {
        _event = events;
        _authCookiesManager = authCookiesManager;
    }
    
    [BindProperty]
    public Entities.Event Event { get; set; }
    public IEnumerable<SelectListItem> EventTriggerTypeSelectList { get; private set; }

    public void OnGet()
    {
        Event = new Entities.Event();

        EventTriggerTypeSelectList = Enum.GetValues<Entities.Event.EventTriggerType>()
                                        .Select(v => new SelectListItem
                                        {
                                            Value = ((int)v).ToString(),
                                            Text = v.ToString(),
                                            Selected = Event?.TriggerType == v
                                        }).ToList();
    }

    public async Task<IActionResult> OnPost()
    {
        Event.Interval ??= null;
        Event.DailyTime ??= null;

        Event.OwnerId = int.Parse(_authCookiesManager.GetUserIdByCookie());
        Event.LastExecution = DateTime.Now;
        Event.CreatedAt = DateTime.UtcNow;
        _event.CreateEvent(Event);
        return RedirectToPage("/Devices/Events");
    }
}
