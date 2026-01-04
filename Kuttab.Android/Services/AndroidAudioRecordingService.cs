using Android;
using Android.Content;
using Android.Content.PM;
using Android.Media;
using Android.OS;
using Kuttab.Core.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Kuttab.Android.Services;

public class AndroidAudioRecordingService : IAudioRecordingService
{
    private readonly Context _context;
    private MediaRecorder? _mediaRecorder;
    private MediaPlayer? _mediaPlayer;
    private string? _recordingPath;
    private bool _isRecording;
    private Timer? _durationTimer;
    private DateTime _recordingStartTime;
    private int? _currentSurahNumber;
    private int? _currentAyaNumber;
    
    public AndroidAudioRecordingService(Context context)
    {
        _context = context;
    }
    
    public bool IsRecording => _isRecording;
    public bool HasRecording => _currentSurahNumber.HasValue && _currentAyaNumber.HasValue && HasRecordingForAya(_currentSurahNumber.Value, _currentAyaNumber.Value);
    public string? RecordingPath => _recordingPath;
    public int? CurrentSurahNumber => _currentSurahNumber;
    public int? CurrentAyaNumber => _currentAyaNumber;
    
    public event EventHandler<bool>? RecordingStateChanged;
    public event EventHandler<string>? RecordingError;
    public event EventHandler<TimeSpan>? RecordingDurationChanged;
    
    public async Task<bool> RequestRecordingPermissionAsync()
    {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
        {
            if (_context.CheckSelfPermission(Manifest.Permission.RecordAudio) != Permission.Granted)
            {
                return false;
            }
        }
        return true;
    }
    
    public bool HasRecordingPermission()
    {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
        {
            return _context.CheckSelfPermission(Manifest.Permission.RecordAudio) == Permission.Granted;
        }
        return true;
    }
    
    public async Task<bool> StartRecordingAsync(int surahNumber, int ayaNumber)
    {
        try
        {
            if (!await RequestRecordingPermissionAsync())
            {
                RecordingError?.Invoke(this, "Recording permission denied");
                return false;
            }
            
            _currentSurahNumber = surahNumber;
            _currentAyaNumber = ayaNumber;
            
            var filesDir = global::Android.App.Application.Context.FilesDir?.AbsolutePath;
            if (string.IsNullOrEmpty(filesDir))
            {
                RecordingError?.Invoke(this, "Cannot access app storage");
                return false;
            }
            
            var recordingsDir = Path.Combine(filesDir, "audio", "user");
            if (!Directory.Exists(recordingsDir))
            {
                Directory.CreateDirectory(recordingsDir);
            }
            
            _recordingPath = GetRecordingPath(surahNumber, ayaNumber);
            
            if (_isRecording)
            {
                await StopRecordingAsync();
            }
            
            if (File.Exists(_recordingPath))
            {
                File.Delete(_recordingPath);
            }
            
            _mediaRecorder = new MediaRecorder();
            _mediaRecorder.SetAudioSource(AudioSource.Mic);
            _mediaRecorder.SetOutputFormat(OutputFormat.Mpeg4);
            _mediaRecorder.SetAudioEncoder(AudioEncoder.Aac);
            _mediaRecorder.SetAudioEncodingBitRate(128000);
            _mediaRecorder.SetAudioSamplingRate(44100);
            _mediaRecorder.SetOutputFile(_recordingPath);
            
            _mediaRecorder.Prepare();
            _mediaRecorder.Start();
            
            _isRecording = true;
            _recordingStartTime = DateTime.Now;
            
            _durationTimer = new Timer(UpdateDuration, null, 0, 1000);
            
            RecordingStateChanged?.Invoke(this, true);
            return true;
        }
        catch (Exception ex)
        {
            RecordingError?.Invoke(this, "Failed to start recording: " + ex.Message);
            return false;
        }
    }
    
    public async Task<string?> StopRecordingAsync()
    {
        try
        {
            if (!_isRecording || _mediaRecorder == null)
            {
                return _recordingPath;
            }
            
            _durationTimer?.Dispose();
            _durationTimer = null;
            
            _mediaRecorder.Stop();
            _mediaRecorder.Release();
            _mediaRecorder = null;
            
            _isRecording = false;
            RecordingStateChanged?.Invoke(this, false);
            
            if (!string.IsNullOrEmpty(_recordingPath) && File.Exists(_recordingPath))
            {
                var fileInfo = new FileInfo(_recordingPath);
                var sizeKB = fileInfo.Length / 1024.0;
                RecordingError?.Invoke(this, string.Format("Saved: {0:F1} KB", sizeKB));
            }
            else
            {
                RecordingError?.Invoke(this, "Recording file was not created!");
            }
            
            return _recordingPath;
        }
        catch (Exception ex)
        {
            RecordingError?.Invoke(this, "Failed to stop recording: " + ex.Message);
            return null;
        }
    }
    
