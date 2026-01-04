using System;
using System.IO;
using System.Threading.Tasks;

namespace Kuttab.Core.Interfaces;

/// <summary>
/// Platform-agnostic audio recording service interface
/// Allows users to record their recitation and play it back
/// </summary>
public interface IAudioRecordingService
{
    // Properties
    bool IsRecording { get; }
    bool HasRecording { get; }
    string? RecordingPath { get; }
    int? CurrentSurahNumber { get; }
    int? CurrentAyaNumber { get; }
    
    // Events
    event EventHandler<bool>? RecordingStateChanged;
    event EventHandler<string>? RecordingError;
    event EventHandler<TimeSpan>? RecordingDurationChanged;
    
    // Methods
    Task<bool> StartRecordingAsync(int surahNumber, int ayaNumber);
    Task<string?> StopRecordingAsync();
    Task<bool> PlayRecordingAsync(int surahNumber, int ayaNumber);
    Task StopPlaybackAsync();
    Task DeleteRecordingAsync(int surahNumber, int ayaNumber);
    bool HasRecordingForAya(int surahNumber, int ayaNumber);
    string GetRecordingPath(int surahNumber, int ayaNumber);
    
    // Permission check
    Task<bool> RequestRecordingPermissionAsync();
    bool HasRecordingPermission();
}
