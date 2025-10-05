using System;
using System.Collections.Generic;
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
            // Look for audio files in the same directory as the executable
            var exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
            _audioBasePath = Path.Combine(exeDirectory, "AL Husary");
            
            // Log the path for debugging
            Console.WriteLine($"[AudioService] Looking for audio files in: {_audioBasePath}");
            Console.WriteLine($"[AudioService] Directory exists: {Directory.Exists(_audioBasePath)}");
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
                await StopAsync();

                string pathToPlay;

                if (_useRemoteSource)
                {
                    // Try to get from local cache first, download if needed
                    pathToPlay = await GetOrDownloadAudioFileAsync(suraNumber, ayaNumber);
                    if (string.IsNullOrEmpty(pathToPlay))
                    {
                        PlaybackError?.Invoke(this, $"Failed to get audio for Aya {suraNumber}:{ayaNumber}");
                        return;
                    }
                }
                else
                {
                    // Use local file directly
                    pathToPlay = GetAudioSource(suraNumber, ayaNumber);
                    if (!File.Exists(pathToPlay))
                    {
                        PlaybackError?.Invoke(this, $"Audio file not found: {Path.GetFileName(pathToPlay)}");
                        return;
                    }
                }

                _currentAudioFile = pathToPlay;
                
                // Set playing state immediately before starting playback
                _isPlaying = true;
                PlaybackStateChanged?.Invoke(this, true);
                
                await StartAudioPlaybackAsync(pathToPlay);
            }
            catch (Exception ex)
            {
                // Ensure we clear the playing state on error
                _isPlaying = false;
                PlaybackStateChanged?.Invoke(this, false);
                PlaybackError?.Invoke(this, $"Error playing audio: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets audio file from cache or downloads it if not cached
        /// </summary>
        private async Task<string?> GetOrDownloadAudioFileAsync(int suraNumber, int ayaNumber)
        {
            try
            {
                // Ensure cache directory exists
                if (!Directory.Exists(_audioBasePath))
                {
                    Console.WriteLine($"[AudioService] Creating cache directory: {_audioBasePath}");
                    Directory.CreateDirectory(_audioBasePath);
                }

                // Check if file exists in cache
                string fileName = $"{suraNumber:D3}{ayaNumber:D3}.mp3";
                string cachedFilePath = Path.Combine(_audioBasePath, fileName);

                if (File.Exists(cachedFilePath))
                {
                    Console.WriteLine($"[AudioService] Using cached file: {cachedFilePath}");
                    return cachedFilePath;
                }

                // Download to cache
                string remoteUrl = $"{_remoteAudioBaseUrl.TrimEnd('/')}/{fileName}";
                Console.WriteLine($"[AudioService] Downloading from: {remoteUrl}");
                Console.WriteLine($"[AudioService] Caching to: {cachedFilePath}");

                using var response = await _httpClient.GetAsync(remoteUrl, HttpCompletionOption.ResponseHeadersRead);
                Console.WriteLine($"[AudioService] Response status: {response.StatusCode}");
                response.EnsureSuccessStatusCode();

                await using (var fs = File.Create(cachedFilePath))
                {
                    await response.Content.CopyToAsync(fs);
                }

                var fileInfo = new FileInfo(cachedFilePath);
                Console.WriteLine($"[AudioService] Downloaded and cached file size: {fileInfo.Length} bytes");

                return cachedFilePath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AudioService] Failed to get or download audio: {ex.Message}");
                return null;
            }
        }

        private async Task StartAudioPlaybackAsync(string filePath)
        {
            try
            {
                _shouldStop = false;
                
                string? command = null;
                string arguments = "";
                List<string> errors = new List<string>();

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    // Try multiple players in order of preference for Linux
                    string[] linuxPlayers = { "mpg123", "paplay", "ffplay", "aplay" };
                    
                    foreach (var player in linuxPlayers)
                    {
                        if (IsCommandAvailable(player))
                        {
                            command = player;
                            arguments = player switch
                            {
                                "mpg123" => $"-q \"{filePath}\"",  // -q for quiet mode
                                "paplay" => $"\"{filePath}\"",
                                "ffplay" => $"-nodisp -autoexit -loglevel quiet \"{filePath}\"",
                                "aplay" => $"\"{filePath}\"",
                                _ => $"\"{filePath}\""
                            };
                            Console.WriteLine($"[AudioService] Using audio player: {command}");
                            break;
                        }
                        else
                        {
                            errors.Add($"{player} not available");
                        }
                    }
                    
                    if (command == null)
                    {
                        PlaybackError?.Invoke(this, $"No audio player found on Linux. Tried: {string.Join(", ", linuxPlayers)}");
                        return;
                    }
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // Try multiple options for Windows
                    // First try PowerShell with MediaPlayer (supports MP3)
                    command = "powershell";
                    arguments = $"-Command \"Add-Type -AssemblyName PresentationCore; $player = New-Object System.Windows.Media.MediaPlayer; $player.Open('{filePath}'); $player.Play(); while($player.NaturalDuration.TimeSpan.TotalSeconds -eq 0){{Start-Sleep -Milliseconds 100}}; Start-Sleep -Seconds $player.NaturalDuration.TimeSpan.TotalSeconds\"";
                    Console.WriteLine($"[AudioService] Using PowerShell MediaPlayer for Windows");
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    // Use afplay for macOS (built-in)
                    command = "afplay";
                    arguments = $"\"{filePath}\"";
                    Console.WriteLine($"[AudioService] Using afplay for macOS");
                }
                else
                {
                    PlaybackError?.Invoke(this, "Unsupported operating system for audio playback");
                    return;
                }

                Console.WriteLine($"[AudioService] Starting playback: {command} {arguments}");
                Console.WriteLine($"[AudioService] File path: {filePath}");
                Console.WriteLine($"[AudioService] File exists: {File.Exists(filePath)}");

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
                
                // Read error output in case of issues
                var errorOutput = await _audioProcess.StandardError.ReadToEndAsync();
                if (!string.IsNullOrEmpty(errorOutput))
                {
                    Console.WriteLine($"[AudioService] Player error output: {errorOutput}");
                }
                
                // Only update state if it's not already set
                if (!_isPlaying)
                {
                    _isPlaying = true;
                    PlaybackStateChanged?.Invoke(this, true);
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AudioService] Exception in StartAudioPlaybackAsync: {ex.Message}");
                Console.WriteLine($"[AudioService] Stack trace: {ex.StackTrace}");
                PlaybackError?.Invoke(this, $"Error starting audio playback: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Checks if a command is available on the system
        /// </summary>
        private bool IsCommandAvailable(string command)
        {
            try
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "where" : "which",
                    Arguments = command,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using var process = Process.Start(processStartInfo);
                if (process != null)
                {
                    process.WaitForExit(1000); // Wait up to 1 second
                    return process.ExitCode == 0;
                }
                return false;
            }
            catch
            {
                return false;
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

        private async void OnAudioProcessExited(object? sender, EventArgs e)
        {
            try
            {
                // Update the state immediately
                _isPlaying = false;
                
                // Notify UI about the state change
                // The MainWindowViewModel will handle the UI thread dispatching
                PlaybackStateChanged?.Invoke(this, false);

                // Only restart if we're in repeat mode AND haven't been told to stop
                if (_isRepeating && !_shouldStop && !string.IsNullOrEmpty(_currentAudioFile))
                {
                    // Add a small delay before restarting to prevent CPU thrashing
                    await Task.Delay(100);
                    if (!_shouldStop) // Check again in case stop was requested during delay
                    {
                        await StartAudioPlaybackAsync(_currentAudioFile);
                    }
                }
                
                // Clear the current audio file reference when playback completes naturally
                if (!_isRepeating)
                {
                    _currentAudioFile = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AudioService] Error in OnAudioProcessExited: {ex}");
                // Ensure we still update the state even if there's an error
                _isPlaying = false;
                PlaybackStateChanged?.Invoke(this, false);
            }
        }

        public void Dispose()
        {
            StopAsync().Wait();
        }
    }
}

