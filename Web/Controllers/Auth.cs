using Microsoft.AspNetCore.Mvc;
using Services.Users;
using Services.Web.Auth;
using System.Security.Claims;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class Auth : ControllerBase
{
    private readonly IUsers _users;
    private readonly IJwtTokenBuilder _tokenBuilder;

    public Auth(IUsers users, IJwtTokenBuilder tokenBuilder)
    {
        _users = users;
        _tokenBuilder = tokenBuilder;
    }

    public class LoginDto
    {
        public string Username { get; set; }
        public string ApiToken { get; set; }
        public string Audience { get; set; }
        public int? Minutes { get; set; }
    }
    private const int MaxTokenMinutes = 10080;

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginDto dto)
    {
        var user = _users.ValidateUserApiCredentials(dto.Username, dto.ApiToken);
        if (user == null)
        {
            return Unauthorized();
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.UserName)
        };

        var minutes = (int?)(dto.Minutes.HasValue ? int.Min(MaxTokenMinutes, dto.Minutes.Value) : null);
        var token = string.Empty;
        if (minutes.HasValue && minutes > 0)
        {
            token = _tokenBuilder.BuildJwt(claims, dto.Audience, minutes.Value);
        }
        else
        {
            token = _tokenBuilder.BuildJwt(claims, dto.Audience);
        }
        return Ok(token);
    }
    [HttpPost("loginquery")]
    public IActionResult LoginQuery([FromQuery] LoginDto dto)
    {
        var user = _users.ValidateUserApiCredentials(dto.Username, dto.ApiToken);
        if (user == null)
        {
            return Unauthorized();
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.UserName)
        };

        var minutes = (int?)(dto.Minutes.HasValue ? int.Min(MaxTokenMinutes, dto.Minutes.Value) : null);
        var token = string.Empty;
        if (minutes.HasValue && minutes > 0)
        {
            token = _tokenBuilder.BuildJwt(claims, dto.Audience, minutes.Value);
        }
        else
        {
            token = _tokenBuilder.BuildJwt(claims, dto.Audience);
        }
        return Ok(token);
    }
}
