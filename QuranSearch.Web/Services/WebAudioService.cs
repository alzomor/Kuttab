using QuranSearch.Core.Services;

namespace QuranSearch.Web.Services;

public class WebAudioService : IAudioService
{
    private bool _isPlaying;
    private bool _isRepeating;

    public bool IsPlaying => _isPlaying;
    public bool IsRepeating => _isRepeating;

    public event EventHandler<AudioStatusEventArgs>? AudioStatusChanged;

    public async Task<bool> PlayAudioAsync(string audioFilePath)
    {
        try
        {
            await StopAudioAsync();
            
            // For web, we'll use JavaScript interop to play audio
            // This is a placeholder implementation
            _isPlaying = true;
            _isRepeating = false;
            OnAudioStatusChanged(true, false, audioFilePath, "Playing audio");

            // Simulate audio playback duration
            _ = Task.Run(async () =>
            {
                await Task.Delay(3000); // Simulate 3 second audio
                _isPlaying = false;
                OnAudioStatusChanged(false, false, null, "Audio finished");
            });

            return true;
        }
        catch (Exception ex)
        {
            OnAudioStatusChanged(false, false, null, $"Error playing audio: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> PlayAudioRepeatAsync(string audioFilePath)
    {
        try
        {
            await StopAudioAsync();
            
            _isPlaying = true;
            _isRepeating = true;
            OnAudioStatusChanged(true, true, audioFilePath, "Playing audio on repeat");

            // For web implementation, this would use JavaScript audio APIs
            // This is a placeholder
            return true;
        }
        catch (Exception ex)
        {
            OnAudioStatusChanged(false, false, null, $"Error playing audio on repeat: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> PlaySequenceAsync(IEnumerable<string> audioFilePaths)
    {
        try
        {
            await StopAudioAsync();
            
            var validPaths = audioFilePaths.ToList();
            if (!validPaths.Any())
            {
                OnAudioStatusChanged(false, false, null, "No audio files to play");
                return false;
            }

            _isPlaying = true;
            _isRepeating = false;
            OnAudioStatusChanged(true, false, null, "Playing sequence");

            // For web implementation, this would sequence through audio files
            // This is a placeholder
            return true;
        }
        catch (Exception ex)
        {
            OnAudioStatusChanged(false, false, null, $"Error playing sequence: {ex.Message}");
            return false;
        }
    }

    public async Task StopAudioAsync()
    {
        try
        {
            _isPlaying = false;
            _isRepeating = false;
            OnAudioStatusChanged(false, false, null, "Audio stopped");
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            OnAudioStatusChanged(false, false, null, $"Error stopping audio: {ex.Message}");
        }
    }

    private void OnAudioStatusChanged(bool isPlaying, bool isRepeating, string? currentFile, string? statusMessage)
    {
        AudioStatusChanged?.Invoke(this, new AudioStatusEventArgs
        {
            IsPlaying = isPlaying,
            IsRepeating = isRepeating,
            CurrentFile = currentFile,
            StatusMessage = statusMessage
        });
    }
}
