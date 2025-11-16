using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using QuranSearch.Core.Models;
using QuranSearch.Core.Interfaces;

namespace QuranSearch.Core.Services;

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

    public QuranSearchService(IFileService fileService)
    {
        _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
    }

    public async Task LoadQuranTextAsync()
    {
        try
        {
            var content = await _fileService.ReadAllTextAsync("quran-uthmani.txt");
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

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var rulesContainer = JsonSerializer.Deserialize<RulesContainer>(jsonContent, options);
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

    public List<QuranAya> SearchByRuleName(string ruleName)
    {
        var results = new List<QuranAya>();
        var rule = _rules.FirstOrDefault(r => r.Name == ruleName);
        if (rule == null) return results;

        foreach (var aya in _quranText)
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

    public List<QuranAya> SearchPattern(string pattern)
    {
        return SearchByRuleName(pattern);
    }
}
