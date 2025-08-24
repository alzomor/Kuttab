using QuranSearch.Core.Services;
using System.Diagnostics;

namespace QuranSearch.MAUI.Services;

public class PlatformAudioService : IAudioService
{
    private Process? _currentProcess;
    private bool _isPlaying;
    private bool _isRepeating;
    private CancellationTokenSource? _cancellationTokenSource;

    public bool IsPlaying => _isPlaying;
    public bool IsRepeating => _isRepeating;

    public event EventHandler<AudioStatusEventArgs>? AudioStatusChanged;

    public async Task<bool> PlayAudioAsync(string audioFilePath)
    {
        try
        {
            await StopAudioAsync();
            
            if (!File.Exists(audioFilePath))
            {
                OnAudioStatusChanged(false, false, null, $"Audio file not found: {audioFilePath}");
                return false;
            }

            _currentProcess = CreateAudioProcess(audioFilePath);
            _currentProcess.Start();
            
            _isPlaying = true;
            _isRepeating = false;
            OnAudioStatusChanged(true, false, audioFilePath, "Playing audio");

            // Monitor process completion
            _ = Task.Run(async () =>
            {
                await _currentProcess.WaitForExitAsync();
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
            
            if (!File.Exists(audioFilePath))
            {
                OnAudioStatusChanged(false, false, null, $"Audio file not found: {audioFilePath}");
                return false;
            }

            _isPlaying = true;
            _isRepeating = true;
            _cancellationTokenSource = new CancellationTokenSource();
            
            OnAudioStatusChanged(true, true, audioFilePath, "Playing audio on repeat");

            // Start repeat loop
            _ = Task.Run(async () =>
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    try
                    {
                        _currentProcess = CreateAudioProcess(audioFilePath);
                        _currentProcess.Start();
                        await _currentProcess.WaitForExitAsync(_cancellationTokenSource.Token);
                        
                        if (_cancellationTokenSource.Token.IsCancellationRequested)
                            break;
                            
                        await Task.Delay(500, _cancellationTokenSource.Token); // Brief pause between repeats
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
                
                _isPlaying = false;
                _isRepeating = false;
                OnAudioStatusChanged(false, false, null, "Repeat stopped");
            });

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
            
            var validPaths = audioFilePaths.Where(File.Exists).ToList();
            if (!validPaths.Any())
            {
                OnAudioStatusChanged(false, false, null, "No valid audio files found");
                return false;
            }

            _isPlaying = true;
            _isRepeating = false;
            _cancellationTokenSource = new CancellationTokenSource();
            
            OnAudioStatusChanged(true, false, null, "Playing sequence");

            // Start sequence playback
            _ = Task.Run(async () =>
            {
                foreach (var audioPath in validPaths)
                {
                    if (_cancellationTokenSource.Token.IsCancellationRequested)
                        break;

                    try
                    {
                        OnAudioStatusChanged(true, false, audioPath, $"Playing: {Path.GetFileName(audioPath)}");
                        
                        _currentProcess = CreateAudioProcess(audioPath);
                        _currentProcess.Start();
                        await _currentProcess.WaitForExitAsync(_cancellationTokenSource.Token);
                        
                        if (_cancellationTokenSource.Token.IsCancellationRequested)
                            break;
                            
                        await Task.Delay(300, _cancellationTokenSource.Token); // Brief pause between files
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
                
                _isPlaying = false;
                OnAudioStatusChanged(false, false, null, "Sequence finished");
            });

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
            _cancellationTokenSource?.Cancel();
            
            if (_currentProcess != null && !_currentProcess.HasExited)
            {
                _currentProcess.Kill();
                await _currentProcess.WaitForExitAsync();
            }
            
            _currentProcess?.Dispose();
            _currentProcess = null;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
            
            _isPlaying = false;
            _isRepeating = false;
            
            OnAudioStatusChanged(false, false, null, "Audio stopped");
        }
        catch (Exception ex)
        {
            OnAudioStatusChanged(false, false, null, $"Error stopping audio: {ex.Message}");
        }
    }

    private Process CreateAudioProcess(string audioFilePath)
    {
        var process = new Process();
        
#if WINDOWS
        process.StartInfo.FileName = "powershell";
        process.StartInfo.Arguments = $"-Command \"Add-Type -AssemblyName presentationCore; $player = New-Object system.windows.media.mediaplayer; $player.open('{audioFilePath}'); $player.Play(); Start-Sleep -Seconds 10; $player.Stop()\"";
#elif ANDROID
        // For Android, we would use MediaPlayer through platform-specific implementation
        process.StartInfo.FileName = "am";
        process.StartInfo.Arguments = $"start -a android.intent.action.VIEW -d file://{audioFilePath} -t audio/mpeg";
#else
        // Linux/macOS
        if (OperatingSystem.IsLinux())
        {
            process.StartInfo.FileName = "paplay";
            process.StartInfo.Arguments = $"\"{audioFilePath}\"";
        }
        else if (OperatingSystem.IsMacOS())
        {
            process.StartInfo.FileName = "afplay";
            process.StartInfo.Arguments = $"\"{audioFilePath}\"";
        }
#endif

        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        
        return process;
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
