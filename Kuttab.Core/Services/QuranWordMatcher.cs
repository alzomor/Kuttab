using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Kuttab.Core.Services;

/// <summary>
/// Normalizes Arabic text and performs fuzzy word matching for Hifz (memorization) verification.
/// Compares ASR output against expected Quran text word-by-word.
/// </summary>
public class QuranWordMatcher
{
    // Unicode ranges for Arabic diacritics (harakat)
    private static readonly Regex HarakatRegex = new Regex(
        "[\u0610-\u061A\u064B-\u065F\u0670\u06D6-\u06DC\u06DF-\u06E4\u06E7\u06E8\u06EA-\u06ED\u08D3-\u08E1\u08E3-\u08FF\uFE70-\uFE7F]",
        RegexOptions.Compiled);

    // Common Quran-specific characters to normalize
    private static readonly Dictionary<char, char> CharNormalization = new()
    {
        { '\u0671', '\u0627' }, // ٱ (alef wasla) → ا (alef)
        { '\u0622', '\u0627' }, // آ (alef madda) → ا
        { '\u0623', '\u0627' }, // أ (alef hamza above) → ا
        { '\u0625', '\u0627' }, // إ (alef hamza below) → ا
        { '\u0624', '\u0648' }, // ؤ (waw hamza) → و
        { '\u0626', '\u064A' }, // ئ (ya hamza) → ي
        { '\u0629', '\u0647' }, // ة (ta marbuta) → ه
        { '\u0649', '\u064A' }, // ى (alef maqsura) → ي
        { '\u0670', '\u0627' }, // ٰ (superscript alef) → ا
        { '\u06E5', '\u0648' }, // ۥ (small waw) → و
        { '\u06E6', '\u064A' }, // ۦ (small yaa) → ي
    };

    // Small Quran annotation characters to remove (excluding small letters which are normalized)
    private static readonly HashSet<char> SmallAnnotations = new()
    {
        '\u06D6', '\u06D7', '\u06D8', '\u06D9', '\u06DA', '\u06DB', '\u06DC',
        '\u06DD', '\u06DE', '\u06DF', '\u06E0', '\u06E1', '\u06E2', '\u06E3',
        '\u06E4', '\u06E7', '\u06E8', '\u06E9', '\u06EA',
        '\u06EB', '\u06EC', '\u06ED',
        '\u0615', '\u0616', '\u0617', '\u0618', '\u0619', '\u061A',
        '\u08D4', '\u08D5', '\u08D6', '\u08D7', '\u08D8', '\u08D9',
        '\u08DA', '\u08DB', '\u08DC', '\u08DD', '\u08DE', '\u08DF',
        '\u08E0', '\u08E1', '\u08E3', '\u08E4', '\u08E5', '\u08E6',
        '\u08E7', '\u08E8', '\u08E9', '\u08EA', '\u08EB', '\u08EC',
        '\u0640', // tatweel (kashida)
    };

    /// <summary>
    /// Normalize Arabic text by stripping harakat, normalizing letter forms, and lowering.
    /// </summary>
    public static string Normalize(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        // First pass: normalize characters (including small letters) BEFORE removing harakat
        // This ensures small yaa/waw/alef are replaced before being stripped by HarakatRegex
        var sb = new StringBuilder(text.Length);
        foreach (var ch in text)
        {
            // Normalize characters first
            if (CharNormalization.TryGetValue(ch, out var normalized))
                sb.Append(normalized);
            else
                sb.Append(ch);
        }

        // Second pass: remove harakat and annotations
        var normalized1 = sb.ToString();
        var stripped = HarakatRegex.Replace(normalized1, "");

        var sb2 = new StringBuilder(stripped.Length);
        foreach (var ch in stripped)
        {
            // Skip small annotations
            if (SmallAnnotations.Contains(ch))
                continue;
            sb2.Append(ch);
        }

        return sb2.ToString().Trim();
    }

    /// <summary>
    /// Tokenize Arabic text into words, splitting on whitespace.
    /// </summary>
    public static List<string> TokenizeWords(string text)
    {
        var words = new List<string>();
        if (string.IsNullOrWhiteSpace(text))
            return words;

        var parts = text.Split(new[] { ' ', '\t', '\n', '\r', '\u00A0' },
            StringSplitOptions.RemoveEmptyEntries);

        foreach (var part in parts)
        {
            var trimmed = part.Trim();
            if (trimmed.Length > 0)
                words.Add(trimmed);
        }

        return words;
    }

    /// <summary>
    /// Compute Levenshtein distance between two strings.
    /// </summary>
    public static int LevenshteinDistance(string a, string b)
    {
        if (string.IsNullOrEmpty(a)) return b?.Length ?? 0;
        if (string.IsNullOrEmpty(b)) return a.Length;

        var lenA = a.Length;
        var lenB = b.Length;

        // Use single-row optimization
        var prev = new int[lenB + 1];
        var curr = new int[lenB + 1];

        for (int j = 0; j <= lenB; j++)
            prev[j] = j;

        for (int i = 1; i <= lenA; i++)
        {
            curr[0] = i;
            for (int j = 1; j <= lenB; j++)
            {
                int cost = a[i - 1] == b[j - 1] ? 0 : 1;
                curr[j] = Math.Min(
                    Math.Min(curr[j - 1] + 1, prev[j] + 1),
                    prev[j - 1] + cost);
            }
            (prev, curr) = (curr, prev);
        }

        return prev[lenB];
    }

