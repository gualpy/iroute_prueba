using System.Text;
using IRoute.Api.Auth;
using IRoute.Api.Repositories;
using IRoute.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<ICommerceRepository, CommerceRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddSingleton<ICommerceCsvParser, CommerceCsvParser>();
builder.Services.AddSingleton<ITokenService, TokenService>();
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o => o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    });
builder.Services.AddAuthorization();

var app = builder.Build();

// Usuario inicial de desarrollo (se crea solo si no existe)
try
{
    using var scope = app.Services.CreateScope();
    var users = scope.ServiceProvider.GetRequiredService<IUserRepository>();
    await users.EnsureUserAsync("admin", PasswordHasher.Hash("admin123"));
}
catch (Exception ex)
{
    app.Logger.LogWarning(ex, "No se pudo crear el usuario inicial. ¿Ejecutó database/02_auth.sql?");
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
