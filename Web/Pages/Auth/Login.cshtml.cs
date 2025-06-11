using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages.Auth;

public class LoginModel : PageModel
{
    private readonly ILogger<LoginModel> _logger;
    private readonly Services.Users.IUsers _users;
    public LoginModel(ILogger<LoginModel> logger, Services.Users.IUsers users)
    {
        _logger = logger;
        _users = users;
    }

    [BindProperty]
    public Services.Users.IUsers.LoginFormModel LoginForm { get; set; }

    public void OnGet()
    {
        if (User.Identity.IsAuthenticated)
        {
            Response.Redirect("/Index");
        }
        else
        {
            LoginForm = new Services.Users.IUsers.LoginFormModel();
        }
    }

    public async Task<IActionResult> OnPost()
    {
        try
        {
            var Response = await _users.LoginUser(LoginForm);
            if (Response == default)
            {
                return RedirectToPage("/Index");
            }
            else
            {
                ModelState.Clear();
                foreach (var item in Response)
                {
                    if (item.Error == Services.Users.Users.LoginErrorMessages.NotInsertedUserName || item.Error == Services.Users.Users.LoginErrorMessages.UserDoesNotExist)
                    {
                        ModelState.AddModelError("LoginForm.LoginUsername", item.message);
                    }
                    else if (item.Error == Services.Users.Users.LoginErrorMessages.NotInsertedPassword || item.Error == Services.Users.Users.LoginErrorMessages.PasswordIsIncorrect)
                    {
                        ModelState.AddModelError("LoginForm.LoginPassword", item.message);
                    }
                    else
                    {
                        ModelState.AddModelError(item.Error.ToString(), item.message);
                    }
                }
                return Page();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Login");
            ModelState.AddModelError("LoginForm.LoginUsername", "An error occurred while processing your request.");
            return Page();
        }
    }
}