    /// <summary>
    /// Returns the Levenshtein distance between a recognized and expected word after normalization.
    /// Returns int.MaxValue if either word is empty after normalization.
    /// </summary>
    public static int GetMatchDistance(string recognized, string expected)
    {
        var normRecognized = Normalize(recognized);
        var normExpected = Normalize(expected);

        if (string.IsNullOrEmpty(normRecognized) || string.IsNullOrEmpty(normExpected))
            return int.MaxValue;

        if (normRecognized == normExpected)
            return 0;

        return LevenshteinDistance(normRecognized, normExpected);
    }

    /// <summary>
    /// Check if a recognized word matches an expected word using normalized fuzzy matching.
    /// </summary>
    /// <param name="recognized">The ASR-recognized word</param>
    /// <param name="expected">The expected Quran word</param>
    /// <param name="maxDistance">Maximum Levenshtein distance to accept (default 2)</param>
    /// <returns>True if words match within the threshold</returns>
    public static bool IsWordMatch(string recognized, string expected, int maxDistance = 2)
    {
        var normRecognized = Normalize(recognized);
        var normExpected = Normalize(expected);

        if (string.IsNullOrEmpty(normRecognized) || string.IsNullOrEmpty(normExpected))
            return false;

        // Exact match after normalization
        if (normRecognized == normExpected)
            return true;

        // Fuzzy match using Levenshtein distance
        var distance = LevenshteinDistance(normRecognized, normExpected);

        // Scale threshold by word length to avoid false matches
        // Short words (<=2 chars): max 1 edit
        // Medium words (<=5 chars): max 1 edit (prevents الرحيم matching الرحمن)
        // Longer words: use full maxDistance
        var effectiveMax = normExpected.Length <= 5 ? 1 : maxDistance;

        return distance <= effectiveMax;
    }

    /// <summary>
    /// Try to match recognized text against the expected words starting from a given position.
    /// Returns the number of newly matched words.
    /// </summary>
    public static int MatchRecognizedText(string recognizedText, List<string> expectedWords,
        int currentWordIndex, int maxDistance = 2)
    {
        if (string.IsNullOrWhiteSpace(recognizedText) || expectedWords == null)
            return 0;

        var recognizedWords = TokenizeWords(recognizedText);
        if (recognizedWords.Count == 0)
            return 0;

        int matched = 0;
        int recStartIdx = 0;

        // Skip recognized words that match already-completed words
        for (int i = 0; i < recognizedWords.Count && recStartIdx < recognizedWords.Count; i++)
        {
            if (i >= currentWordIndex) break;
            if (IsWordMatch(recognizedWords[recStartIdx], expectedWords[i], maxDistance))
            {
                recStartIdx++;
            }
            else
            {
                break;
            }
        }

        // Match remaining recognized words against expected words from currentWordIndex
        for (int r = recStartIdx; r < recognizedWords.Count; r++)
        {
            var nextExpectedIdx = currentWordIndex + matched;
            if (nextExpectedIdx >= expectedWords.Count)
                break;

            if (IsWordMatch(recognizedWords[r], expectedWords[nextExpectedIdx], maxDistance))
            {
                matched++;
            }
            else
            {
                // Try lookahead — ASR might skip a word
                bool foundAhead = false;
                for (int lookahead = 1; lookahead <= 2 && nextExpectedIdx + lookahead < expectedWords.Count; lookahead++)
                {
                    if (IsWordMatch(recognizedWords[r], expectedWords[nextExpectedIdx + lookahead], maxDistance))
                    {
                        matched += lookahead + 1;
                        foundAhead = true;
                        break;
                    }
                }

                if (!foundAhead)
                {
                    break;
                }
            }
        }

        return matched;
    }

    /// <summary>
    /// Match recognized words against expected words starting from a given expected index.
    /// Returns the total number of expected words matched (absolute position, not relative).
    /// Works with cumulative ASR text within a single recognition cycle.
    /// Also works with fresh recognition cycles by passing startFromExpectedIndex.
    /// </summary>
    public static int CountMatchedFromStart(string recognizedText, List<string> expectedWords, 
        int maxDistance = 2, int startFromExpectedIndex = 0)
    {
        if (string.IsNullOrWhiteSpace(recognizedText) || expectedWords == null || expectedWords.Count == 0)
            return startFromExpectedIndex;

        var recognizedWords = TokenizeWords(recognizedText);
        if (recognizedWords.Count == 0)
            return startFromExpectedIndex;

        int expectedIdx = startFromExpectedIndex;

        for (int r = 0; r < recognizedWords.Count; r++)
        {
            if (expectedIdx >= expectedWords.Count)
                break;

            if (IsWordMatch(recognizedWords[r], expectedWords[expectedIdx], maxDistance))
            {
                expectedIdx++;
            }
            else
            {
                // Try lookahead — ASR might merge/skip words
                bool foundAhead = false;
                for (int lookahead = 1; lookahead <= 2 && expectedIdx + lookahead < expectedWords.Count; lookahead++)
                {
                    if (IsWordMatch(recognizedWords[r], expectedWords[expectedIdx + lookahead], maxDistance))
                    {
                        expectedIdx += lookahead + 1;
                        foundAhead = true;
                        break;
                    }
                }

                if (!foundAhead)
                {
                    // Unrecognized word — skip it (ASR noise)
                    // Don't break — allow continuing to match subsequent words
                }
            }
        }

        return expectedIdx;
    }
}
