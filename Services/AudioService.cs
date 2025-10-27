using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Text;
using QuranSearch.Core.Interfaces;

namespace QuranSearchApp.Services
{
    public class AudioService : IAudioService
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
        private static readonly string _logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "audio_debug.log");
        private static readonly object _logLock = new object();

        public bool UseRemoteSource
        {
            get => _useRemoteSource;
            set => _useRemoteSource = value;
        }

        public AudioService()
        {
            // Look for audio files in the same directory as the executable
            var exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
            _audioBasePath = Path.Combine(exeDirectory, "Alhusary");
            
            // Log the path for debugging
            LogDebug($"Looking for audio files in: {_audioBasePath}");
            LogDebug($"Directory exists: {Directory.Exists(_audioBasePath)}");
        }

        /// <summary>
        /// Logs debug information to both Debug output and a log file
        /// </summary>
        private static void LogDebug(string message)
        {
            string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [AudioService] {message}";
            
            // Write to Debug output (visible in debuggers and DebugView on Windows)
            Debug.WriteLine(logMessage);
            Trace.WriteLine(logMessage);
            
            // Also write to console (works on Linux when run from terminal)
            Console.WriteLine(logMessage);
            
            // Write to log file for persistent debugging
            try
            {
                lock (_logLock)
                {
                    File.AppendAllText(_logFilePath, logMessage + Environment.NewLine);
                }
            }
            catch
            {
                // Ignore file write errors to prevent crashes
            }
        }

        public event EventHandler<bool>? PlaybackStateChanged;
        public event EventHandler<string>? PlaybackError;
        public event EventHandler? SequencePlaybackEnded;

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

                string? pathToPlay;

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
                    LogDebug($"Creating cache directory: {_audioBasePath}");
                    Directory.CreateDirectory(_audioBasePath);
                }

                // Check if file exists in cache
                string fileName = $"{suraNumber:D3}{ayaNumber:D3}.mp3";
                string cachedFilePath = Path.Combine(_audioBasePath, fileName);

                if (File.Exists(cachedFilePath))
                {
                    LogDebug($"Using cached file: {cachedFilePath}");
                    return cachedFilePath;
                }

                // Download to cache
                string remoteUrl = $"{_remoteAudioBaseUrl.TrimEnd('/')}/{fileName}";
                LogDebug($"Downloading from: {remoteUrl}");
                LogDebug($"Caching to: {cachedFilePath}");

                using var response = await _httpClient.GetAsync(remoteUrl, HttpCompletionOption.ResponseHeadersRead);
                LogDebug($"Response status: {response.StatusCode}");
                response.EnsureSuccessStatusCode();

                await using (var fs = File.Create(cachedFilePath))
                {
                    await response.Content.CopyToAsync(fs);
                }

                var fileInfo = new FileInfo(cachedFilePath);
                LogDebug($"Downloaded and cached file size: {fileInfo.Length} bytes");

                return cachedFilePath;
            }
            catch (Exception ex)
            {
                LogDebug($"Failed to get or download audio: {ex.Message}");
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
                            // Get the full path to the player for reliability
                            command = GetFullPathToPlayer(player) ?? player;
                            arguments = player switch
                            {
                                "mpg123" => $"-q \"{filePath}\"",  // -q for quiet mode
                                "paplay" => $"\"{filePath}\"",
                                "ffplay" => $"-nodisp -autoexit -loglevel quiet \"{filePath}\"",
                                "aplay" => $"\"{filePath}\"",
                                _ => $"\"{filePath}\""
                            };
                            LogDebug($"Using audio player: {command}");
                            LogDebug($"Arguments: {arguments}");
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
                    // Use PowerShell with MediaPlayer (supports MP3)
                    command = "powershell";
                    // Escape single quotes in file path for PowerShell
                    string escapedPath = filePath.Replace("'", "''");
                    // Improved PowerShell script that ensures clean exit
                    arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"" +
                        $"Add-Type -AssemblyName PresentationCore; " +
                        $"$player = New-Object System.Windows.Media.MediaPlayer; " +
                        $"$player.Open('{escapedPath}'); " +
                        $"$player.Play(); " +
                        $"while($player.NaturalDuration.HasTimeSpan -eq $false){{ Start-Sleep -Milliseconds 100 }}; " +
                        $"$duration = $player.NaturalDuration.TimeSpan.TotalMilliseconds; " +
                        $"Start-Sleep -Milliseconds $duration; " +
                        $"$player.Stop(); " +
                        $"$player.Close(); " +
                        $"exit\"";
                    LogDebug($"Using PowerShell MediaPlayer for Windows");
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    // Use afplay for macOS (built-in)
                    command = "afplay";
                    arguments = $"\"{filePath}\"";
                    LogDebug($"Using afplay for macOS");
                }
                else
                {
                    PlaybackError?.Invoke(this, "Unsupported operating system for audio playback");
                    return;
                }

                LogDebug($"Starting playback: {command} {arguments}");
                LogDebug($"File path: {filePath}");
                LogDebug($"File exists: {File.Exists(filePath)}");

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
                
                // Read error output asynchronously without blocking
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var errorOutput = await _audioProcess.StandardError.ReadToEndAsync();
                        if (!string.IsNullOrEmpty(errorOutput))
                        {
                            LogDebug($"Player error output: {errorOutput}");
                        }
                    }
                    catch (Exception ex)
                    {
                        LogDebug($"Error reading process output: {ex.Message}");
                    }
                });
                
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
                LogDebug($"Exception in StartAudioPlaybackAsync: {ex.Message}");
                LogDebug($"Stack trace: {ex.StackTrace}");
                PlaybackError?.Invoke(this, $"Error starting audio playback: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Gets the full path to a command/player on the system
        /// </summary>
        private string? GetFullPathToPlayer(string command)
        {
            try
            {
                using var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "where" : "which",
                        Arguments = command,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardError = true
                    }
                };

                process.Start();
                string? result = process.StandardOutput.ReadLine()?.Trim();
                process.WaitForExit();

                return !string.IsNullOrEmpty(result) && process.ExitCode == 0 ? result : null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Checks if a command is available on the system
        /// </summary>
        private bool IsCommandAvailable(string command)
        {
            return GetFullPathToPlayer(command) != null;
        }

        /// <summary>
        /// Stops current playback
        /// </summary>
        public async Task StopAsync()
        {
            _shouldStop = true;
            // Don't reset _isRepeating here - it should only be controlled by SetRepeatMode()
            // Otherwise, calling PlayAyaAsync() in repeat mode will clear the flag

            if (_audioProcess != null)
            {
                try
                {
                    // Check if process has already exited (common on Windows)
                    if (_audioProcess.HasExited)
                    {
                        LogDebug("Process already exited, cleaning up");
                    }
                    else
                    {
                        LogDebug("Killing active audio process");
                        _audioProcess.Kill();
                        _audioProcess.WaitForExit(1000);
                    }
                }
                catch (Exception ex)
                {
                    LogDebug($"Error stopping audio: {ex.Message}");
                    PlaybackError?.Invoke(this, $"Error stopping audio: {ex.Message}");
                }
                finally
                {
                    _audioProcess?.Dispose();
                    _audioProcess = null;
                }
            }

            // Always update state, even if process was already gone
            if (_isPlaying)
            {
                _isPlaying = false;
                PlaybackStateChanged?.Invoke(this, false);
                LogDebug("Playback state cleared in StopAsync");
            }
            
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
                PlaybackStateChanged?.Invoke(this, false);

                // Only restart if we're in repeat mode AND haven't been told to stop
                if (_isRepeating && !_shouldStop && !string.IsNullOrEmpty(_currentAudioFile))
                {
                    // Add a small delay before restarting to prevent CPU thrashing
                    await Task.Delay(100);
                    if (!_shouldStop) // Check again in case stop was requested during delay
                    {
                        await StartAudioPlaybackAsync(_currentAudioFile);
                        return;
                    }
                }
                
                // Clear the current audio file reference when playback completes naturally
                if (!_isRepeating)
                {
                    string? completedFile = _currentAudioFile;
                    _currentAudioFile = null;
                    
                    // Notify that the current audio in sequence has ended
                    if (completedFile != null)
                    {
                        SequencePlaybackEnded?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
            catch (Exception ex)
            {
                LogDebug($"Error in OnAudioProcessExited: {ex}");
                // Ensure we still update the state even if there's an error
                _isPlaying = false;
                PlaybackStateChanged?.Invoke(this, false);
                
                // Even on error, notify that the sequence playback has ended
                if (!_isRepeating)
                {
                    SequencePlaybackEnded?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public void Dispose()
        {
            StopAsync().Wait();
        }
    }
}

