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


[Route("api/[controller]")]
[ApiController]
public class EventsToken : ControllerBase
{
    private readonly IEvents _eventsService;
    private readonly ILogger<EventsToken> _logger;

    public EventsToken(IEvents eventsService, ILogger<EventsToken> logger)
    {
        _eventsService = eventsService;
        _logger = logger;
    }

    [HttpPost("{eventId}/execute")]
    public IActionResult Execute(int eventId, [FromBody] string token)
    {
        var Owner = _eventsService.GetOwner(eventId);
        if (Owner.ApiToken == token)
        {
            _eventsService.TriggerEvent(eventId);
        }
        else
        {
           _logger.LogWarning($"Failed to execute event {eventId} with token {token}");
            return Unauthorized("Invalid token");
        }
        return Ok();
    }
}
