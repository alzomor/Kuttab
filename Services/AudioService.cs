using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace QuranSearchApp.Services
{
    public class AudioService
    {
        private readonly string _audioBasePath;
        private readonly string _remoteAudioBaseUrl = "https://everyayah.com/data/Husary_128kbps";
        private readonly HttpClient _httpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(30) };
        private bool _useRemoteSource = false;
        private Process? _audioProcess;
        private bool _isPlaying;
        private bool _isRepeating;
        private string? _currentAudioFile;
        private bool _shouldStop;

        public bool UseRemoteSource
        {
            get => _useRemoteSource;
            set => _useRemoteSource = value;
        }

        public AudioService()
        {
            // Use the project directory structure
            var projectDir = AppDomain.CurrentDomain.BaseDirectory;
            
            // Navigate to the audio folder in the project
            _audioBasePath = Path.Combine(projectDir, "..", "..", "..", "..", "AL Husary");
            
            // If that doesn't work, try relative to current directory
            if (!Directory.Exists(_audioBasePath))
            {
                _audioBasePath = Path.Combine(Directory.GetCurrentDirectory(), "AL Husary");
            }
            
            // Normalize the path
            _audioBasePath = Path.GetFullPath(_audioBasePath);
        }

        public event EventHandler<bool>? PlaybackStateChanged;
        public event EventHandler<string>? PlaybackError;

        public bool IsPlaying => _isPlaying;
        public bool IsRepeating => _isRepeating;

        /// <summary>
        /// Gets the audio source (local file path or remote URL) for a specific Sura and Aya
        /// </summary>
        /// <param name="suraNumber">Sura number (1-114)</param>
        /// <param name="ayaNumber">Aya number</param>
        /// <returns>Full path or URL to the MP3 file</returns>
        public string GetAudioSource(int suraNumber, int ayaNumber)
        {
            // Format: SSSAAA.mp3 (6 digits, no underscore, zero-padded)
            string fileName = $"{suraNumber:D3}{ayaNumber:D3}.mp3";
            if (_useRemoteSource)
            {
                return $"{_remoteAudioBaseUrl.TrimEnd('/')}/{fileName}";
            }
            return Path.Combine(_audioBasePath, fileName);
        }

        /// <summary>
        /// Checks if an audio file exists for the given Sura and Aya
        /// </summary>
        public bool AudioFileExists(int suraNumber, int ayaNumber)
        {
            if (_useRemoteSource)
            {
                // Avoid blocking the UI with network I/O. Assume remote exists; errors will surface on playback.
                return true;
            }
            string filePath = GetAudioSource(suraNumber, ayaNumber);
            return File.Exists(filePath);
        }

        /// <summary>
        /// Plays audio for a specific Aya
        /// </summary>
        public async Task PlayAyaAsync(int suraNumber, int ayaNumber)
        {
            try
            {
                string source = GetAudioSource(suraNumber, ayaNumber);

                await StopAsync();

                string pathToPlay = source;
                string? tempFile = null;

                if (_useRemoteSource)
                {
                    // Download to a temp file, then play locally
                    try
                    {
                        tempFile = Path.Combine(Path.GetTempPath(), $"{suraNumber:D3}{ayaNumber:D3}_{Guid.NewGuid():N}.mp3");
                        using var response = await _httpClient.GetAsync(source, HttpCompletionOption.ResponseHeadersRead);
                        response.EnsureSuccessStatusCode();
                        await using (var fs = File.Create(tempFile))
                        {
                            await response.Content.CopyToAsync(fs);
                        }
                        pathToPlay = tempFile;
                    }
                    catch (Exception ex)
                    {
                        PlaybackError?.Invoke(this, $"Failed to download audio: {ex.Message}");
                        // Cleanup temp file if partially created
                        if (tempFile != null && File.Exists(tempFile))
                        {
                            try { File.Delete(tempFile); } catch { }
                        }
                        return;
                    }
                }
                else
                {
                    if (!File.Exists(pathToPlay))
                    {
                        PlaybackError?.Invoke(this, $"Audio file not found: {Path.GetFileName(pathToPlay)}");
                        return;
                    }
                }

                _currentAudioFile = pathToPlay;
                await StartAudioPlaybackAsync(pathToPlay);
            }
            catch (Exception ex)
            {
                PlaybackError?.Invoke(this, $"Error playing audio: {ex.Message}");
            }
        }

        private async Task StartAudioPlaybackAsync(string filePath)
        {
            try
            {
                _shouldStop = false;
                
                string command;
                string arguments;

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    // Use aplay or paplay for Linux
                    command = "paplay";
                    arguments = $"\"{filePath}\"";
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // Use Windows Media Player or PowerShell
                    command = "powershell";
                    arguments = $"-Command \"(New-Object Media.SoundPlayer '{filePath}').PlaySync()\"";
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    // Use afplay for macOS
                    command = "afplay";
                    arguments = $"\"{filePath}\"";
                }
                else
                {
                    PlaybackError?.Invoke(this, "Unsupported operating system for audio playback");
                    return;
                }

                _audioProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = command,
                        Arguments = arguments,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    }
                };

                _audioProcess.Exited += OnAudioProcessExited;
                _audioProcess.EnableRaisingEvents = true;

                _audioProcess.Start();
                _isPlaying = true;
                PlaybackStateChanged?.Invoke(this, true);

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                PlaybackError?.Invoke(this, $"Error starting audio playback: {ex.Message}");
            }
        }

        /// <summary>
        /// Stops current playback
        /// </summary>
        public async Task StopAsync()
        {
            _shouldStop = true;
            _isRepeating = false;

            if (_audioProcess != null && !_audioProcess.HasExited)
            {
                try
                {
                    _audioProcess.Kill();
                    _audioProcess.WaitForExit(1000);
                }
                catch (Exception ex)
                {
                    PlaybackError?.Invoke(this, $"Error stopping audio: {ex.Message}");
                }
                finally
                {
                    _audioProcess?.Dispose();
                    _audioProcess = null;
                }
            }

            _isPlaying = false;
            PlaybackStateChanged?.Invoke(this, false);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Pauses current playback (Note: not all system players support pause)
        /// </summary>
        public void Pause()
        {
            // System audio players typically don't support pause, so we stop instead
            _ = StopAsync();
        }

        /// <summary>
        /// Resumes paused playback (restarts the audio)
        /// </summary>
        public void Resume()
        {
            if (!_isPlaying && !string.IsNullOrEmpty(_currentAudioFile))
            {
                _ = StartAudioPlaybackAsync(_currentAudioFile);
            }
        }

        /// <summary>
        /// Toggles repeat mode for current Aya
        /// </summary>
        public void SetRepeatMode(bool repeat)
        {
            _isRepeating = repeat;
        }

        private void OnAudioProcessExited(object? sender, EventArgs e)
        {
            _isPlaying = false;
            PlaybackStateChanged?.Invoke(this, false);

            // Only restart if we're in repeat mode AND haven't been told to stop
            if (_isRepeating && !_shouldStop && !string.IsNullOrEmpty(_currentAudioFile))
            {
                // Restart the same audio after a short delay
                Task.Delay(100).ContinueWith(_ => StartAudioPlaybackAsync(_currentAudioFile));
            }

            // If the current file is a temp file (remote download), try to delete it
            try
            {
                if (!string.IsNullOrEmpty(_currentAudioFile))
                {
                    var tempPath = Path.GetFullPath(Path.GetTempPath());
                    var playedPath = Path.GetFullPath(_currentAudioFile);
                    if (playedPath.StartsWith(tempPath, StringComparison.OrdinalIgnoreCase) && File.Exists(playedPath))
                    {
                        try { File.Delete(playedPath); } catch { /* ignore */ }
                    }
                }
            }
            catch { /* ignore */ }
        }

        public void Dispose()
        {
            StopAsync().Wait();
        }
    }
}

