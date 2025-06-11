using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services.Administrate;
using Services.Variables;
using System.Security.Claims;

namespace Web.Pages.Devices;

public class VariablesModel : PageModel
{
    private readonly IGeneralVariables _generalVariables;
    public VariablesModel(IGeneralVariables generalVariables)
    {
        _generalVariables = generalVariables;
    }
    public void OnGet()
    {
        if (!User.Identity.IsAuthenticated)
        {
            Response.Redirect("/Auth/Login");
            return;
        }
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var Variables = _generalVariables.GetVariableByUser(userId);
        ViewData["Variables"] = Variables;
    }
}
