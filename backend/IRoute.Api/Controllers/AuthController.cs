using IRoute.Api.Auth;
using IRoute.Api.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace IRoute.Api.Controllers;

public record LoginRequest(string Username, string Password);

[ApiController]
[Route("api/[controller]")]
public class AuthController(IUserRepository users, ITokenService tokens) : ControllerBase
{
    /// <summary>Valida credenciales y retorna un JWT.</summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var hash = await users.GetPasswordHashAsync(request.Username, ct);
        if (hash is null || !PasswordHasher.Verify(request.Password, hash))
            return Unauthorized(new { message = "Usuario o contraseña incorrectos." });

        var (token, expiresAt) = tokens.Create(request.Username);
        return Ok(new { token, expiresAt, username = request.Username });
    }
}
