using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using QuranSearchApp.Models;

namespace QuranSearchApp.Services;

public class QuranSearchService
{
    private List<QuranAya> _quranText = new();
    private List<TajweedRule> _rules = new();

    public async Task LoadQuranTextAsync()
    {
        try
        {
            var lines = await File.ReadAllLinesAsync("quran-uthmani.txt");
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
            if (!File.Exists("rules.json"))
            {
                throw new Exception("rules.json file not found");
            }

            var jsonContent = await File.ReadAllTextAsync("rules.json");
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
        if (string.IsNullOrWhiteSpace(ruleName))
            return new List<QuranAya>();

        var rule = _rules.FirstOrDefault(r => r.Name == ruleName);
        if (rule == null)
            return new List<QuranAya>();

        var results = new List<QuranAya>();

        foreach (var aya in _quranText)
        {
            foreach (var ruleCase in rule.Cases)
            {
                try
                {
                    var regex = new Regex(ruleCase.Regex, RegexOptions.IgnoreCase);
                    var matches = regex.Matches(aya.Text);
                    
                    // Add the aya once for each match found
                    foreach (Match match in matches)
                    {
                        var matchPositions = new List<MatchPosition>
                        {
                            new MatchPosition
                            {
                                Start = match.Index,
                                Length = match.Length,
                                MatchedText = match.Value
                            }
                        };

                        results.Add(new QuranAya
                        {
                            SurahNumber = aya.SurahNumber,
                            AyaNumber = aya.AyaNumber,
                            Text = aya.Text,
                            FullLine = aya.FullLine,
                            MatchedCase = ruleCase.Description,
                            MatchedText = match.Value,
                            MatchPositions = matchPositions
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

    // Keep the old method for backward compatibility
    public List<QuranAya> SearchPattern(string pattern)
    {
        return SearchByRuleName(pattern);
    }
}
