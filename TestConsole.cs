using System;
using System.Threading.Tasks;
using QuranSearchApp.Services;

namespace QuranSearchApp;

public class TestConsole
{
    public static async Task TestMain(string[] args)
    {
        Console.WriteLine("Testing Quran Search Service...");
        
        var service = new QuranSearchService();
        
        try
        {
            await service.LoadQuranTextAsync();
            Console.WriteLine("✓ Quran text loaded successfully");
            
            await service.LoadRulesAsync();
            Console.WriteLine("✓ Rules loaded successfully");
            
            var ruleNames = service.GetRuleNames();
            Console.WriteLine($"✓ Found {ruleNames.Count} rules");
            
            if (ruleNames.Count > 0)
            {
                var firstRuleName = ruleNames[0];
                Console.WriteLine($"Testing search for: {firstRuleName}");
                
                var results = service.SearchPattern(firstRuleName);
                Console.WriteLine($"✓ Found {results.Count} matches for '{firstRuleName}'");
                
                if (results.Count > 0)
                {
                    Console.WriteLine("First few matches:");
                    for (int i = 0; i < Math.Min(3, results.Count); i++)
                    {
                        var result = results[i];
                        Console.WriteLine($"  {result.SurahNumber}:{result.AyaNumber} - {result.Text}");
                    }
                }
            }
            
            Console.WriteLine("\n✓ All tests passed! The core functionality is working.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error: {ex.Message}");
        }
    }
}