    public async Task<bool> PlayRecordingAsync(int surahNumber, int ayaNumber)
    {
        try
        {
            var recordingPath = GetRecordingPath(surahNumber, ayaNumber);
            
            if (!File.Exists(recordingPath))
            {
                RecordingError?.Invoke(this, "No recording available to play");
                return false;
            }
            
            _currentSurahNumber = surahNumber;
            _currentAyaNumber = ayaNumber;
            _recordingPath = recordingPath;
            
            await StopPlaybackAsync();
            
            _mediaPlayer = new MediaPlayer();
            
            _mediaPlayer.Completion += (s, e) =>
            {
                try
                {
                    _mediaPlayer?.Release();
                    _mediaPlayer = null;
                }
                catch { }
            };
            
            _mediaPlayer.Error += (s, e) =>
            {
                RecordingError?.Invoke(this, "Playback error: " + e.What);
                _mediaPlayer?.Release();
                _mediaPlayer = null;
            };
            
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                var audioAttributes = new AudioAttributes.Builder()
                    .SetUsage(AudioUsageKind.Media)
                    .SetContentType(AudioContentType.Music)
                    .Build();
                _mediaPlayer.SetAudioAttributes(audioAttributes);
            }
            else
            {
                _mediaPlayer.SetAudioStreamType(global::Android.Media.Stream.Music);
            }
            
            _mediaPlayer.SetVolume(1.0f, 1.0f);
            _mediaPlayer.SetDataSource(recordingPath);
            _mediaPlayer.Prepare();
            
            var duration = _mediaPlayer.Duration;
            if (duration <= 0)
            {
                RecordingError?.Invoke(this, "Recording file is empty or invalid");
                _mediaPlayer?.Release();
                _mediaPlayer = null;
                return false;
            }
            
            _mediaPlayer.Start();
            RecordingError?.Invoke(this, string.Format("Playing ({0}s)", duration/1000));
            
            return true;
        }
        catch (Exception ex)
        {
            RecordingError?.Invoke(this, "Failed to play: " + ex.Message);
            _mediaPlayer?.Release();
            _mediaPlayer = null;
            return false;
        }
    }
    
    public async Task StopPlaybackAsync()
    {
        try
        {
            if (_mediaPlayer != null)
            {
                if (_mediaPlayer.IsPlaying)
                {
                    _mediaPlayer.Stop();
                }
                _mediaPlayer.Release();
                _mediaPlayer = null;
            }
        }
        catch (Exception ex)
        {
            RecordingError?.Invoke(this, "Failed to stop playback: " + ex.Message);
        }
    }
    
    public async Task DeleteRecordingAsync(int surahNumber, int ayaNumber)
    {
        try
        {
            if (_isRecording && _currentSurahNumber == surahNumber && _currentAyaNumber == ayaNumber)
            {
                await StopRecordingAsync();
            }
            
            await StopPlaybackAsync();
            
            var recordingPath = GetRecordingPath(surahNumber, ayaNumber);
            if (File.Exists(recordingPath))
            {
                File.Delete(recordingPath);
            }
            
            if (_currentSurahNumber == surahNumber && _currentAyaNumber == ayaNumber)
            {
                _recordingPath = null;
                _currentSurahNumber = null;
                _currentAyaNumber = null;
            }
        }
        catch (Exception ex)
        {
            RecordingError?.Invoke(this, "Failed to delete recording: " + ex.Message);
        }
    }
    
    public bool HasRecordingForAya(int surahNumber, int ayaNumber)
    {
        return File.Exists(GetRecordingPath(surahNumber, ayaNumber));
    }
    
    public string GetRecordingPath(int surahNumber, int ayaNumber)
    {
        var filesDir = global::Android.App.Application.Context.FilesDir?.AbsolutePath;
        if (string.IsNullOrEmpty(filesDir))
        {
            return string.Empty;
        }
        
        var paddedSurah = surahNumber.ToString("D3");
        var paddedAya = ayaNumber.ToString("D3");
        var recordingsDir = Path.Combine(filesDir, "audio", "user");
        return Path.Combine(recordingsDir, string.Format("{0}{1}.m4a", paddedSurah, paddedAya));
    }
    
    private void UpdateDuration(object? state)
    {
        if (_isRecording)
        {
            var duration = DateTime.Now - _recordingStartTime;
            RecordingDurationChanged?.Invoke(this, duration);
        }
    }
    
    public void Dispose()
    {
        if (_isRecording)
        {
            StopRecordingAsync().Wait();
        }
        
        StopPlaybackAsync().Wait();
        _durationTimer?.Dispose();
    }
}
