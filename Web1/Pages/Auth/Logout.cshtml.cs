using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services.Web.Cookies;

namespace Web.Pages.Auth;

public class LogoutModel : PageModel
{
    private readonly IAuthCookiesManager _authCookiesManager;
    public LogoutModel(IAuthCookiesManager authCookiesManager)
    {
        _authCookiesManager = authCookiesManager;
    }
    public IActionResult OnGet()
    {
        if (!User.Identity.IsAuthenticated) return RedirectToPage("/Index");
        _authCookiesManager.RemoveLoginCookie();
        return RedirectToPage("/Index");
    }
}
