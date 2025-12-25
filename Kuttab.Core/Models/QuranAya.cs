using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Kuttab.Core.Models;

public class QuranAya : INotifyPropertyChanged
{
    private bool _isCurrentlyPlaying;
    
    public int SurahNumber { get; set; }
    public int AyaNumber { get; set; }
    public string Text { get; set; } = string.Empty;
    public string FullLine { get; set; } = string.Empty;
    public string MatchedCase { get; set; } = string.Empty;
    public string MatchedText { get; set; } = string.Empty;
    public List<MatchPosition> MatchPositions { get; set; } = new();

    public string SurahName => SurahInfo.GetSurahName(SurahNumber);
    
    public bool IsCurrentlyPlaying
    {
        get => _isCurrentlyPlaying;
        set
        {
            if (_isCurrentlyPlaying != value)
            {
                _isCurrentlyPlaying = value;
                OnPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
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
