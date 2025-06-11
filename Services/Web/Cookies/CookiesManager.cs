using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Web.Cookies;

public class CookiesManager : ICookiesManager
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CookiesManager(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void SetCookie(string key, string value)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            httpContext.Response.Cookies.Append(key, value);
        }
    }

    public string? GetCookie(string key)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null && httpContext.Request.Cookies.ContainsKey(key))
        {
            return httpContext.Request.Cookies[key];
        }
        return null; // Retorna null si la cookie no existe
    }

    public void RemoveCookie(string key)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            httpContext.Response.Cookies.Delete(key);
        }
    }
}
public interface ICookiesManager
{
    void SetCookie(string key, string value);
    string? GetCookie(string key);
    void RemoveCookie(string key);
}