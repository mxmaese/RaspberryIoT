using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using Services.GeneralFunctions;

namespace Web.Pages.Auth;

public class RegisterModel : PageModel
{

    [BindProperty]
    public Services.Users.IUsers.RegisterFormModel RegisterForm { get; set; }


    private readonly ILogger<RegisterModel> _logger;
    private readonly Services.Users.IUsers _users;
    public RegisterModel(ILogger<RegisterModel> logger, Services.Users.IUsers users)
    {
        _logger = logger;
        _users = users;
    }


    //how can i get the submitted data from the form?
    public async Task<IActionResult> OnPost()
    {
        var Response = await _users.CreateUser(RegisterForm);
        if(Response.IsNullOrEmpty())
        {
            return RedirectToPage("/Index");
        }
        else
        {
            ModelState.Clear();
            foreach (var item in Response)
            {
                if (item.Error == Services.Users.Users.RegisterErrorMessages.NotInsertedUserName || item.Error == Services.Users.Users.RegisterErrorMessages.UserAlreadyExists)
                {
                    ModelState.AddModelError("RegisterForm.RegisteUsername", item.message); // hacer ifs para todo el resto de campos
                }else if (item.Error == Services.Users.Users.RegisterErrorMessages.NotInsertedEmail || item.Error == Services.Users.Users.RegisterErrorMessages.EmailAlreadyExists)
                {
                    ModelState.AddModelError("RegisterForm.RegisterEmail", item.message); // hacer ifs para todo el resto de campos
                }else if (item.Error == Services.Users.Users.RegisterErrorMessages.NotInsertedPassword)
                {
                    ModelState.AddModelError("RegisterForm.RegisterPassword", item.message); // hacer ifs para todo el resto de campos
                }
                else if (item.Error == Services.Users.Users.RegisterErrorMessages.NotInsertedPasswordConfirm || item.Error == Services.Users.Users.RegisterErrorMessages.PasswordsDoNotMatch)
                {
                    ModelState.AddModelError("RegisterForm.RegisterPasswordConfirm", item.message); // hacer ifs para todo el resto de campos
                }
                else
                {
                    ModelState.AddModelError(item.Error.ToString(), item.message);
                }
            }
            return Page();
        }

    }



    public void OnGet()
    {
        if (User.Identity.IsAuthenticated)
        {
            Response.Redirect("/Index");
        }
    }
}
