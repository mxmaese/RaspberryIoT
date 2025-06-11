using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Api.Controllers.Administrate;

[Route("api/AdministrateVariables")]
[ApiController]
public class AdministrateVariables : ControllerBase
{
    private readonly Services.Administrate.IVariable _variable;
    public AdministrateVariables(Services.Administrate.IVariable variable)
    {
        _variable = variable;
    }
    [HttpGet("{Id:int}")]                    // /api/AdministrateVariables/1
    public Entities.Variable GetVariable(int Id)
    {
        return _variable.GetVariable(Id).FirstOrDefault() ?? new();
    }
    [HttpPost]
    public void CreateVariable(Entities.Variable variable)
    {
        _variable.CreateVariable(variable);
    }
    [HttpPut]
    public void UpdateVariable(Entities.Variable variable)
    {
        _variable.UpdateVariable(variable);
    }
    [HttpDelete]
    public void DeleteVariable(int VariableId)
    {
        _variable.DeleteVariable(VariableId);
    }

    /*
    // GET: api/<AdministrateVariables>
    [HttpGet]
    public IEnumerable<string> Get()
    {
        return new string[] { "value1", "value2" };
    }

    // GET api/<AdministrateVariables>/5
    [HttpGet("{id}")]
    public string Get(int id)
    {
        return "value";
    }

    // POST api/<AdministrateVariables>
    [HttpPost]
    public void Post([FromBody] string value)
    {
    }

    // PUT api/<AdministrateVariables>/5
    [HttpPut("{id}")]
    public void Put(int id, [FromBody] string value)
    {
    }

    // DELETE api/<AdministrateVariables>/5
    [HttpDelete("{id}")]
    public void Delete(int id)
    {
    }*/
}
