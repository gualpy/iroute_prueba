using System.Data;
using Microsoft.Data.SqlClient;

namespace IRoute.Api.Repositories;

public interface IUserRepository
{
    Task<string?> GetPasswordHashAsync(string username, CancellationToken ct = default);
    Task EnsureUserAsync(string username, string passwordHash, CancellationToken ct = default);
}

public class UserRepository(IConfiguration config) : IUserRepository
{
    private readonly string _cs = config.GetConnectionString("Default")
        ?? throw new InvalidOperationException("Falta ConnectionStrings:Default");

    public async Task<string?> GetPasswordHashAsync(string username, CancellationToken ct = default)
    {
        await using var cn = new SqlConnection(_cs);
        await using var cmd = new SqlCommand("dbo.sp_get_user_hash", cn)
        { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.Add("@username", SqlDbType.VarChar, 50).Value = username;
        await cn.OpenAsync(ct);
        return await cmd.ExecuteScalarAsync(ct) as string;
    }

    public async Task EnsureUserAsync(string username, string passwordHash, CancellationToken ct = default)
    {
        await using var cn = new SqlConnection(_cs);
        await using var cmd = new SqlCommand("dbo.sp_ensure_user", cn)
        { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.Add("@username", SqlDbType.VarChar, 50).Value = username;
        cmd.Parameters.Add("@password_hash", SqlDbType.VarChar, 200).Value = passwordHash;
        await cn.OpenAsync(ct);
        await cmd.ExecuteNonQueryAsync(ct);
    }
}
