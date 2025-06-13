using Microsoft.AspNetCore.Mvc;
using Services.Events;
using Services.Web.Auth;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = "Events")]
public class Events : ControllerBase
{
    private readonly IEvents _eventsService;

    public Events(IEvents eventsService)
    {
        _eventsService = eventsService;
    }

    [HttpPost("{eventId}/execute")]
    public IActionResult Execute(int eventId)
    {
        _eventsService.TriggerEvent(eventId);
        return Ok();
    }
}
