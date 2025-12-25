using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace QuranRulesTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Testing Tajweed rules on both Quran text files...");
            
            // Load both files
            var originalText = await LoadQuranText("/home/hossam-alzomor/Work/Kuttab/quran-uthmani.txt");
            var cleanText = await LoadQuranText("/home/hossam-alzomor/Work/Kuttab/quran-uthmani-ver1.2.txt");
            
            // Load rules
            var rules = await LoadRules("/home/hossam-alzomor/Work/Kuttab/rules.json");
            
            Console.WriteLine($"Loaded {originalText.Count} ayahs from original file");
            Console.WriteLine($"Loaded {cleanText.Count} ayahs from clean file");
            Console.WriteLine($"Loaded {rules.Count} rules");
            
            // Apply rules to both files
            var originalResults = await ApplyRules(originalText, rules);
            var cleanResults = await ApplyRules(cleanText, rules);
            
            Console.WriteLine($"Found {originalResults.Count} matches in original file");
            Console.WriteLine($"Found {cleanResults.Count} matches in clean file");
            
            // Print Iqlab rule counts specifically
            var iqlab = "إقلاب النون الساكنة والتنوين";
            var originalIqlabCount = originalResults.GetValueOrDefault(iqlab, new List<RuleMatch>()).Count;
            var cleanIqlabCount = cleanResults.GetValueOrDefault(iqlab, new List<RuleMatch>()).Count;
            Console.WriteLine($"Iqlab matches in original: {originalIqlabCount}");
            Console.WriteLine($"Iqlab matches in clean: {cleanIqlabCount}");
            
            // Print الوقف اللازم rule counts
            var waqfLazim = "الوقف اللازم";
            var originalWaqfCount = originalResults.GetValueOrDefault(waqfLazim, new List<RuleMatch>()).Count;
            var cleanWaqfCount = cleanResults.GetValueOrDefault(waqfLazim, new List<RuleMatch>()).Count;
            Console.WriteLine($"Waqf Lazim matches in original: {originalWaqfCount}");
            Console.WriteLine($"Waqf Lazim matches in clean: {cleanWaqfCount}");
            
            // Compare results
            var differences = CompareResults(originalResults, cleanResults);
            
            // Generate CSV report
            await GenerateCsvReport(differences, "/home/hossam-alzomor/Work/Kuttab/quran_rules_comparison.csv");
            
            Console.WriteLine($"Report generated with {differences.Count} differences");
            Console.WriteLine("Done!");
        }
        
        static async Task<List<QuranAya>> LoadQuranText(string filePath)
        {
            var ayahs = new List<QuranAya>();
            var lines = await File.ReadAllLinesAsync(filePath);
            
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                
                var parts = line.Split('|');
                if (parts.Length >= 3)
                {
                    if (int.TryParse(parts[0], out int surah) && int.TryParse(parts[1], out int aya))
                    {
                        ayahs.Add(new QuranAya
                        {
                            SurahNumber = surah,
                            AyaNumber = aya,
                            Text = parts[2],
                            FullLine = line
                        });
                    }
                }
            }
            
            return ayahs;
        }
        
        static async Task<List<TajweedRule>> LoadRules(string filePath)
        {
            var jsonContent = await File.ReadAllTextAsync(filePath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var rulesContainer = JsonSerializer.Deserialize<RulesContainer>(jsonContent, options);
            return rulesContainer?.Rules ?? new List<TajweedRule>();
        }
        
        static async Task<Dictionary<string, List<RuleMatch>>> ApplyRules(List<QuranAya> quranText, List<TajweedRule> rules)
        {
            var results = new Dictionary<string, List<RuleMatch>>();
            
            foreach (var rule in rules)
            {
                var ruleMatches = new List<RuleMatch>();
                
                foreach (var aya in quranText)
                {
                    foreach (var ruleCase in rule.Cases)
                    {
                        try
                        {
                            var regex = new Regex(ruleCase.Regex, RegexOptions.CultureInvariant);
                            var regexMatches = regex.Matches(aya.Text);
                            
                            foreach (Match match in regexMatches)
                            {
                                ruleMatches.Add(new RuleMatch
                                {
                                    SurahNumber = aya.SurahNumber,
                                    AyaNumber = aya.AyaNumber,
                                    RuleName = rule.Name,
                                    CaseDescription = ruleCase.Description,
                                    MatchedText = match.Value,
                                    StartIndex = match.Index,
                                    EndIndex = match.Index + match.Length
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error applying rule {rule.Name}: {ex.Message}");
                        }
                    }
                }
                
                results[rule.Name] = ruleMatches;
            }
            
            return results;
        }
        
        static List<RuleDifference> CompareResults(
            Dictionary<string, List<RuleMatch>> originalResults,
            Dictionary<string, List<RuleMatch>> cleanResults)
        {
            var differences = new List<RuleDifference>();
            
            // Get all unique keys from both results
            var allRuleNames = originalResults.Keys.Union(cleanResults.Keys).ToList();
            
            foreach (var ruleName in allRuleNames)
            {
                var originalMatches = originalResults.GetValueOrDefault(ruleName, new List<RuleMatch>());
                var cleanMatches = cleanResults.GetValueOrDefault(ruleName, new List<RuleMatch>());
                
                // Group matches by Surah and Aya for comparison
                var originalByAya = originalMatches.GroupBy(m => $"{m.SurahNumber}:{m.AyaNumber}")
                                                  .ToDictionary(g => g.Key, g => g.ToList());
                var cleanByAya = cleanMatches.GroupBy(m => $"{m.SurahNumber}:{m.AyaNumber}")
                                            .ToDictionary(g => g.Key, g => g.ToList());
                
                // Get all unique ayahs
                var allAyahs = originalByAya.Keys.Union(cleanByAya.Keys).ToList();
                
                foreach (var ayaKey in allAyahs)
                {
                    var originalAyaMatches = originalByAya.GetValueOrDefault(ayaKey, new List<RuleMatch>());
                    var cleanAyaMatches = cleanByAya.GetValueOrDefault(ayaKey, new List<RuleMatch>());
                    
                    // Normalize matched text by removing small meem below (ۭ)
                    // Count occurrences of each normalized text
                    var originalNormalizedCounts = originalAyaMatches
                        .Select(m => m.MatchedText.Replace("ۭ", ""))
                        .GroupBy(t => t)
                        .ToDictionary(g => g.Key, g => g.Count());
                    
                    var cleanNormalizedCounts = cleanAyaMatches
                        .Select(m => m.MatchedText)
                        .GroupBy(t => t)
                        .ToDictionary(g => g.Key, g => g.Count());
                    
                    // Get all unique normalized texts
                    var allNormalizedTexts = originalNormalizedCounts.Keys.Union(cleanNormalizedCounts.Keys).ToList();
                    
                    foreach (var normalizedText in allNormalizedTexts)
                    {
                        var originalCount = originalNormalizedCounts.GetValueOrDefault(normalizedText, 0);
                        var cleanCount = cleanNormalizedCounts.GetValueOrDefault(normalizedText, 0);
                        
                        var parts = ayaKey.Split(':');
                        var surah = int.Parse(parts[0]);
                        var aya = int.Parse(parts[1]);
                        
                        if (originalCount > cleanCount)
                        {
                            // More matches in original than clean
                            for (int i = 0; i < originalCount - cleanCount; i++)
                            {
                                // Find the original text (with ۭ if present)
                                var originalText = originalAyaMatches
                                    .FirstOrDefault(m => m.MatchedText.Replace("ۭ", "") == normalizedText)?.MatchedText ?? normalizedText;
                                
                                differences.Add(new RuleDifference
                                {
                                    SurahNumber = surah,
                                    AyaNumber = aya,
                                    RuleName = ruleName,
                                    FoundInOriginal = originalText,
                                    FoundInClean = ""
                                });
                            }
                        }
                        else if (cleanCount > originalCount)
                        {
                            // More matches in clean than original
                            for (int i = 0; i < cleanCount - originalCount; i++)
                            {
                                differences.Add(new RuleDifference
                                {
                                    SurahNumber = surah,
                                    AyaNumber = aya,
                                    RuleName = ruleName,
                                    FoundInOriginal = "",
                                    FoundInClean = normalizedText
                                });
                            }
                        }
                        // If counts are equal, no difference to report
                    }
                }
            }
            
            return differences.OrderBy(d => d.SurahNumber)
                           .ThenBy(d => d.AyaNumber)
                           .ThenBy(d => d.RuleName)
                           .ToList();
        }
        
        static async Task GenerateCsvReport(List<RuleDifference> differences, string outputPath)
        {
            using var writer = new StreamWriter(outputPath);
            
            // Write header
            await writer.WriteLineAsync("Surah,Ayah,Rule,FoundInOriginal,FoundInClean");
            
            // Write data
            foreach (var diff in differences)
            {
                var line = $"{diff.SurahNumber},{diff.AyaNumber},\"{diff.RuleName}\",\"{diff.FoundInOriginal}\",\"{diff.FoundInClean}\"";
                await writer.WriteLineAsync(line);
            }
        }
    }
    
    public class QuranAya
    {
        public int SurahNumber { get; set; }
        public int AyaNumber { get; set; }
        public string Text { get; set; } = string.Empty;
        public string FullLine { get; set; } = string.Empty;
    }
    
    public class TajweedRule
    {
        public string Name { get; set; } = string.Empty;
        public string Group { get; set; } = string.Empty;
        public List<TajweedCase> Cases { get; set; } = new();
    }
    
    public class TajweedCase
    {
        public string Description { get; set; } = string.Empty;
        public string Regex { get; set; } = string.Empty;
    }
    
    public class RulesContainer
    {
        public List<TajweedRule> Rules { get; set; } = new();
    }
    
    public class RuleMatch
    {
        public int SurahNumber { get; set; }
        public int AyaNumber { get; set; }
        public string RuleName { get; set; } = string.Empty;
        public string CaseDescription { get; set; } = string.Empty;
        public string MatchedText { get; set; } = string.Empty;
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
    }
    
    public class RuleDifference
    {
        public int SurahNumber { get; set; }
        public int AyaNumber { get; set; }
        public string RuleName { get; set; } = string.Empty;
        public string FoundInOriginal { get; set; } = string.Empty;
        public string FoundInClean { get; set; } = string.Empty;
    }
}
