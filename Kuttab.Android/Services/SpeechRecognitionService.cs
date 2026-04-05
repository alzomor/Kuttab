using Android.Content;
using Android.Media;
using Android.OS;
using Android.Speech;
using System;
using System.Collections.Generic;

namespace Kuttab.Android.Services;

/// <summary>
/// Wraps Android SpeechRecognizer for continuous Arabic speech recognition with partial results.
/// </summary>
public class SpeechRecognitionService : Java.Lang.Object, IRecognitionListener, IDisposable
{
    private readonly Context _context;
    private SpeechRecognizer? _speechRecognizer;
    private bool _isListening;
    private bool _shouldRestart;
    private string _languageCode = "ar";
    private int _consecutiveErrors = 0;
    private const int MAX_CONSECUTIVE_ERRORS = 15;

    public bool IsListening => _isListening;

    public event EventHandler<List<string>>? PartialResultReceived;
    public event EventHandler<List<string>>? FinalResultReceived;
    public event EventHandler<string>? ErrorOccurred;
    public event EventHandler? ListeningStarted;
    public event EventHandler? ListeningStopped;
    public event EventHandler? SpeechEngineNotAvailable;

    public SpeechRecognitionService(Context context)
    {
        _context = context;
    }

    public bool IsSpeechRecognitionAvailable()
    {
        return SpeechRecognizer.IsRecognitionAvailable(_context);
    }

    public void SetLanguage(string languageCode)
    {
        _languageCode = languageCode;
    }

    public void StartListening()
    {
        if (_isListening) return;

        try
        {
            // Mute the music/notification stream to suppress beep sounds
            MuteBeepSound(true);

            _speechRecognizer?.Destroy();
            _speechRecognizer = SpeechRecognizer.CreateSpeechRecognizer(_context);
            _speechRecognizer.SetRecognitionListener(this);

            var intent = new Intent(RecognizerIntent.ActionRecognizeSpeech);
            intent.PutExtra(RecognizerIntent.ExtraLanguageModel, RecognizerIntent.LanguageModelFreeForm);
            intent.PutExtra(RecognizerIntent.ExtraLanguage, _languageCode);
            intent.PutExtra(RecognizerIntent.ExtraLanguagePreference, _languageCode);
            intent.PutExtra(RecognizerIntent.ExtraPartialResults, true);
            intent.PutExtra(RecognizerIntent.ExtraMaxResults, 3);
            // Longer silence timeouts for Quran recitation to reduce restarts between ayahs
            // This minimizes the "missed first word" issue caused by engine restart delay
            intent.PutExtra(RecognizerIntent.ExtraSpeechInputCompleteSilenceLengthMillis, 5000);
            intent.PutExtra(RecognizerIntent.ExtraSpeechInputPossiblyCompleteSilenceLengthMillis, 3000);
            intent.PutExtra(RecognizerIntent.ExtraSpeechInputMinimumLengthMillis, 500);

            _shouldRestart = true;
            _consecutiveErrors = 0;
            _speechRecognizer.StartListening(intent);
            _isListening = true;
            ListeningStarted?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            _isListening = false;
            MuteBeepSound(false);
            ErrorOccurred?.Invoke(this, $"Failed to start speech recognition: {ex.Message}");
        }
    }

    public void StopListening()
    {
        _shouldRestart = false;
        _consecutiveErrors = 0;
        var wasListening = _isListening;
        _isListening = false;

        if (wasListening && _speechRecognizer != null)
        {
            try
            {
                _speechRecognizer.StopListening();
                _speechRecognizer.Cancel();
            }
            catch { }
        }

        MuteBeepSound(false);
        ListeningStopped?.Invoke(this, EventArgs.Empty);
    }

    private void MuteBeepSound(bool mute)
    {
        try
        {
            var audioManager = (AudioManager?)_context.GetSystemService(Context.AudioService);
            if (audioManager == null) return;

            if (mute)
            {
                // Mute all streams to suppress speech recognizer beeps
                audioManager.AdjustStreamVolume(global::Android.Media.Stream.Music, Adjust.Mute, 0);
                audioManager.AdjustStreamVolume(global::Android.Media.Stream.Notification, Adjust.Mute, 0);
                audioManager.AdjustStreamVolume(global::Android.Media.Stream.System, Adjust.Mute, 0);
                audioManager.AdjustStreamVolume(global::Android.Media.Stream.Ring, Adjust.Mute, 0);
            }
            else
            {
                // Restore audio
                audioManager.AdjustStreamVolume(global::Android.Media.Stream.Music, Adjust.Unmute, 0);
                audioManager.AdjustStreamVolume(global::Android.Media.Stream.Notification, Adjust.Unmute, 0);
                audioManager.AdjustStreamVolume(global::Android.Media.Stream.System, Adjust.Unmute, 0);
                audioManager.AdjustStreamVolume(global::Android.Media.Stream.Ring, Adjust.Unmute, 0);
            }
        }
        catch { }
    }

