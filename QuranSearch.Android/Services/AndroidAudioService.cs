using Android.Content;
using Android.Media;
using QuranSearch.Core.Interfaces;
using System;
using System.Threading.Tasks;

namespace QuranSearch.Android.Services;

public class AndroidAudioService : IAudioService
{
    private readonly Context _context;
    private MediaPlayer? _mediaPlayer;
    private bool _isPlaying;
    private bool _isRepeating;
    
    public AndroidAudioService(Context context)
    {
        _context = context;
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
            StopPlayback();
            
            _mediaPlayer = new MediaPlayer();
            
            if (UseRemoteSource)
            {
                // Use online URL
                var uri = global::Android.Net.Uri.Parse(audioPath);
                if (uri != null)
                {
                    await _mediaPlayer.SetDataSourceAsync(_context, uri);
                }
            }
            else
            {
                // Use local file from assets
                var assetFileDescriptor = _context.Assets?.OpenFd(audioPath);
                if (assetFileDescriptor != null)
                {
                    _mediaPlayer.SetDataSource(
                        assetFileDescriptor.FileDescriptor,
                        assetFileDescriptor.StartOffset,
                        assetFileDescriptor.Length);
                    assetFileDescriptor.Close();
                }
            }
            
            _mediaPlayer.Completion += OnPlaybackCompleted;
            _mediaPlayer.Error += OnPlaybackError;
            
            _mediaPlayer.PrepareAsync();
            _mediaPlayer.Start();
            
            _isPlaying = true;
            PlaybackStateChanged?.Invoke(this, true);
        }
        catch (Exception ex)
        {
            _isPlaying = false;
            PlaybackError?.Invoke(this, ex.Message);
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
        PlaybackError?.Invoke(this, $"Playback error: {e.What}");
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
                _isRepeating = false;
                PlaybackStateChanged?.Invoke(this, false);
            }
        }
    }
    
    public void SetRepeatMode(bool repeat)
    {
        _isRepeating = repeat;
    }
    
    public bool AudioFileExists(int surahNumber, int ayaNumber)
    {
        if (UseRemoteSource)
        {
            return true; // Assume remote files exist
        }
        
        var audioPath = GetLocalAudioPath(surahNumber, ayaNumber);
        try
        {
            using var stream = _context.Assets?.Open(audioPath);
            return stream != null;
        }
        catch
        {
            return false;
        }
    }
    
    public async Task PlayAyaAsync(int surahNumber, int ayaNumber)
    {
        var audioPath = UseRemoteSource 
            ? GetRemoteAudioUrl(surahNumber, ayaNumber)
            : GetLocalAudioPath(surahNumber, ayaNumber);
        
        await PlayAudioAsync(audioPath);
    }
    
    public Task StopAsync()
    {
        StopPlayback();
        return Task.CompletedTask;
    }
    
    private string GetRemoteAudioUrl(int surah, int aya)
    {
        var paddedSurah = surah.ToString("D3");
        var paddedAya = aya.ToString("D3");
        return $"https://everyayah.com/data/Husary_128kbps/{paddedSurah}{paddedAya}.mp3";
    }
    
    private string GetLocalAudioPath(int surah, int aya)
    {
        var paddedSurah = surah.ToString("D3");
        var paddedAya = aya.ToString("D3");
        return $"AL Husary/000_versebyverse/{paddedSurah}{paddedAya}.mp3";
    }
    
    public void Dispose()
    {
        StopPlayback();
    }
}
