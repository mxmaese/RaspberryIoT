using Microsoft.AspNetCore.Mvc;
using Services.Events;
using Services.Web.Auth;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[ApiTokenAuthentication]
public class Events : ControllerBase
{
    private readonly IEventExecutor _executor;
    private readonly Services.Databases.IDatabasesActions _db;

    public Events(IEventExecutor executor, Services.Databases.IDatabasesActions db)
    {
        _executor = executor;
        _db = db;
    }

    [HttpPost("Execute/{id:int}")]
    public IActionResult Execute(int id)
    {
        var ev = _db.GetEvents(new Entities.Event { EventId = id }).FirstOrDefault();
        if (ev == null) return NotFound();
        _executor.ExecuteEvent(ev);
        return Ok();
    }
}
