using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Services.SensorsAndActuators;
using Services.Variables;

namespace Web.Pages.Devices.Edit;

public class VariablesEditModel : PageModel
{
    private readonly IGeneralVariables _variables;

    public VariablesEditModel(IGeneralVariables actuator)
    {
        _variables = actuator;
    }

    [BindProperty]
    public Entities.Variable Variable { get; set; }


    // SelectList que se usará en la vista
    public IEnumerable<SelectListItem> VariableTypeSelectList { get; private set; }

    public void OnGet(int variableId)
    {
        LoadVariable(variableId);
        BuildVariableTypeSelectList();
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        var response = await _variables.EditVariable(Variable);
        if (response != default)
        {
            foreach (var item in response)
            {
                ModelState.AddModelError(item.Error.ToString(), item.message);
            }

            Variable = _variables.GetVaraible(Variable.VariableId) ?? new Entities.Variable();

            Variable = Variable ?? new Entities.Variable();

            BuildVariableTypeSelectList();
            return Page();
        }
        return RedirectToPage("/Devices/Variable");
    }

    /* ------------------ helpers privados ------------------- */

    private void LoadVariable(int id)
    {
        Variable = _variables.GetVaraible(id) ?? new();
    }

    private void BuildVariableTypeSelectList()
    {
        VariableTypeSelectList = Enum.GetValues<Entities.Variable.VariableType>()
                                     .Select(v => new SelectListItem
                                     {
                                         Value = ((int)v).ToString(),
                                         Text = v.ToString(),          // podés traducir el texto si querés
                                         Selected = Variable?.Type == v
                                     })
                                     .ToList();
    }
}
