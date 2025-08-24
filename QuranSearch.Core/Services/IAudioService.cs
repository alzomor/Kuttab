namespace QuranSearch.Core.Services;

public interface IAudioService
{
    Task<bool> PlayAudioAsync(string audioFilePath);
    Task<bool> PlayAudioRepeatAsync(string audioFilePath);
    Task<bool> PlaySequenceAsync(IEnumerable<string> audioFilePaths);
    Task StopAudioAsync();
    bool IsPlaying { get; }
    bool IsRepeating { get; }
    
    event EventHandler<AudioStatusEventArgs>? AudioStatusChanged;
}

public class AudioStatusEventArgs : EventArgs
{
    public bool IsPlaying { get; set; }
    public bool IsRepeating { get; set; }
    public string? CurrentFile { get; set; }
    public string? StatusMessage { get; set; }
}
