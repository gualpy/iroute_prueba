using System.Text.RegularExpressions;
using IRoute.Api.Models;
using IRoute.Api.Repositories;
using IRoute.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IRoute.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public partial class CommerceController(
    ICommerceRepository repo, ICommerceCsvParser parser) : ControllerBase
{
    [GeneratedRegex(@"^commerce_\d{8}\.csv$", RegexOptions.IgnoreCase)]
    private static partial Regex FileNameRegex();

    /// <summary>Carga el archivo commerce_DDMMYYYY.csv y lo registra vía sp_create_commerce.</summary>
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile? file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "El archivo está vacío." });
        if (!FileNameRegex().IsMatch(file.FileName))
            return BadRequest(new { message = "El nombre debe ser commerce_DDMMYYYY.csv." });

        List<Commerce> rows;
        try
        {
            await using var stream = file.OpenReadStream();
            rows = await parser.ParseAsync(stream, ct);
        }
        catch (Exception ex) when (ex is CsvHelper.CsvHelperException or FormatException)
        {
            return BadRequest(new { message = "CSV con formato inválido: " + ex.Message });
        }

        if (rows.Count == 0)
            return BadRequest(new { message = "El archivo no contiene registros." });

        var inserted = await repo.CreateAsync(rows, ct);
        return Ok(new { inserted });
    }

    /// <summary>Valida los registros de la fecha y mueve los inválidos a cuarentena.</summary>
    [HttpPost("process")]
    public async Task<IActionResult> Process([FromBody] ProcessRequest request, CancellationToken ct)
    {
        var quarantined = await repo.ProcessAsync(request.ProcessDate, ct);
        return Ok(new { quarantined });
    }

    /// <summary>Lista los comercios en cuarentena.</summary>
    [HttpGet("quarantine")]
    public async Task<ActionResult<List<CommerceQuarantine>>> Quarantine(CancellationToken ct)
        => Ok(await repo.GetQuarantineAsync(ct));
}
