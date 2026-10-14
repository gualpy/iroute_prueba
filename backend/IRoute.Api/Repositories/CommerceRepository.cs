using System.Data;
using IRoute.Api.Models;
using Microsoft.Data.SqlClient;

namespace IRoute.Api.Repositories;

public interface ICommerceRepository
{
    Task<int> CreateAsync(IEnumerable<Commerce> rows, CancellationToken ct = default);
    Task<int> ProcessAsync(DateTime processDate, CancellationToken ct = default);
    Task<List<CommerceQuarantine>> GetQuarantineAsync(CancellationToken ct = default);
}

/// <summary>Acceso a datos vía stored procedures (ADO.NET).</summary>
public class CommerceRepository(IConfiguration config) : ICommerceRepository
{
    private readonly string _cs = config.GetConnectionString("Default")
        ?? throw new InvalidOperationException("Falta ConnectionStrings:Default");

    public async Task<int> CreateAsync(IEnumerable<Commerce> rows, CancellationToken ct = default)
    {
        // Table-Valued Parameter: una sola llamada al SP para todo el archivo.
        var table = new DataTable();
        table.Columns.Add("pc_numdoc", typeof(string));
        table.Columns.Add("pc_tipodoc", typeof(string));
        table.Columns.Add("pc_nomcomred", typeof(string));
        table.Columns.Add("pc_razonsocial", typeof(string));
        table.Columns.Add("pc_direccion", typeof(string));
        table.Columns.Add("pc_telefono", typeof(string));
        table.Columns.Add("pc_email", typeof(string));
        table.Columns.Add("pc_processdate", typeof(DateTime));
        foreach (var r in rows)
            table.Rows.Add(r.PcNumdoc, r.PcTipodoc, r.PcNomcomred, r.PcRazonsocial,
                           r.PcDireccion, r.PcTelefono, r.PcEmail, r.PcProcessdate.Date);

        await using var cn = new SqlConnection(_cs);
        await using var cmd = new SqlCommand("dbo.sp_create_commerce", cn)
        { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.Add(new SqlParameter("@rows", SqlDbType.Structured)
        { TypeName = "dbo.commerce_type", Value = table });
        await cn.OpenAsync(ct);
        return Convert.ToInt32(await cmd.ExecuteScalarAsync(ct));
    }

    public async Task<int> ProcessAsync(DateTime processDate, CancellationToken ct = default)
    {
        await using var cn = new SqlConnection(_cs);
        await using var cmd = new SqlCommand("dbo.sp_process_commerce", cn)
        { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.Add("@processdate", SqlDbType.Date).Value = processDate.Date;
        await cn.OpenAsync(ct);
        return Convert.ToInt32(await cmd.ExecuteScalarAsync(ct));
    }

    public async Task<List<CommerceQuarantine>> GetQuarantineAsync(CancellationToken ct = default)
    {
        const string sql = @"SELECT id, pc_numdoc, pc_tipodoc, pc_nomcomred, pc_razonsocial,
                                    pc_direccion, pc_telefono, pc_email, pc_processdate, motivo
                             FROM dbo.commerce_quarantine ORDER BY id";
        var list = new List<CommerceQuarantine>();
        await using var cn = new SqlConnection(_cs);
        await using var cmd = new SqlCommand(sql, cn);
        await cn.OpenAsync(ct);
        await using var rd = await cmd.ExecuteReaderAsync(ct);
        while (await rd.ReadAsync(ct))
        {
            list.Add(new CommerceQuarantine
            {
                Id = rd.GetInt32(0),
                PcNumdoc = rd.IsDBNull(1) ? null : rd.GetString(1),
                PcTipodoc = rd.IsDBNull(2) ? null : rd.GetString(2),
                PcNomcomred = rd.IsDBNull(3) ? null : rd.GetString(3),
                PcRazonsocial = rd.IsDBNull(4) ? null : rd.GetString(4),
                PcDireccion = rd.IsDBNull(5) ? null : rd.GetString(5),
                PcTelefono = rd.IsDBNull(6) ? null : rd.GetString(6),
                PcEmail = rd.IsDBNull(7) ? null : rd.GetString(7),
                PcProcessdate = rd.GetDateTime(8),
                Motivo = rd.GetString(9)
            });
        }
        return list;
    }
}
