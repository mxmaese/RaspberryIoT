using Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Services.Events;
using Services.SensorsAndActuators;
using Services.Variables;

namespace Web.Pages.Devices.Edit;

public class EventsEditModel : PageModel
{
    private readonly IEvents _event;

    public EventsEditModel(IEvents eventAdministrate)
    {
        _event = eventAdministrate;
    }

    [BindProperty]
    public Event Event { get; set; }
    public IEnumerable<SelectListItem> EventTriggerTypeSelectList { get; private set; }

    public void OnGet(int EventId)
    {
        Event = _event.GetEvent(EventId) ?? new Event();

        BuildEventTriggerTypeSelectList();
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        var response = await _event.EditVariable(Event);
        if (response != default)
        {
            foreach (var item in response)
            {
                ModelState.AddModelError(item.Error.ToString(), item.message);
            }

            Event = _event.GetEvent(Event.EventId) ?? new Event();

            Event = Event ?? new Event();

            BuildEventTriggerTypeSelectList();
            return Page();
        }
        return RedirectToPage("/Devices/Events");
    }

    private void BuildEventTriggerTypeSelectList()
    {
        EventTriggerTypeSelectList = Enum.GetValues<Entities.Event.EventTriggerType>()
                                        .Select(v => new SelectListItem
                                        {
                                            Value = ((int)v).ToString(),
                                            Text = v.ToString(),
                                            Selected = Event?.TriggerType == v
                                        }).ToList();
    }
}
