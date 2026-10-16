using System.Text.RegularExpressions;

namespace Backend.Services;

public static class ManualOpecExtractor
{
    public static ManualOpecSection? Extract(string? manualText, string? opecCode)
    {
        var code = NormalizeCode(opecCode);
        if (code.Length < 3 || string.IsNullOrWhiteSpace(manualText))
            return null;

        var text = manualText.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
        ManualOpecSection? best = null;
        var bestScore = 0;
        foreach (Match match in Regex.Matches(text, $@"(?<![A-Z0-9]){Regex.Escape(code)}(?![A-Z0-9])", RegexOptions.IgnoreCase))
        {
            var candidate = ReadSection(text, match, code);
            if (candidate is null)
                continue;

            var score = candidate.Purpose.Length + candidate.Functions.Length + candidate.Knowledge.Length;
            if (score > bestScore)
            {
                best = candidate;
                bestScore = score;
            }
        }

        return bestScore >= 40 ? best : null;
    }

    private static ManualOpecSection? ReadSection(string text, Match match, string code)
    {
        var start = BlockStart(text, match.Index, code);
        var end = BlockEnd(text, Math.Max(match.Index + match.Length, start), code);
        if (end <= start)
            return null;

        var section = text[start..end].Trim();
        if (!ContainsCode(section, code))
            return null;

        return new ManualOpecSection(
            code,
            Field(section, "DENOMINACIÓN", "DENOMINACION"),
            NormalizeLevel(Field(section, "NIVEL")),
            Field(section, "GRADO"),
            Between(section, "PROPÓSITO PRINCIPAL", "PROPOSITO PRINCIPAL"),
            Between(section, "FUNCIONES ESENCIALES", "DESCRIPCIÓN DE FUNCIONES", "DESCRIPCION DE FUNCIONES"),
            Between(section, "CONOCIMIENTOS BÁSICOS", "CONOCIMIENTOS BASICOS", "CONOCIMIENTOS ESENCIALES"),
            Limit(section, 20000));
    }

    public static string NormalizeCode(string? opecCode)
    {
        var value = (opecCode ?? string.Empty).Trim();
        value = Regex.Replace(value, @"^(OPEC|NÚMERO DE OPEC|NUMERO DE OPEC)\s*", string.Empty, RegexOptions.IgnoreCase);
        return Regex.Replace(value, @"\s+", string.Empty).ToUpperInvariant();
    }

    private static Match? FindCode(string text, string code)
    {
        var pattern = $@"(?<![A-Z0-9]){Regex.Escape(code)}(?![A-Z0-9])";
        var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
        return match.Success ? match : null;
    }

    private static bool ContainsCode(string text, string code)
    {
        return FindCode(text, code) is not null;
    }

    private static int BlockStart(string text, int codeIndex, string code)
    {
        if (HeadsItsOwnFicha(text, codeIndex))
            return codeIndex;

        var identification = LastIndexBefore(text, codeIndex, "IDENTIFICACIÓN DEL EMPLEO", "IDENTIFICACION DEL EMPLEO");
        var previousOpec = PreviousDifferentOpec(text, codeIndex, code);
        var start = identification;
        if (previousOpec > start)
            start = previousOpec;

        return Math.Max(0, start);
    }

    private static int BlockEnd(string text, int afterCode, string code)
    {
        var nextIdentification = NextIdentification(text, afterCode);
        var nextOpec = NextDifferentOpec(text, afterCode, code);
        var end = text.Length;
        if (nextIdentification > afterCode)
            end = Math.Min(end, nextIdentification);
        if (nextOpec > afterCode)
            end = Math.Min(end, nextOpec);
        return end;
    }

    private static bool HeadsItsOwnFicha(string text, int codeIndex)
    {
        var identification = NextIndex(text, codeIndex, "IDENTIFICACIÓN DEL EMPLEO", "IDENTIFICACION DEL EMPLEO");
        if (identification < 0)
            return false;

        var gap = text[codeIndex..identification];
        return gap.Length <= 80
            && !gap.Contains("PROPÓSITO", StringComparison.OrdinalIgnoreCase)
            && !gap.Contains("PROPOSITO", StringComparison.OrdinalIgnoreCase)
            && !gap.Contains("FUNCIONES", StringComparison.OrdinalIgnoreCase);
    }

    private static int NextIdentification(string text, int afterCode)
    {
        var next = NextIndex(text, afterCode, "IDENTIFICACIÓN DEL EMPLEO", "IDENTIFICACION DEL EMPLEO");
        if (next < 0)
            return -1;

        var gap = text[afterCode..next];
        if (gap.Length <= 80
            && !gap.Contains("PROPÓSITO", StringComparison.OrdinalIgnoreCase)
            && !gap.Contains("PROPOSITO", StringComparison.OrdinalIgnoreCase)
            && !gap.Contains("FUNCIONES", StringComparison.OrdinalIgnoreCase))
            return NextIndex(text, next + 1, "IDENTIFICACIÓN DEL EMPLEO", "IDENTIFICACION DEL EMPLEO");

        return next;
    }

