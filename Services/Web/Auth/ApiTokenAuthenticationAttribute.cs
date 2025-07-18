using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Services.Users;

namespace Services.Web.Auth;

public class ApiTokenAuthenticationAttribute : Attribute, IAsyncActionFilter
{
    private const string HeaderName = "X-Api-Token";

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(HeaderName, out var token))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var userService = (IUsers)context.HttpContext.RequestServices.GetRequiredService<IUsers>();
        var user = userService.GetUserByApiToken(token!);
        if (user == null)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        await next();
    }
}
