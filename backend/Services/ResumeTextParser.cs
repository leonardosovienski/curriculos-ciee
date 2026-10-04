using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Curriculos.Api.Services;

public record ResumeParseResult(string? FullName, string? Email, string? Phone);

/// <summary>
/// Heurísticas determinísticas para encontrar nome, e-mail e telefone no texto de um currículo.
/// Quando não há confiança suficiente, o campo volta null: o parser nunca inventa dados,
/// e o usuário sempre pode preencher ou corrigir no formulário.
/// </summary>
public static partial class ResumeTextParser
{
    private const int NameSearchLines = 8;
    private const int NameMaxLength = 60;

    [GeneratedRegex(@"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}")]
    private static partial Regex EmailRegex();

    // Formatos brasileiros: +55, DDD com ou sem parênteses, 8 ou 9 dígitos, separadores opcionais.
    // As fronteiras (?<!\d) e (?!\d) evitam recortar números maiores, como CPF ou CNPJ sem pontuação.
    [GeneratedRegex(@"(?<!\d)(?:\+?55[\s.-]?)?\(?\d{2}\)?[\s.-]?\d{4,5}[\s.-]?\d{4}(?!\d)")]
    private static partial Regex PhoneRegex();

    [GeneratedRegex(@"\b(tel|telefone|fone|celular|cel|whatsapp|contato)\b", RegexOptions.IgnoreCase)]
    private static partial Regex PhoneLabelRegex();

    [GeneratedRegex(@"^[\p{L}][\p{L}'’.\- ]*$")]
    private static partial Regex NameCharactersRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();

    // Palavras que indicam título de seção ou cargo, não nome de pessoa (comparadas sem acento).
    private static readonly HashSet<string> NonNameWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "curriculo", "curriculum", "vitae", "resume", "resumo", "cv", "objetivo", "objetivos", "perfil",
        "contato", "contatos", "experiencia", "experiencias", "formacao", "academica", "habilidades",
        "competencias", "dados", "pessoais", "endereco", "profissional", "idiomas", "cursos",
        "desenvolvedor", "desenvolvedora", "analista", "engenheiro", "engenheira", "estagiario", "estagiaria",
        "programador", "programadora", "assistente", "auxiliar", "tecnico", "tecnica", "gerente",
        "coordenador", "coordenadora", "designer", "developer", "engineer", "software", "sistemas"
    };

    // Partículas comuns em nomes que aparecem em minúsculas ("Maria da Silva").
    private static readonly HashSet<string> NameConnectors = new(StringComparer.Ordinal)
    {
        "de", "da", "do", "das", "dos", "e", "di", "du", "van", "von"
    };

    public static ResumeParseResult Parse(string text)
    {
        var lines = NormalizeLines(text);

        return new ResumeParseResult(
            FullName: FindName(lines),
            Email: FindEmail(lines),
            Phone: FindPhone(lines));
    }

    private static List<string> NormalizeLines(string text) =>
        text.Replace('\u00A0', ' ')
            .Split('\n')
            .Select(line => WhitespaceRegex().Replace(line, " ").Trim())
            .Where(line => line.Length > 0)
            .ToList();

    private static string? FindEmail(List<string> lines)
    {
        foreach (var line in lines)
        {
            var match = EmailRegex().Match(line);
            if (match.Success) return match.Value;
        }

        return null;
    }

    private static string? FindPhone(List<string> lines)
    {
        foreach (var line in lines)
        {
            foreach (Match match in PhoneRegex().Matches(line))
            {
                var raw = match.Value;
                var hasFormatting = raw.Any(c => c is '(' or ')' or '-' or '.' or ' ' or '+');

                // Uma sequência pura de dígitos (ex.: 41987654321) só é aceita com um rótulo
                // como "Telefone" ou "Celular" na mesma linha; sem isso, poderia ser um CPF.
                if (!hasFormatting && !PhoneLabelRegex().IsMatch(line)) continue;

                var formatted = FormatBrazilianPhone(raw);
                if (formatted is not null) return formatted;
            }
        }

        return null;
    }

    private static string? FormatBrazilianPhone(string raw)
    {
        var digits = new string(raw.Where(char.IsDigit).ToArray());

        if (digits.Length is 12 or 13 && digits.StartsWith("55"))
            digits = digits[2..];

        if (digits.Length is not (10 or 11)) return null;
        if (digits[0] == '0' || digits[1] == '0') return null;          // DDD válido: 11 a 99
        if (digits.Length == 11 && digits[2] != '9') return null;        // celular começa com 9

        var ddd = digits[..2];
        var number = digits[2..];
        return number.Length == 9
            ? $"({ddd}) {number[..5]}-{number[5..]}"
            : $"({ddd}) {number[..4]}-{number[4..]}";
    }

    private static string? FindName(List<string> lines)
    {
        foreach (var line in lines.Take(NameSearchLines))
        {
            if (line.Length > NameMaxLength) continue;
            if (!NameCharactersRegex().IsMatch(line)) continue;   // descarta e-mail, URL, números, "|", ":" etc.

            var words = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length is < 2 or > 6) continue;
            if (words.Any(word => NonNameWords.Contains(RemoveAccents(word)))) continue;

            // Em nomes, cada palavra começa com maiúscula (exceto partículas como "da" e "dos").
            // Isso descarta frases comuns de currículo, como "Desenvolvimento de APIs em .NET".
            if (!words.All(word => char.IsUpper(word[0]) || NameConnectors.Contains(word))) continue;

            return line;
        }

        return null;
    }

    private static string RemoveAccents(string value)
    {
        var normalized = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);
        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) builder.Append(c);
        }

        return builder.ToString();
    }
}
