using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuranSearchApp.Models;

public class QuranAya
{
    public int SurahNumber { get; set; }
    public int AyaNumber { get; set; }
    public string Text { get; set; } = string.Empty;
    public string FullLine { get; set; } = string.Empty;
    public string MatchedCase { get; set; } = string.Empty;
    public string MatchedText { get; set; } = string.Empty;
    public List<MatchPosition> MatchPositions { get; set; } = new();
    
    public override string ToString()
    {
        return $"{SurahNumber}:{AyaNumber} - {Text}";
    }
}

public class MatchPosition
{
    public int Start { get; set; }
    public int Length { get; set; }
    public string MatchedText { get; set; } = string.Empty;
}
