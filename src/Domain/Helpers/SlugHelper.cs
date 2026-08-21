using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Domain.Helpers;

public static class SlugHelper
{
    public static string Generate(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        // NFD decomposition separates base chars from combining diacritics
        var normalized = input.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalized.Length);
        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }

        var result = sb.ToString()
            .Normalize(NormalizationForm.FormC)
            .ToLowerInvariant();

        // Replace any non-alphanumeric sequence with a single hyphen
        result = Regex.Replace(result, @"[^a-z0-9]+", "-");

        return result.Trim('-');
    }
}
