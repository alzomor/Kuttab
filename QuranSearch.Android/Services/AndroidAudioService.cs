using Android.Content;
using Android.Media;
using Android.Net;
using QuranSearch.Core.Interfaces;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace QuranSearch.Android.Services;

public class AndroidAudioService : IAudioService
{
    private readonly Context _context;
    private readonly HttpClient _httpClient;
    private MediaPlayer? _mediaPlayer;
    private bool _isPlaying;
    private bool _isRepeating;
    
    public AndroidAudioService(Context context)
    {
        _context = context;
        _httpClient = new HttpClient();
    }
    
    public bool UseRemoteSource { get; set; } = true;
    public bool IsPlaying => _isPlaying;
    public bool IsRepeating => _isRepeating;
    
    public event EventHandler<bool>? PlaybackStateChanged;
    public event EventHandler<string>? PlaybackError;
    public event EventHandler? SequencePlaybackEnded;
    
    public async Task PlayAudioAsync(string audioPath)
    {
        try
        {
            // Stop any existing playback first, but preserve repeat mode
            if (_mediaPlayer != null)
            {
                try
                {
                    if (_mediaPlayer.IsPlaying)
                    {
                        _mediaPlayer.Stop();
                    }
                    _mediaPlayer.Release();
                }
                catch { }
                _mediaPlayer = null;
            }
            
            _mediaPlayer = new MediaPlayer();
            
            // Configure audio routing to device speakers
            _mediaPlayer.SetAudioStreamType(global::Android.Media.Stream.Music);
            
            // When called with a file system path, use it directly
            if (File.Exists(audioPath))
            {
                _mediaPlayer.SetDataSource(audioPath);
            }
            else
            {
                // Otherwise assume it's a URL
                var uri = global::Android.Net.Uri.Parse(audioPath);
                if (uri != null)
                {
                    await _mediaPlayer.SetDataSourceAsync(_context, uri);
                }
                else
                {
                    PlaybackError?.Invoke(this, "ERROR: Failed to parse URI");
                    return;
                }
            }
            
            _mediaPlayer.Completion += OnPlaybackCompleted;
            _mediaPlayer.Error += OnPlaybackError;
            _mediaPlayer.Prepared += OnMediaPlayerPrepared;
            
            _mediaPlayer.PrepareAsync();
            // Don't call Start() here - wait for Prepared event
        }
        catch (Exception ex)
        {
            _isPlaying = false;
            PlaybackError?.Invoke(this, ex.Message);
        }
    }
    
    private void OnMediaPlayerPrepared(object? sender, EventArgs e)
    {
        try
        {
            // Set volume to maximum to ensure audibility
            _mediaPlayer?.SetVolume(1.0f, 1.0f);
            
            _mediaPlayer?.Start();
            _isPlaying = true;
            PlaybackStateChanged?.Invoke(this, true);
        }
        catch (Exception ex)
        {
            PlaybackError?.Invoke(this, $"ERROR starting after prepare: {ex.Message}");
        }
    }
    
    private void OnPlaybackCompleted(object? sender, EventArgs e)
    {
        if (_isRepeating)
        {
            // Restart the same audio
            _mediaPlayer?.SeekTo(0);
            _mediaPlayer?.Start();
        }
        else
        {
            _isPlaying = false;
            PlaybackStateChanged?.Invoke(this, false);
            SequencePlaybackEnded?.Invoke(this, EventArgs.Empty);
        }
    }
    
    private void OnPlaybackError(object? sender, MediaPlayer.ErrorEventArgs e)
    {
        _isPlaying = false;
        var errorMsg = $"MediaPlayer error: {e.What} (code: {(int)e.What}). Extra: {e.Extra}";
        PlaybackError?.Invoke(this, errorMsg);
        PlaybackStateChanged?.Invoke(this, false);
    }
    
