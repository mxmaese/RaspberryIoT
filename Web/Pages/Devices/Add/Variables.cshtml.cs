using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Services.Administrate;
using Services.Web.Cookies;

namespace Web.Pages.Devices.Add;

public class VariablesAddModel : PageModel
{
    private readonly IVariable _variable;
    private readonly IAuthCookiesManager _authCookiesManager;

    public VariablesAddModel(IVariable variable, IAuthCookiesManager authCookiesManager)
    {
        _variable = variable;
        _authCookiesManager = authCookiesManager;
    }
    
    [BindProperty]
    public Entities.Variable Variable { get; set; }
    public IEnumerable<SelectListItem> VariableTypeSelectList { get; private set; }

    public void OnGet()
    {
        Variable = new Entities.Variable();

        VariableTypeSelectList = Enum.GetValues<Entities.Variable.VariableType>()
                                     .Select(v => new SelectListItem
                                     {
                                         Value = ((int)v).ToString(),
                                         Text = v.ToString(),          // podés traducir el texto si querés
                                         Selected = Variable?.Type == v
                                     })
                                     .ToList();
    }

    public async Task<IActionResult> OnPost()
    {
        Variable.OwnerId = int.Parse(_authCookiesManager.GetUserIdByCookie());
        _variable.CreateVariable(Variable);
        return RedirectToPage("/Devices/Events");
    }
}