    private void RestartListening()
    {
        if (!_shouldRestart) return;

        try
        {
            _isListening = false;
            // Minimal delay before restarting to reduce startup delay impact
            // Faster restart = less time for first word to be missed
            var handler = new Handler(Looper.MainLooper!);
            handler.PostDelayed(() =>
            {
                if (_shouldRestart)
                {
                    StartListening();
                }
            }, 100);
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, $"Failed to restart: {ex.Message}");
        }
    }

    // IRecognitionListener implementation
    public void OnReadyForSpeech(Bundle? @params) { }
    public void OnBeginningOfSpeech() { }
    public void OnRmsChanged(float rmsdB) { }
    public void OnBufferReceived(byte[]? buffer) { }
    public void OnEndOfSpeech() { }

    public void OnResults(Bundle? results)
    {
        if (results == null) return;

        _consecutiveErrors = 0; // success resets the counter
        var matches = results.GetStringArrayList(SpeechRecognizer.ResultsRecognition);
        if (matches != null && matches.Count > 0)
        {
            // Pass all alternatives (up to 3) to the matching logic
            var alternatives = new List<string>();
            for (int i = 0; i < Math.Min(3, matches.Count); i++)
            {
                var result = matches[i];
                if (!string.IsNullOrWhiteSpace(result))
                    alternatives.Add(result);
            }
            if (alternatives.Count > 0)
            {
                FinalResultReceived?.Invoke(this, alternatives);
            }
        }

        // Auto-restart for continuous listening
        RestartListening();
    }

    public void OnPartialResults(Bundle? partialResults)
    {
        if (partialResults == null) return;

        _consecutiveErrors = 0; // partial result means engine is working
        var matches = partialResults.GetStringArrayList(SpeechRecognizer.ResultsRecognition);
        if (matches != null && matches.Count > 0)
        {
            // Pass all alternatives (up to 3) to the matching logic
            var alternatives = new List<string>();
            for (int i = 0; i < Math.Min(3, matches.Count); i++)
            {
                var result = matches[i];
                if (!string.IsNullOrWhiteSpace(result))
                    alternatives.Add(result);
            }
            if (alternatives.Count > 0)
            {
                PartialResultReceived?.Invoke(this, alternatives);
            }
        }
    }

    public void OnError(SpeechRecognizerError error)
    {
        _isListening = false;
        _consecutiveErrors++;

        // If we keep failing, stop entirely
        if (_consecutiveErrors >= MAX_CONSECUTIVE_ERRORS)
        {
            _shouldRestart = false;
            SpeechEngineNotAvailable?.Invoke(this, EventArgs.Empty);
            return;
        }

        switch (error)
        {
            case SpeechRecognizerError.NoMatch:
            case SpeechRecognizerError.SpeechTimeout:
                // Normal — no speech detected, just restart
                RestartListening();
                break;
            case SpeechRecognizerError.Network:
            case SpeechRecognizerError.NetworkTimeout:
                // Try again — might work offline
                RestartListening();
                break;
            case SpeechRecognizerError.RecognizerBusy:
                // Wait a bit longer before retrying
                if (_shouldRestart)
                {
                    var handler = new Handler(Looper.MainLooper!);
                    handler.PostDelayed(() =>
                    {
                        if (_shouldRestart) StartListening();
                    }, 1000);
                }
                break;
            case SpeechRecognizerError.Client:
            case SpeechRecognizerError.InsufficientPermissions:
                // Fatal — speech engine not available or not permitted
                _shouldRestart = false;
                SpeechEngineNotAvailable?.Invoke(this, EventArgs.Empty);
                break;
            case SpeechRecognizerError.Server:
            default:
                // Transient errors (including ServerDisconnected) — just restart
                ErrorOccurred?.Invoke(this, $"Speech recognition error: {error}");
                RestartListening();
                break;
        }
    }

    public void OnEvent(int eventType, Bundle? @params) { }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            StopListening();
            _speechRecognizer?.Destroy();
            _speechRecognizer = null;
        }
        base.Dispose(disposing);
    }
}
