using System;
using System.Threading.Tasks;

namespace QuranSearch.Core.Interfaces;

/// <summary>
/// Platform-agnostic audio service interface
/// Desktop and Android will provide their own implementations
/// </summary>
public interface IAudioService
{
    // Properties
    bool IsPlaying { get; }
    bool IsRepeating { get; }
    bool UseRemoteSource { get; set; }
    
    // Events
    event EventHandler<bool>? PlaybackStateChanged;
    event EventHandler<string>? PlaybackError;
    event EventHandler? SequencePlaybackEnded;
    
    // Methods
    bool AudioFileExists(int surahNumber, int ayaNumber);
    Task PlayAyaAsync(int surahNumber, int ayaNumber);
    Task StopAsync();
    void SetRepeatMode(bool repeat);
}
