using System;
using System.Text;

class UnicodeTestProgram
{
    static void TestMain()
    {
        // Test the specific characters you mentioned
        var testChars = new[] { 'ۘ', 'ۛ' };
        var allStopSigns = new[] { 'ۚ', 'ۖ', 'ۗ', 'ۙ', 'ۘ', 'ۛ' };
        
        Console.WriteLine("=== Unicode Character Test ===");
        Console.OutputEncoding = Encoding.UTF8;
        
        // Test individual characters
        foreach (var ch in testChars)
        {
            Console.WriteLine($"Character: {ch}");
            Console.WriteLine($"Unicode Code Point: U+{((int)ch):X4}");
            Console.WriteLine($"Char.IsControl: {char.IsControl(ch)}");
            Console.WriteLine($"Char.IsSymbol: {char.IsSymbol(ch)}");
            Console.WriteLine($"Char.GetUnicodeCategory: {char.GetUnicodeCategory(ch)}");
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
                
                // Simulate the exact highlighting logic from HighlightedTextBlock
                int originalStart = index;
                int originalLength = 1;
                
                // Apply the same logic as in your highlighting code
                int adjustedStart = originalStart;
                int adjustedLength = originalLength;
                
                if (originalLength == 1)
                {
                    adjustedLength = 3;
                    if (adjustedStart > 0)
                        adjustedStart--;
                }
                
                // Ensure we don't go out of bounds
                adjustedLength = Math.Min(adjustedLength, testText.Length - adjustedStart);
                
                string extracted = testText.Substring(adjustedStart, adjustedLength);
                Console.WriteLine($"Original position: start={originalStart}, length={originalLength}");
                Console.WriteLine($"Adjusted position: start={adjustedStart}, length={adjustedLength}");
                Console.WriteLine($"Extracted text: '{extracted}'");
                
                // Verify the character is still there
                bool stillContains = extracted.Contains(stopSign);
                Console.WriteLine($"Character preserved: {stillContains}");
                
                // Check each character in the substring
                Console.Write("Characters in substring: ");
                for (int i = 0; i < extracted.Length; i++)
                {
                    char c = extracted[i];
                    Console.Write($"[{i}]='{c}'(U+{((int)c):X4}) ");
                }
                Console.WriteLine();
            }
        }
        
        Console.WriteLine("\n=== Testing potential corruption scenarios ===");
        
        // Test if substring operations corrupt the characters
        foreach (var stopSign in testChars)
        {
            string singleChar = stopSign.ToString();
            string withContext = $"ا{stopSign}ب"; // Arabic letter + stop sign + Arabic letter
            
            Console.WriteLine($"\nTesting character: {stopSign} (U+{((int)stopSign):X4})");
            Console.WriteLine($"Single char string: '{singleChar}' (length: {singleChar.Length})");
            Console.WriteLine($"With context: '{withContext}' (length: {withContext.Length})");
            
            // Test substring operations
            if (withContext.Length >= 3)
            {
                string sub1 = withContext.Substring(0, 1); // First char
                string sub2 = withContext.Substring(1, 1); // Stop sign
                string sub3 = withContext.Substring(2, 1); // Last char
                string sub123 = withContext.Substring(0, 3); // All three
                
                Console.WriteLine($"Substring(0,1): '{sub1}' contains stop sign: {sub1.Contains(stopSign)}");
                Console.WriteLine($"Substring(1,1): '{sub2}' contains stop sign: {sub2.Contains(stopSign)}");
                Console.WriteLine($"Substring(2,1): '{sub3}' contains stop sign: {sub3.Contains(stopSign)}");
                Console.WriteLine($"Substring(0,3): '{sub123}' contains stop sign: {sub123.Contains(stopSign)}");
            }
        }
    }
}
