using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace IRoute.Api.Auth;

public interface ITokenService
{
    (string Token, DateTime ExpiresAt) Create(string username);
}

public class TokenService(IConfiguration config) : ITokenService
{
    public (string Token, DateTime ExpiresAt) Create(string username)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var expires = DateTime.UtcNow.AddMinutes(config.GetValue("Jwt:ExpireMinutes", 60));
        var jwt = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Issuer"],
            claims: [new Claim(ClaimTypes.Name, username)],
            expires: expires,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        return (new JwtSecurityTokenHandler().WriteToken(jwt), expires);
    }
}
