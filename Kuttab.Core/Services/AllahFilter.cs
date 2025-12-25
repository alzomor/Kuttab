using System.Linq;

namespace Kuttab.Core.Services;

public static class AllahFilter
{
    public static bool IsLamMufakhkham(string text, int allahStartIndex)
    {
        // Check if the position is valid
        if (string.IsNullOrEmpty(text) || allahStartIndex < 0 || allahStartIndex >= text.Length)
            return false;

        // Special case: "تالله" is always Mufakhkham
        if (allahStartIndex > 0 && text[allahStartIndex - 1] == 'ت')
        {
            return true;
        }

        // Special cases that are always Muraqqaqah (light)
        if (allahStartIndex > 0)
        {
            // Check for "بالله" or "لله" which are always Muraqqaqah
            if ((text[allahStartIndex - 1] == 'ب') || // بالله
                (allahStartIndex > 1 && text[allahStartIndex - 1] == 'ل' && text[allahStartIndex - 2] == 'ل')) // لله
            {
                return false;
            }
        }

        // For other cases, find the start of the previous word
        int wordStart = allahStartIndex - 1;
        while (wordStart >= 0 && !char.IsWhiteSpace(text[wordStart]) && text[wordStart] != ' ')
        {
            wordStart--;
        }

        // Get the previous word
        string previousWord = wordStart >= 0 ? 
            text.Substring(wordStart, allahStartIndex - wordStart).Trim() : 
            string.Empty;

        if (string.IsNullOrEmpty(previousWord))
            return false;

        // Get the last character of the previous word
        char lastChar = previousWord[^1];

        // Check if the last character has Fatha (َ) or Damma (ُ)
        if (lastChar == 'َ' || lastChar == 'ُ')
        {
            return true;
        }

        // Check if the last character is one of the Tafkheem letters (خص ضغط قظ)
        string tafkheemLetters = "خَصْضَغْطٍقِظْ";
        if (tafkheemLetters.Contains(lastChar))
        {
            return true;
        }

        // Default to Muraqqaqah (light)
        return false;
    }
}
