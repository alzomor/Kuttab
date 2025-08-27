using System.Text.Json;
using System.Text.RegularExpressions;
using QuranSearch.Core.Models;

namespace QuranSearch.Web.Services;

public class WebQuranSearchService
{
    private readonly HttpClient _httpClient;
    private List<QuranAya> _quranText = new();
    private List<TajweedRule> _rules = new();

    public WebQuranSearchService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task LoadQuranTextAsync()
    {
        try
        {
            var content = await _httpClient.GetStringAsync("data/quran-uthmani.txt");
            var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);

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
            var jsonContent = await _httpClient.GetStringAsync("data/rules.json");
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var rulesContainer = JsonSerializer.Deserialize<RulesContainer>(jsonContent, options);
            
            if (rulesContainer?.Rules != null)
            {
                _rules = rulesContainer.Rules;
                Console.WriteLine($"Loaded {_rules.Count} rules successfully");
            }
            else
            {
                Console.WriteLine("No rules found in JSON");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading rules: {ex.Message}");
            throw new Exception($"Error loading rules: {ex.Message}");
        }
    }

    public List<string> GetRuleNames()
    {
        Console.WriteLine($"GetRuleNames called, _rules count: {_rules.Count}");
        var names = _rules.Select(r => r.Name).ToList();
        Console.WriteLine($"Rule names: {string.Join(", ", names)}");
        return names;
    }

    public List<QuranAya> SearchByRuleName(string ruleName)
    {
        var matchedAyas = new List<QuranAya>();
        var rule = _rules.FirstOrDefault(r => r.Name == ruleName);
        
        if (rule == null) return matchedAyas;

        foreach (var aya in _quranText)
        {
            var ayaMatches = new List<MatchPosition>();
            
            foreach (var ruleCase in rule.Cases)
            {
                try
                {
                    var regex = new Regex(ruleCase.Regex, RegexOptions.IgnoreCase);
                    var matches = regex.Matches(aya.Text);
                    
                    foreach (Match match in matches)
                    {
                        ayaMatches.Add(new MatchPosition
                        {
                            Start = match.Index,
                            Length = match.Length,
                            MatchedText = match.Value,
                            RuleName = ruleName,
                            CaseName = ruleCase.Name
                        });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Regex error for rule '{ruleName}', case '{ruleCase.Name}': {ex.Message}");
                }
            }

            if (ayaMatches.Any())
            {
                var matchedAya = new QuranAya
                {
                    SurahNumber = aya.SurahNumber,
                    AyaNumber = aya.AyaNumber,
                    Text = aya.Text,
                    FullLine = aya.FullLine,
                    Matches = ayaMatches
                };
                matchedAyas.Add(matchedAya);
            }
        }

        return matchedAyas;
    }

    public List<QuranAya> GetAllAyas()
    {
        return _quranText;
    }

    public QuranAya? GetAya(int surahNumber, int ayaNumber)
    {
        return _quranText.FirstOrDefault(a => a.SurahNumber == surahNumber && a.AyaNumber == ayaNumber);
    }
}
