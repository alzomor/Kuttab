namespace QuranSearch.Core.Models;

public class QuranAya
{
    public int SurahNumber { get; set; }
    public int AyaNumber { get; set; }
    public string Text { get; set; } = string.Empty;
    public string FullLine { get; set; } = string.Empty;
    public string? MatchedCase { get; set; }
    public string? MatchedText { get; set; }
    public List<MatchPosition>? MatchPositions { get; set; }
}

public class MatchPosition
{
    public int Start { get; set; }
    public int Length { get; set; }
    public string MatchedText { get; set; } = string.Empty;
}
