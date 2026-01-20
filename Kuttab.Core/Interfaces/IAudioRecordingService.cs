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
    
    // Events
    event EventHandler<bool>? RecordingStateChanged;
    event EventHandler<string>? RecordingError;
    event EventHandler<TimeSpan>? RecordingDurationChanged;
    
    // Methods
    Task<bool> StartRecordingAsync(string filePath);
    Task<string?> StopRecordingAsync();
    Task<bool> PlayRecordingAsync();
    Task StopPlaybackAsync();
    Task DeleteRecordingAsync();
    
    // Permission check
    Task<bool> RequestRecordingPermissionAsync();
    bool HasRecordingPermission();
}
