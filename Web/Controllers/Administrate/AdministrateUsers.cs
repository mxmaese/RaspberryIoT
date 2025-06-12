using Microsoft.AspNetCore.Mvc;
using Services.Web.Auth;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Api.Controllers.Administrate;

[Route("api/[controller]")]
[ApiController]
[ApiTokenAuthentication]
public class AdministrateUsers : ControllerBase
{
    private readonly Services.Administrate.IUser _user;
    public AdministrateUsers(Services.Administrate.IUser user)
    {
        _user = user;
    }
    [HttpGet]
    public Entities.User GetUser(int Id)
    {
        return _user.GetUser(Id);
    }
    [HttpPost]
    public void CreateUser(Entities.User user)
    {
        _user.CreateUser(user);
    }
    [HttpPut]
    public void UpdateUser(Entities.User user)
    {
        _user.UpdateUser(user);
    }
    [HttpDelete]
    public void DeleteUser(int UserId)
    {
        _user.DeleteUser(UserId);
    }
    /*
    // GET: api/<AdministrateUsers>
    [HttpGet]
    public IEnumerable<string> Get()
    {
        return new string[] { "value1", "value2" };
    }

    // GET api/<AdministrateUsers>/5
    [HttpGet("{id}")]
    public string Get(int id)
    {
        return "value";
    }

    // POST api/<AdministrateUsers>
    [HttpPost]
    public void Post([FromBody] string value)
    {
    }

    // PUT api/<AdministrateUsers>/5
    [HttpPut("{id}")]
    public void Put(int id, [FromBody] string value)
    {
    }

    // DELETE api/<AdministrateUsers>/5
    [HttpDelete("{id}")]
    public void Delete(int id)
    {
    }*/
}
