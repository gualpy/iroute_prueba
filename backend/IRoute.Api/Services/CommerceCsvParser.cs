using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using IRoute.Api.Models;

namespace IRoute.Api.Services;

public interface ICommerceCsvParser
{
    Task<List<Commerce>> ParseAsync(Stream csv, CancellationToken ct = default);
}

/// <summary>Convierte el CSV (encabezados pc_*) en registros de dominio.</summary>
public class CommerceCsvParser : ICommerceCsvParser
{
    private sealed class Map : ClassMap<Commerce>
    {
        public Map()
        {
            Map(m => m.PcNumdoc).Name("pc_numdoc");
            Map(m => m.PcTipodoc).Name("pc_tipodoc");
            Map(m => m.PcNomcomred).Name("pc_nomcomred");
            Map(m => m.PcRazonsocial).Name("pc_razonsocial");
            Map(m => m.PcDireccion).Name("pc_direccion");
            Map(m => m.PcTelefono).Name("pc_telefono");
            Map(m => m.PcEmail).Name("pc_email");
            Map(m => m.PcProcessdate).Name("pc_processdate")
                .TypeConverterOption.Format("yyyy-MM-dd");
        }
    }

    public async Task<List<Commerce>> ParseAsync(Stream csv, CancellationToken ct = default)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            TrimOptions = TrimOptions.Trim,
            MissingFieldFound = null,
            BadDataFound = null
        };
        using var reader = new StreamReader(csv);
        using var parser = new CsvReader(reader, config);
        parser.Context.RegisterClassMap<Map>();

        var rows = new List<Commerce>();
        await foreach (var row in parser.GetRecordsAsync<Commerce>(ct))
            rows.Add(row);
        return rows;
    }
}
