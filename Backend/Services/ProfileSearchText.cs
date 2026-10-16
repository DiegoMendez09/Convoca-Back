namespace Backend.Services;

public static class ProfileSearchText
{
    private static readonly HashSet<string> Stopwords = new(StringComparer.OrdinalIgnoreCase)
    {
        "para", "como", "entre", "sobre", "desde", "hasta", "donde", "cuando", "anos", "años",
        "experiencia", "laboral", "trabajo", "titulo", "título", "profesional", "proceso",
        "contratos", "contrato", "servicios", "servicio"
    };

    public static string? Build(string? title, string? experience, string? keywords)
    {
        var phrases = Phrases(title, experience, keywords);
        return phrases.Count == 0 ? null : Limit(string.Join(" o ", phrases));
    }

    public static IReadOnlyList<string> Phrases(string? title, string? experience, string? keywords)
    {
        if (!string.IsNullOrWhiteSpace(keywords))
        {
            return keywords.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(term => term.Length > 1)
                .Take(4)
                .ToArray();
        }

        var tokens = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var token in Tokenize($"{title} {experience}"))
        {
            if (token.Length < 4 || Stopwords.Contains(token) || !seen.Add(token))
                continue;

            tokens.Add(token);
            if (tokens.Count == 4)
                break;
        }

        return tokens;
    }

    private static IEnumerable<string> Tokenize(string value)
    {
        var current = new List<char>();
        foreach (var character in value)
        {
            if (char.IsLetterOrDigit(character))
            {
                current.Add(character);
                continue;
            }

            if (current.Count == 0)
                continue;

            yield return new string(current.ToArray());
            current.Clear();
        }

        if (current.Count > 0)
            yield return new string(current.ToArray());
    }

    private static string? Limit(string text)
    {
        var trimmed = text.Trim();
        if (trimmed.Length == 0)
            return null;

        return trimmed.Length <= 200 ? trimmed : trimmed[..200].Trim();
    }
}
