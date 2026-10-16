using System.Globalization;
using System.Text;
using Backend.Clients;
using Backend.Models.DTOs.Responses;

namespace Backend.Services;

public static class ConvocaQueryBuilder
{
    public const int MaxPageSize = 50;
    public const int DefaultPageSize = 20;
    public const int MaxExportSize = 500;
    public const int MaxTextLength = 200;

    public static string BuildWhere(ProcessSearchCriteria criteria)
    {
        var clauses = new List<string>();

        if (criteria.PublishedOnly)
        {
            clauses.Add($"{ConvocaFields.PublishedOn} IS NOT NULL");
            clauses.Add($"{ConvocaFields.Status} != 'Borrador'");
        }

        AddContains(clauses, ConvocaFields.EntityName, criteria.EntityName);
        AddContains(clauses, ConvocaFields.EntityNit, criteria.EntityNit);
        AddContains(clauses, ConvocaFields.Department, criteria.Department);
        AddContains(clauses, ConvocaFields.City, criteria.City);
        AddContains(clauses, ConvocaFields.Modality, criteria.Modality);
        AddContains(clauses, ConvocaFields.Phase, criteria.Phase);
        AddContains(clauses, ConvocaFields.ContractType, criteria.ContractType);

        if (criteria.OpenForOffer)
        {
            clauses.Add($"{ConvocaFields.Status} = 'Publicado'");
            clauses.Add($"({ConvocaFields.Awarded} IS NULL OR {ConvocaFields.Awarded} = 'No')");
            clauses.Add(
                $"({ConvocaFields.SupplierName} IS NULL OR {ConvocaFields.SupplierName} = 'No Definido')");
            clauses.Add($"{ConvocaFields.OffersDueOn} IS NOT NULL");
            clauses.Add(SubscribableModalityClause());
            clauses.Add($"{ConvocaFields.Modality} = 'Mínima cuantía'");
            clauses.Add($"{ConvocaFields.ContractType} = 'Prestación de servicios'");
            if (criteria.OffersDueFrom is not null)
            {
                var dueFrom = criteria.OffersDueFrom.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                clauses.Add($"{ConvocaFields.OffersDueOn} >= '{dueFrom}T00:00:00'");
            }
        }
        else
        {
            AddContains(clauses, ConvocaFields.Status, criteria.Status);
        }

        if (criteria.PublishedFrom is not null)
        {
            var from = criteria.PublishedFrom.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            clauses.Add($"{ConvocaFields.PublishedOn} >= '{from}T00:00:00'");
        }

        if (criteria.PublishedTo is not null)
        {
            var to = criteria.PublishedTo.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            clauses.Add($"{ConvocaFields.PublishedOn} <= '{to}T23:59:59'");
        }

        if (criteria.MinBasePrice is not null)
        {
            clauses.Add(
                $"{ConvocaFields.BasePrice} >= {criteria.MinBasePrice.Value.ToString(CultureInfo.InvariantCulture)}");
        }

        if (criteria.MaxBasePrice is not null)
        {
            clauses.Add(
                $"{ConvocaFields.BasePrice} <= {criteria.MaxBasePrice.Value.ToString(CultureInfo.InvariantCulture)}");
        }

        AddProfilePhrases(clauses, criteria.ProfilePhrases);

        return string.Join(" AND ", clauses);
    }

    public static string BuildProcessIdWhere(string processId)
    {
        return $"{ConvocaFields.ProcessId} = {Literal(processId)}";
    }

    public static string Literal(string value)
    {
        return "'" + value.Replace("'", "''", StringComparison.Ordinal) + "'";
    }

    public static string ContainsLiteral(string value)
    {
        var sanitized = value.Replace("%", string.Empty, StringComparison.Ordinal)
            .Replace("_", string.Empty, StringComparison.Ordinal)
            .Trim();
        return Literal("%" + sanitized + "%");
    }

    public static string? BuildFullText(string? keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return null;

        var builder = new StringBuilder(keyword.Length);
        foreach (var character in keyword.Trim())
        {
            if (char.IsLetterOrDigit(character) || char.IsWhiteSpace(character) || character is ',' or '.' or '-')
                builder.Append(character);
        }

        var text = builder.ToString().Trim();
        return text.Length == 0 ? null : text;
    }

    private static void AddContains(List<string> clauses, string column, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;

        clauses.Add($"{column} like {ContainsLiteral(value)}");
    }

    private static string SubscribableModalityClause()
    {
        var excluded = new[]
        {
            "Contratación régimen especial",
            "Contratación directa",
            "Solicitud de información a los Proveedores"
        };

        var parts = excluded.Select(modality =>
            $"{ConvocaFields.Modality} != {Literal(modality)}");
        return string.Join(" AND ", parts);
    }

