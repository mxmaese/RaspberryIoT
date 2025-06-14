using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Services.Web.Auth;

public interface IJwtTokenBuilder
{
    string BuildJwt(IEnumerable<Claim> claims, string audience, int minutes = 30);
}

public class JwtTokenBuilder : IJwtTokenBuilder
{
    private readonly IConfiguration _config;

    public JwtTokenBuilder(IConfiguration config)
    {
        _config = config;
    }

    public string BuildJwt(IEnumerable<Claim> claims, string audience, int minutes = 30)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(minutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
