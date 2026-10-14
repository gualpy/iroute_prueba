namespace IRoute.Api.Models;

/// <summary>Registro de comercio (misma estructura que el CSV y la tabla commerce).</summary>
public class Commerce
{
    public string? PcNumdoc { get; set; }
    public string? PcTipodoc { get; set; }
    public string? PcNomcomred { get; set; }
    public string? PcRazonsocial { get; set; }
    public string? PcDireccion { get; set; }
    public string? PcTelefono { get; set; }
    public string? PcEmail { get; set; }
    public DateTime PcProcessdate { get; set; }
}

/// <summary>Registro en cuarentena, incluye el motivo del rechazo.</summary>
public class CommerceQuarantine : Commerce
{
    public int Id { get; set; }
    public string Motivo { get; set; } = string.Empty;
}

public record ProcessRequest(DateTime ProcessDate);