    private static void AddProfilePhrases(List<string> clauses, IReadOnlyList<string>? phrases)
    {
        if (phrases is null || phrases.Count == 0)
            return;

        var parts = new List<string>();
        foreach (var phrase in phrases.Take(4))
        {
            if (string.IsNullOrWhiteSpace(phrase))
                continue;

            var literal = ContainsLiteral(phrase);
            parts.Add(
                $"({ConvocaFields.ProcedureName} like {literal} OR {ConvocaFields.Description} like {literal})");
        }

        if (parts.Count == 0)
            return;

        clauses.Add("(" + string.Join(" OR ", parts) + ")");
    }
}

public sealed record ProcessSearchCriteria
{
    public string? Keyword { get; init; }

    public string? EntityName { get; init; }

    public string? EntityNit { get; init; }

    public string? Department { get; init; }

    public string? City { get; init; }

    public string? Modality { get; init; }

    public string? Phase { get; init; }

    public string? Status { get; init; }

    public string? ContractType { get; init; }

    public DateTime? PublishedFrom { get; init; }

    public DateTime? PublishedTo { get; init; }

    public decimal? MinBasePrice { get; init; }

    public decimal? MaxBasePrice { get; init; }

    public bool PublishedOnly { get; init; } = true;

    public bool OpenForOffer { get; init; }

    public DateTime? OffersDueFrom { get; init; }

    public IReadOnlyList<string> ProfilePhrases { get; init; } = [];
}

public static class ConvocaValueParser
{
    private static readonly HashSet<string> EmptySentinels = new(StringComparer.OrdinalIgnoreCase)
    {
        "No Definido",
        "No definido",
        "No Adjudicado",
        "UNSPECIFIED"
    };

    public static string? Clean(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var trimmed = value.Trim();
        return EmptySentinels.Contains(trimmed) ? null : trimmed;
    }

    public static decimal? ParseDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;
    }

    public static int? ParseInt(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;
    }

    public static DateTime? ParseDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return DateTime.TryParse(
            value,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
            out var parsed)
            ? parsed
            : null;
    }

    public static string ResolvePublicUrl(string? processId, string? rawUrl)
    {
        if (!string.IsNullOrWhiteSpace(rawUrl) &&
            !rawUrl.Contains("/STS/Users/Login", StringComparison.OrdinalIgnoreCase))
            return rawUrl;

        if (string.IsNullOrWhiteSpace(processId))
            return string.Empty;

        return "https://community.secop.gov.co/Public/Tendering/OpportunityDetail/Index?noticeUID=" +
               Uri.EscapeDataString(processId);
    }

    public static bool IsSafeProcessId(string? processId)
    {
        if (string.IsNullOrWhiteSpace(processId) || processId.Length > 80)
            return false;

        foreach (var character in processId)
        {
            if (!char.IsLetterOrDigit(character) && character is not '.' and not '-' and not '_')
                return false;
        }

        return true;
    }
}

public static class ProcessCsvFormatter
{
    public static byte[] Format(IReadOnlyList<ProcessResponse> processes)
    {
        var builder = new StringBuilder();
        builder.Append('\uFEFF');
        builder.AppendLine(string.Join(';',
            "Id del proceso",
            "Referencia",
            "Entidad",
            "NIT entidad",
            "Departamento",
            "Ciudad",
            "Procedimiento",
            "Descripción",
            "Fase",
            "Fecha de publicación",
            "Modalidad",
            "Tipo de contrato",
            "Estado",
            "Precio base",
            "Adjudicado",
            "Proveedor",
            "URL"));

        foreach (var process in processes)
        {
            builder.AppendLine(string.Join(';',
                Cell(process.ProcessId),
                Cell(process.Reference),
                Cell(process.EntityName),
                Cell(process.EntityNit),
                Cell(process.Department),
                Cell(process.City),
                Cell(process.ProcedureName),
                Cell(process.Description),
                Cell(process.Phase),
                Cell(process.PublishedOn?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
                Cell(process.Modality),
                Cell(process.ContractType),
                Cell(process.Status),
                Cell(process.BasePrice?.ToString(CultureInfo.InvariantCulture)),
                Cell(process.Awarded),
                Cell(process.SupplierName),
                Cell(process.ProcessUrl)));
        }

        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    private static string Cell(string? value)
    {
        var text = value ?? string.Empty;
        if (text.Contains(';') || text.Contains('"') || text.Contains('\n') || text.Contains('\r'))
            return "\"" + text.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";

        return text;
    }
}
