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
        public string Password { get; set; }
        public string Audience { get; set; }
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginDto dto)
    {
        var user = _users.ValidateUserCredentials(dto.Username, dto.Password);
        if (user == null)
        {
            return Unauthorized();
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.UserName)
        };

        var token = _tokenBuilder.BuildJwt(claims, dto.Audience);
        return Ok(new { token });
    }
}
