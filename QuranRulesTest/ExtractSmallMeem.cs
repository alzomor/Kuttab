using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace QuranRulesTest
{
    class ExtractSmallMeem
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Extracting words with small meem below (ۭ)...");
            
            var lines = await File.ReadAllLinesAsync("/home/hossam-alzomor/Work/Kuttab/quran-uthmani.txt");
            var results = new List<SmallMeemWord>();
            
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                
                var parts = line.Split('|');
                if (parts.Length < 3) continue;
                
                if (!int.TryParse(parts[0], out int surah) || !int.TryParse(parts[1], out int aya))
                    continue;
                
                var text = parts[2];
                
                // Find all words containing small meem below (ۭ)
                var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                foreach (var word in words)
                {
                    if (word.Contains("ۭ"))
                    {
                        results.Add(new SmallMeemWord
                        {
                            SurahNumber = surah,
                            AyaNumber = aya,
                            WordWithSmallMeem = word
                        });
                    }
                }
            }
            
            // Write to CSV
            var outputPath = "/home/hossam-alzomor/Work/Kuttab/small_meem_words.csv";
            using var writer = new StreamWriter(outputPath);
            await writer.WriteLineAsync("Surah,Ayah,WordWithSmallMeem");
            
            foreach (var item in results)
            {
                await writer.WriteLineAsync($"{item.SurahNumber},{item.AyaNumber},\"{item.WordWithSmallMeem}\"");
            }
            
            Console.WriteLine($"Found {results.Count} words with small meem below");
            Console.WriteLine($"Output written to: {outputPath}");
        }
    }
    
    class SmallMeemWord
    {
        public int SurahNumber { get; set; }
        public int AyaNumber { get; set; }
        public string WordWithSmallMeem { get; set; } = string.Empty;
    }
}
