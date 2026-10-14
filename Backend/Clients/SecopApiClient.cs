using System.Text.Json;
using Backend.Models.External;
using Backend.Options;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace Backend.Clients;

public sealed class SecopApiClient : ISecopApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly SecopOptions _options;
    private readonly ILogger<SecopApiClient> _logger;

    public SecopApiClient(HttpClient httpClient, IOptions<SecopOptions> options, ILogger<SecopApiClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyList<SecopProcessRecord>> SearchAsync(
        string where,
        int limit,
        int offset,
        CancellationToken cancellationToken)
    {
        var query = new Dictionary<string, string?>
        {
            ["$select"] = SecopFields.SelectList,
            ["$order"] = $"{SecopFields.PublishedOn} DESC",
            ["$limit"] = limit.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["$offset"] = offset.ToString(System.Globalization.CultureInfo.InvariantCulture)
        };

        if (!string.IsNullOrWhiteSpace(where))
            query["$where"] = where;

        var body = await GetStringAsync(query, cancellationToken);
        var records = JsonSerializer.Deserialize<List<SecopProcessRecord>>(body, JsonOptions);
        return records ?? [];
    }

    public async Task<int> CountAsync(string where, CancellationToken cancellationToken)
    {
        var query = new Dictionary<string, string?>
        {
            ["$select"] = "count(*)"
        };

        if (!string.IsNullOrWhiteSpace(where))
            query["$where"] = where;

        var body = await GetStringAsync(query, cancellationToken);
        return ReadCount(body);
    }

    private async Task<string> GetStringAsync(
        Dictionary<string, string?> query,
        CancellationToken cancellationToken)
    {
        var path = QueryHelpers.AddQueryString($"{_options.DatasetId}.json", query);
        using var response = await _httpClient.GetAsync(path, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (response.IsSuccessStatusCode)
            return body;

        _logger.LogError(
            "SECOP II respondió {StatusCode}. Cuerpo: {Body}",
            (int)response.StatusCode,
            body);
        throw new SecopUnavailableException("No fue posible consultar los procesos de SECOP II.");
    }

    private static int ReadCount(string json)
    {
        using var document = JsonDocument.Parse(json);
        if (document.RootElement.ValueKind != JsonValueKind.Array || document.RootElement.GetArrayLength() == 0)
            return 0;

        foreach (var property in document.RootElement[0].EnumerateObject())
        {
            if (!property.Name.StartsWith("count", StringComparison.OrdinalIgnoreCase))
                continue;

            if (property.Value.ValueKind == JsonValueKind.Number && property.Value.TryGetInt32(out var number))
                return number;

            if (property.Value.ValueKind == JsonValueKind.String &&
                int.TryParse(property.Value.GetString(), out var parsed))
                return parsed;
        }

        return 0;
    }
}

public sealed class SecopUnavailableException : Exception
{
    public SecopUnavailableException(string message) : base(message)
    {
    }
}