    private static int PreviousDifferentOpec(string text, int before, string code)
    {
        var last = 0;
        foreach (Match match in OpecPattern().Matches(text))
        {
            if (match.Index >= before)
                break;

            var found = NormalizeCode(match.Groups[1].Value);
            if (!found.Equals(code, StringComparison.OrdinalIgnoreCase))
                last = match.Index;
        }

        return last;
    }

    private static int NextDifferentOpec(string text, int after, string code)
    {
        foreach (Match match in OpecPattern().Matches(text))
        {
            if (match.Index < after)
                continue;

            var found = NormalizeCode(match.Groups[1].Value);
            if (!found.Equals(code, StringComparison.OrdinalIgnoreCase))
                return match.Index;
        }

        return -1;
    }

    private static int LastIndexBefore(string text, int before, params string[] markers)
    {
        var last = -1;
        foreach (var marker in markers)
        {
            var index = text.LastIndexOf(marker, before, StringComparison.OrdinalIgnoreCase);
            if (index > last)
                last = index;
        }

        return last;
    }

    private static int NextIndex(string text, int after, params string[] markers)
    {
        var next = -1;
        foreach (var marker in markers)
        {
            var index = text.IndexOf(marker, after, StringComparison.OrdinalIgnoreCase);
            if (index >= 0 && (next < 0 || index < next))
                next = index;
        }

        return next;
    }

    private static string Between(string section, params string[] startMarkers)
    {
        var start = NextIndex(section, 0, startMarkers);
        if (start < 0)
            return string.Empty;

        var marker = startMarkers.First(item => section.IndexOf(item, start, StringComparison.OrdinalIgnoreCase) == start
            || section.IndexOf(item, StringComparison.OrdinalIgnoreCase) == start);
        var raw = Regex.Replace(section[(start + marker.Length)..], @"^\s*(?:O\s+)?ESENCIALES\b", string.Empty, RegexOptions.IgnoreCase);
        var end = NextIndex(raw, 0,
            "IDENTIFICACIÓN DEL EMPLEO",
            "IDENTIFICACION DEL EMPLEO",
            "PROPÓSITO PRINCIPAL",
            "PROPOSITO PRINCIPAL",
            "FUNCIONES ESENCIALES",
            "DESCRIPCIÓN DE FUNCIONES",
            "DESCRIPCION DE FUNCIONES",
            "CONOCIMIENTOS BÁSICOS",
            "CONOCIMIENTOS BASICOS",
            "CONOCIMIENTOS ESENCIALES",
            "REQUISITOS DE ESTUDIO",
            "COMPETENCIAS");

        if (end < 0)
            end = raw.Length;

        return Clean(raw[..end]);
    }

    private static string? Field(string section, params string[] labels)
    {
        foreach (var label in labels)
        {
            var match = Regex.Match(
                section,
                $@"{Regex.Escape(label)}(?:\s+DEL\s+EMPLEO)?\s*[:\-]?\s*(.+)",
                RegexOptions.IgnoreCase);
            if (!match.Success)
                continue;

            var line = match.Groups[1].Value.Split('\n')[0].Trim();
            var value = Regex.Match(
                line,
                @"^(.+?)(?=\s{2,}|\s+(?:Nivel|Grado|Código|Codigo|Número|Numero|OPEC|Dependencia|Propósito|Proposito|Funciones|Conocimientos)\b|$)",
                RegexOptions.IgnoreCase).Groups[1].Value.Trim().Trim(':', '-', ' ');
            if (value.Length > 0)
                return Limit(value, 200);
        }

        return null;
    }

    private static string? NormalizeLevel(string? value)
    {
        return MeritExamGenerator.NormalizeLevel(value);
    }

    private static string Clean(string value)
    {
        var lines = value.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(line => line.Length > 0);
        return Limit(string.Join('\n', lines), 8000);
    }

    private static string Limit(string value, int max)
    {
        var trimmed = value.Trim();
        return trimmed.Length <= max ? trimmed : trimmed[..max].Trim();
    }

    private static Regex OpecPattern()
    {
        return new Regex(
            @"OPEC[\s\.:°ºnNro°]*([A-Z0-9][A-Z0-9\-]{2,20})",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }
}

public sealed record ManualOpecSection(
    string OpecCode,
    string? JobTitle,
    string? Level,
    string? Grade,
    string Purpose,
    string Functions,
    string Knowledge,
    string SectionText);