    public void StopPlayback()
    {
        if (_mediaPlayer != null)
        {
            try
            {
                if (_mediaPlayer.IsPlaying)
                {
                    _mediaPlayer.Stop();
                }
                _mediaPlayer.Release();
            }
            catch { }
            finally
            {
                _mediaPlayer = null;
                _isPlaying = false;
                // Don't clear _isRepeating here - it should persist until explicitly stopped
                PlaybackStateChanged?.Invoke(this, false);
            }
        }
    }
    
    public Task StopAsync()
    {
        _isRepeating = false; // Clear repeat mode when explicitly stopping
        StopPlayback();
        return Task.CompletedTask;
    }
    
    public void SetRepeatMode(bool repeat)
    {
        _isRepeating = repeat;
    }
    
    public bool AudioFileExists(int surahNumber, int ayaNumber)
    {
        var localPath = GetLocalAudioFilePath(surahNumber, ayaNumber);
        return File.Exists(localPath);
    }
    
    public async Task PlayAyaAsync(int surahNumber, int ayaNumber)
    {
        try
        {
            // 1) Try local file
            var localPath = GetLocalAudioFilePath(surahNumber, ayaNumber);
            
            if (File.Exists(localPath))
            {
                await PlayAudioAsync(localPath);
                return;
            }

            // 2) If not local and remote is disabled, error
            if (!UseRemoteSource)
            {
                PlaybackError?.Invoke(this, "Audio file not found locally and remote playback is disabled.");
                return;
            }

            // 3) Check network availability
            if (!IsNetworkAvailable())
            {
                PlaybackError?.Invoke(this, "No internet connection. Audio cannot be downloaded or streamed.");
                return;
            }

            // 4) Try to download and play
            var url = GetRemoteAudioUrl(surahNumber, ayaNumber);
            
            try
            {
                await EnsureLocalAudioAsync(url, localPath);
                
                if (File.Exists(localPath))
                {
                    await PlayAudioAsync(localPath);
                    return;
                }
            }
            catch (Exception downloadEx)
            {
                // Download failed, try streaming directly
                try
                {
                    await PlayAudioAsync(url);
                    return;
                }
                catch (Exception streamEx)
                {
                    PlaybackError?.Invoke(this, $"Download failed: {downloadEx.Message}. Stream failed: {streamEx.Message}");
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            PlaybackError?.Invoke(this, $"Playback error: {ex.Message}");
        }
    }
    
    private string GetRemoteAudioUrl(int surah, int aya)
    {
        var paddedSurah = surah.ToString("D3");
        var paddedAya = aya.ToString("D3");
        return $"https://everyayah.com/data/Husary_128kbps/{paddedSurah}{paddedAya}.mp3";
    }
    
    private string GetLocalAudioAssetsPath(int surah, int aya)
    {
        var paddedSurah = surah.ToString("D3");
        var paddedAya = aya.ToString("D3");
        return $"AL Husary/000_versebyverse/{paddedSurah}{paddedAya}.mp3";
    }

    private string GetLocalAudioFilePath(int surah, int aya)
    {
        var paddedSurah = surah.ToString("D3");
        var paddedAya = aya.ToString("D3");
        var audioDir = Path.Combine(_context.FilesDir!.AbsolutePath, "audio", "AL Husary", "000_versebyverse");
        Directory.CreateDirectory(audioDir);
        return Path.Combine(audioDir, $"{paddedSurah}{paddedAya}.mp3");
    }

    private async Task EnsureLocalAudioAsync(string url, string localPath)
    {
        var tmpPath = localPath + ".download";
        using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();
        await using (var fs = new FileStream(tmpPath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await response.Content.CopyToAsync(fs);
        }
        if (File.Exists(localPath)) File.Delete(localPath);
        File.Move(tmpPath, localPath);
    }

    private bool IsNetworkAvailable()
    {
        try
        {
            var cm = (ConnectivityManager?)_context.GetSystemService(Context.ConnectivityService);
            if (cm == null) return false;
#pragma warning disable CA1416
            var nw = cm.ActiveNetworkInfo;
            return nw != null && nw.IsConnected;
#pragma warning restore CA1416
        }
        catch
        {
            return false;
        }
    }
    
    public void Dispose()
    {
        StopPlayback();
    }
}
