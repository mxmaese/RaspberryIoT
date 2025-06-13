using Microsoft.AspNetCore.Mvc;
using Services.Web.Auth;
using Microsoft.AspNetCore.Authorization;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Api.Controllers.Administrate;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = "Administrate")]
public class AdministrateLocations : ControllerBase
{
    private readonly Services.Administrate.ILocation _location;
    public AdministrateLocations(Services.Administrate.ILocation location)
    {
        _location = location;
    }

    [HttpGet]
    public Entities.Location GetLocation(int Id)
    {
        return _location.GetLocation(Id);
    }
    [HttpPost]
    public void CreateLocation(Entities.Location location)
    {
        _location.CreateLocation(location);
    }
    [HttpPut]
    public void UpdateLocation(Entities.Location location)
    {
        _location.UpdateLocation(location);
    }
    [HttpDelete]
    public void DeleteLocation(int LocationId)
    {
        _location.DeleteLocation(LocationId);
    }

    /*
    // GET: api/<AdministrateLocations>
    [HttpGet]
    public IEnumerable<string> Get()
    {
        return new string[] { "value1", "value2" };
    }

    // GET api/<AdministrateLocations>/5
    [HttpGet("{id}")]
    public string Get(int id)
    {
        return "value";
    }

    // POST api/<AdministrateLocations>
    [HttpPost]
    public void Post([FromBody] string value)
    {
    }

    // PUT api/<AdministrateLocations>/5
    [HttpPut("{id}")]
    public void Put(int id, [FromBody] string value)
    {
    }

    // DELETE api/<AdministrateLocations>/5
    [HttpDelete("{id}")]
    public void Delete(int id)
    {
    }*/
}
