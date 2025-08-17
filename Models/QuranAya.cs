namespace QuranSearchApp.Models;

public class QuranAya
{
    public int SurahNumber { get; set; }
    public int AyaNumber { get; set; }
    public string Text { get; set; } = string.Empty;
    public string FullLine { get; set; } = string.Empty;
    public string MatchedCase { get; set; } = string.Empty;
    public string MatchedText { get; set; } = string.Empty;
    
    public override string ToString()
    {
        return $"{SurahNumber}:{AyaNumber} - {Text}";
    }
}
