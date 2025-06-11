using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Services.Web.Cookies;

public class AuthCookiesManager : IAuthCookiesManager
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    public AuthCookiesManager(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public bool IsUserLoggedIn()
    {
        var user = _httpContextAccessor.HttpContext.User;
        return user.Identity != null && user.Identity.IsAuthenticated;
    }

    public async Task SetLoginCookie(string UserName, int UserId, bool IsPersistent, DateTime? ExpiresUtc = null)
    {
        // Crear una cookie de autenticación segura
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, UserName),
            new Claim(ClaimTypes.NameIdentifier, UserId.ToString())
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await _httpContextAccessor.HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            new AuthenticationProperties
            {
                IsPersistent = IsPersistent,
                ExpiresUtc = ExpiresUtc == null ? DateTime.UtcNow.AddMinutes(30) : ExpiresUtc // Ajusta según lo necesites
            });
    }

    public string? GetUserIdByCookie()
    {
        var user = _httpContextAccessor.HttpContext.User;
        if (user.Identity != null && user.Identity.IsAuthenticated)
        {
            return user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
        return null;
    }

    public async Task RemoveLoginCookie()
    {
        await _httpContextAccessor.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }
}

public interface IAuthCookiesManager
{
    bool IsUserLoggedIn();
    public string? GetUserIdByCookie();
    Task SetLoginCookie(string UserName, int UserId, bool IsPersistent, DateTime? ExpiresUtc = null);
    Task RemoveLoginCookie();
}
