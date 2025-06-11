using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly Services.Web.Cookies.IAuthCookiesManager _authCookiesManager;

    public IndexModel(ILogger<IndexModel> logger, Services.Web.Cookies.IAuthCookiesManager authCookiesManager)
    {
        _logger = logger;
        _authCookiesManager = authCookiesManager;
    }

    public void OnGet()
    {

    }
    private IActionResult OnPostLogOut()
    {
        _authCookiesManager.RemoveLoginCookie();
        return Page();
    }
}
