using System;
using System.Text;

namespace QuranSearchApp
{
    public class UnicodeTest
    {
        public static void TestUnicodePreservation()
        {
            // Test the specific characters you mentioned
            var testChars = new[] { 'ۘ', 'ۛ' };
            var allStopSigns = new[] { 'ۚ', 'ۖ', 'ۗ', 'ۙ', 'ۘ', 'ۛ' };
            
            Console.WriteLine("=== Unicode Character Test ===");
            
            // Test individual characters
            foreach (var ch in testChars)
            {
                Console.WriteLine($"Character: {ch}");
                Console.WriteLine($"Unicode Code Point: U+{((int)ch):X4}");
                Console.WriteLine($"UTF-16 Encoding: {Encoding.Unicode.GetBytes(ch.ToString()).Length} bytes");
                Console.WriteLine($"UTF-8 Encoding: {Encoding.UTF8.GetBytes(ch.ToString()).Length} bytes");
                Console.WriteLine();
            }
            
            // Test string operations that happen in highlighting
            string testText = "بِسْمِ اللَّهِ الرَّحْمَٰنِ الرَّحِيمِۘ الْحَمْدُ لِلَّهِ رَبِّ الْعَالَمِينَۛ";
            Console.WriteLine($"Original text: {testText}");
            Console.WriteLine($"Text length: {testText.Length}");
            
            // Find positions of the characters
            foreach (var stopSign in allStopSigns)
            {
                int index = testText.IndexOf(stopSign);
                if (index >= 0)
                {
                    Console.WriteLine($"\nFound '{stopSign}' at position {index}");
                    
                    // Test substring operations like in the highlighting code
                    int start = Math.Max(0, index - 1);
                    int length = Math.Min(3, testText.Length - start);
                    
                    string extracted = testText.Substring(start, length);
                    Console.WriteLine($"Substring({start}, {length}): '{extracted}'");
                    
                    // Verify the character is still there
                    bool stillContains = extracted.Contains(stopSign);
                    Console.WriteLine($"Character preserved: {stillContains}");
                    
                    // Check each character in the substring
                    Console.Write("Characters in substring: ");
                    foreach (char c in extracted)
                    {
                        Console.Write($"'{c}' (U+{((int)c):X4}) ");
                    }
                    Console.WriteLine();
                }
            }
        }
    }
}
