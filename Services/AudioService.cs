using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace QuranSearchApp.Services
{
    public class AudioService
    {
        private readonly string _audioBasePath;
        private Process? _audioProcess;
        private bool _isPlaying;
        private bool _isRepeating;
        private string? _currentAudioFile;
        private bool _shouldStop;

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
        /// Gets the audio file path for a specific Sura and Aya
        /// </summary>
        /// <param name="suraNumber">Sura number (1-114)</param>
        /// <param name="ayaNumber">Aya number</param>
        /// <returns>Full path to the MP3 file</returns>
        public string GetAudioFilePath(int suraNumber, int ayaNumber)
        {
            // Format: SSSAAA.mp3 (6 digits, no underscore, zero-padded)
            string fileName = $"{suraNumber:D3}{ayaNumber:D3}.mp3";
            return Path.Combine(_audioBasePath, fileName);
        }

        /// <summary>
        /// Checks if an audio file exists for the given Sura and Aya
        /// </summary>
        public bool AudioFileExists(int suraNumber, int ayaNumber)
        {
            string filePath = GetAudioFilePath(suraNumber, ayaNumber);
            return File.Exists(filePath);
        }

        /// <summary>
        /// Plays audio for a specific Aya
        /// </summary>
        public async Task PlayAyaAsync(int suraNumber, int ayaNumber)
        {
            try
            {
                string filePath = GetAudioFilePath(suraNumber, ayaNumber);
                
                if (!File.Exists(filePath))
                {
                    PlaybackError?.Invoke(this, $"Audio file not found: {Path.GetFileName(filePath)}");
                    return;
                }

                await StopAsync();

                _currentAudioFile = filePath;
                await StartAudioPlaybackAsync(filePath);
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
        }

        public void Dispose()
        {
            StopAsync().Wait();
        }
    }
}
