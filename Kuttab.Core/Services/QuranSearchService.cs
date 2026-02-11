using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Kuttab.Core.Models;
using Kuttab.Core.Interfaces;

namespace Kuttab.Core.Services;

public class QuranSearchService
{
    private readonly IFileService _fileService;
    
    // List of exceptions for Idgham Bighunnah (words that should not be considered for Idgham)
    private static readonly List<string> IdghamBighunnahExceptions = new()
    {
        "صنوان", "قنوان", "الدنيا", "بنيان",
        // Add more exceptions here if needed
    };

    // Compile the exceptions into a regex pattern for matching
    private static readonly string IdghamBighunnahExceptionsPattern = 
        $"\\b({string.Join("|", IdghamBighunnahExceptions.Select(Regex.Escape))})\\b";

    private List<QuranAya> _quranText = new();
    private List<TajweedRule> _rules = new();
    private string _quranTextFile = "quran-uthmani-ver1.2.txt";

    public QuranSearchService(IFileService fileService)
    {
        _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
    }

    public void SetQuranTextFile(string fileName)
    {
        _quranTextFile = fileName;
        // Reload the Quran text with new file
        _ = LoadQuranTextAsync();
    }

    public async Task LoadQuranTextAsync()
    {
        try
        {
            var content = await _fileService.ReadAllTextAsync(_quranTextFile);
            var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            _quranText.Clear();

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = line.Split('|');
                if (parts.Length >= 3)
                {
                    if (int.TryParse(parts[0], out int surah) && int.TryParse(parts[1], out int aya))
                    {
                        _quranText.Add(new QuranAya
                        {
                            SurahNumber = surah,
                            AyaNumber = aya,
                            Text = parts[2],
                            FullLine = line
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error loading Quran text: {ex.Message}");
        }
    }

    public async Task LoadRulesAsync()
    {
        try
        {
            if (!_fileService.FileExists("rules.json"))
            {
                throw new Exception("rules.json file not found");
            }

            var jsonContent = await _fileService.ReadAllTextAsync("rules.json");
            if (string.IsNullOrWhiteSpace(jsonContent))
            {
                throw new Exception("rules.json file is empty");
            }

            var rulesContainer = JsonSerializer.Deserialize(jsonContent, KuttabJsonContext.Default.RulesContainer);
            if (rulesContainer == null)
            {
                throw new Exception("Failed to deserialize rules.json");
            }

            _rules = rulesContainer.Rules ?? new List<TajweedRule>();
            
            if (_rules.Count == 0)
            {
                throw new Exception("No rules found in rules.json");
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error loading rules: {ex.Message}");
        }
    }

    public List<TajweedRule> GetRules()
    {
        return _rules.ToList();
    }

    public List<string> GetRuleNames()
    {
        return _rules.Select(r => r.Name).ToList();
    }

    public TajweedRule? GetRuleInfo(string ruleName)
    {
        return _rules.FirstOrDefault(r => r.Name == ruleName);
    }

    public List<QuranAya> SearchByRuleName(string ruleName, int? startSurah = null, int? endSurah = null)
    {
        var results = new List<QuranAya>();
        var rule = _rules.FirstOrDefault(r => r.Name == ruleName);
        if (rule == null) return results;

        // Filter Quran text by domain BEFORE searching (more efficient)
        var filteredText = _quranText;
        if (startSurah.HasValue && endSurah.HasValue)
        {
            filteredText = _quranText.Where(a => a.SurahNumber >= startSurah.Value && 
                                                  a.SurahNumber <= endSurah.Value).ToList();
        }
        else if (startSurah.HasValue)
        {
            filteredText = _quranText.Where(a => a.SurahNumber == startSurah.Value).ToList();
        }

        foreach (var aya in filteredText)
        {
            foreach (var ruleCase in rule.Cases)
            {
                try
                {
                    var matches = new List<MatchPosition>();
                    var regex = new Regex(ruleCase.Regex, RegexOptions.CultureInvariant);
                    var regexMatches = regex.Matches(aya.Text);
                    
                    foreach (Match match in regexMatches)
                    {
                        // Check for Idgham Bighunnah exceptions
                        if (ruleName == "إدغام النون الساكنة والتنوين - بغنة")
                        {
                            // Get the full word containing the match
                            var wordStart = aya.Text.LastIndexOf(' ', match.Index) + 1;
                            if (wordStart < 0) wordStart = 0;
                            var nextSpace = aya.Text.IndexOf(' ', match.Index + match.Length);
                            if (nextSpace < 0) nextSpace = aya.Text.Length;
                            var wordLength = nextSpace - wordStart;
                            var word = aya.Text.Substring(wordStart, wordLength);

                            // Check if this word is in our exceptions list
                            if (IdghamBighunnahExceptions.Any(ex => word.Contains(ex)))
                            {
                                continue; // Skip this match as it's an exception
                            }
                        }
                        
                        // Automatically filter for 'لام لفظ الجلالة المفخمة' rule
                        if (ruleName == "لام لفظ الجلالة المفخمة")
                        {
                            // Check if the match is for 'الله' and has Tafkheem
                            if (!AllahFilter.IsLamMufakhkham(aya.Text, match.Index + 1)) // +1 because the regex matches the whole word
                                continue;
                        }
                        
                        matches.Add(new MatchPosition
                        {
                            Start = match.Index,
                            Length = match.Length,
                            MatchedText = match.Value
                        });
                    }

                    // Fallback: if no regex matches and the pattern appears to be a single literal symbol,
                    // do a direct scan for that Unicode character (useful for small Quranic stop signs)
                    if (matches.Count == 0)
                    {
                        var pattern = ruleCase.Regex;
                        // Heuristic: treat as literal if it has length 1 and no regex metacharacters
                        if (!string.IsNullOrEmpty(pattern) && pattern.Length == 1)
                        {
                            var ch = pattern[0];
                            for (int i = 0; i < aya.Text.Length; i++)
                            {
                                if (aya.Text[i] == ch)
                                {
                                    matches.Add(new MatchPosition
                                    {
                                        Start = i,
                                        Length = 1,
                                        MatchedText = aya.Text.Substring(i, 1)
                                    });
                                }
                            }
                        }
                    }

                    if (matches.Count > 0)
                    {
                        results.Add(new QuranAya
                        {
                            SurahNumber = aya.SurahNumber,
                            AyaNumber = aya.AyaNumber,
                            Text = aya.Text,
                            FullLine = aya.FullLine,
                            MatchedCase = ruleCase.Description,
                            MatchedText = string.Join(", ", matches.Select(m => m.MatchedText)),
                            MatchPositions = matches
                        });
                    }
                }
                catch (Exception)
                {
                    // Skip invalid regex patterns
                    continue;
                }
            }
        }

        return results;
    }

    public List<QuranAya> SearchPattern(string pattern, int? startSurah = null, int? endSurah = null)
    {
        return SearchByRuleName(pattern, startSurah, endSurah);
    }

    public List<TajweedRuleMatch> SearchAyaForRules(string ayaText, List<string>? selectedRuleNames = null)
    {
        var results = new List<TajweedRuleMatch>();
        if (string.IsNullOrWhiteSpace(ayaText) || _rules.Count == 0)
            return results;

        var rulesToSearch = selectedRuleNames != null && selectedRuleNames.Count > 0
            ? _rules.Where(r => selectedRuleNames.Contains(r.Name)).ToList()
            : _rules;

        foreach (var rule in rulesToSearch)
        {
            foreach (var ruleCase in rule.Cases)
            {
                try
                {
                    var regex = new Regex(ruleCase.Regex, RegexOptions.CultureInvariant);
                    var regexMatches = regex.Matches(ayaText);

                    foreach (Match match in regexMatches)
                    {
                        // Apply Idgham Bighunnah exceptions
                        if (rule.Name == "إدغام النون الساكنة والتنوين - بغنة")
                        {
                            var wordStart = ayaText.LastIndexOf(' ', match.Index) + 1;
                            if (wordStart < 0) wordStart = 0;
                            var nextSpace = ayaText.IndexOf(' ', match.Index + match.Length);
                            if (nextSpace < 0) nextSpace = ayaText.Length;
                            var word = ayaText.Substring(wordStart, nextSpace - wordStart);
                            if (IdghamBighunnahExceptions.Any(ex => word.Contains(ex)))
                                continue;
                        }

                        // Apply Lam Mufakhkham filter
                        if (rule.Name == "لام لفظ الجلالة المفخمة")
                        {
                            if (!AllahFilter.IsLamMufakhkham(ayaText, match.Index + 1))
                                continue;
                        }

                        // Extract surrounding words
                        var surroundingStart = match.Index;
                        var surroundingEnd = match.Index + match.Length;

                        // Expand to include the word before
                        if (surroundingStart > 0)
                        {
                            // Go back to find start of current word
                            int pos = surroundingStart - 1;
                            while (pos >= 0 && ayaText[pos] == ' ') pos--;
                            // Now find start of that word
                            while (pos >= 0 && ayaText[pos] != ' ') pos--;
                            surroundingStart = pos + 1;
                        }

                        // Expand to include the word after
                        if (surroundingEnd < ayaText.Length)
                        {
                            int pos = surroundingEnd;
                            while (pos < ayaText.Length && ayaText[pos] == ' ') pos++;
                            // Now find end of that word
                            while (pos < ayaText.Length && ayaText[pos] != ' ') pos++;
                            surroundingEnd = pos;
                        }

                        var surroundingText = ayaText.Substring(surroundingStart, surroundingEnd - surroundingStart);
                        var highlightStartInSurrounding = match.Index - surroundingStart;

                        results.Add(new TajweedRuleMatch
                        {
                            RuleName = rule.Name,
                            GroupName = rule.Group ?? string.Empty,
                            MatchStart = match.Index,
                            MatchLength = match.Length,
                            MatchedText = match.Value,
                            SurroundingText = surroundingText,
                            SurroundingStart = surroundingStart,
                            HighlightStartInSurrounding = highlightStartInSurrounding,
                            HighlightLengthInSurrounding = match.Length
                        });
                    }

                    // Fallback for single literal character patterns
                    if (regexMatches.Count == 0 && !string.IsNullOrEmpty(ruleCase.Regex) && ruleCase.Regex.Length == 1)
                    {
                        var ch = ruleCase.Regex[0];
                        for (int i = 0; i < ayaText.Length; i++)
                        {
                            if (ayaText[i] == ch)
                            {
                                var sStart = i;
                                var sEnd = i + 1;
                                if (sStart > 0) { int p = sStart - 1; while (p >= 0 && ayaText[p] == ' ') p--; while (p >= 0 && ayaText[p] != ' ') p--; sStart = p + 1; }
                                if (sEnd < ayaText.Length) { int p = sEnd; while (p < ayaText.Length && ayaText[p] == ' ') p++; while (p < ayaText.Length && ayaText[p] != ' ') p++; sEnd = p; }
                                var surrounding = ayaText.Substring(sStart, sEnd - sStart);

                                results.Add(new TajweedRuleMatch
                                {
                                    RuleName = rule.Name,
                                    GroupName = rule.Group ?? string.Empty,
                                    MatchStart = i,
                                    MatchLength = 1,
                                    MatchedText = ayaText.Substring(i, 1),
                                    SurroundingText = surrounding,
                                    SurroundingStart = sStart,
                                    HighlightStartInSurrounding = i - sStart,
                                    HighlightLengthInSurrounding = 1
                                });
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    continue;
                }
            }
        }

        // Sort by position of occurrence in the aya
        results.Sort((a, b) => a.MatchStart.CompareTo(b.MatchStart));
        return results;
    }
}
