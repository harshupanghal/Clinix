namespace Clinix.Application.Utilities;

public static class TextNormalizer
    {
    // Basic normalizer: lower, remove diacritics, strip punctuation
    public static string Normalize(string input)
        {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        var lowered = input.ToLowerInvariant();
        // remove diacritics
        var normalized = lowered.Normalize(System.Text.NormalizationForm.FormD);
        var sb = new System.Text.StringBuilder();
        foreach (var ch in normalized)
            {
            var uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch);
            if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                sb.Append(ch);
            }
        var cleaned = sb.ToString().Normalize(System.Text.NormalizationForm.FormC);
        // remove punctuation
        cleaned = System.Text.RegularExpressions.Regex.Replace(cleaned, @"[^\w\s]", " ");
        // collapse spaces
        cleaned = System.Text.RegularExpressions.Regex.Replace(cleaned, @"\s+", " ").Trim();
        return cleaned;
        }
    }
