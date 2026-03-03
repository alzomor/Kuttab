using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Speech;
using Android.Speech.Tts;
using Android.Text;
using Android.Text.Style;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Kuttab.Android.Services;
using Kuttab.Core.Models;
using Kuttab.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;
using LocalizationService = Kuttab.Core.Services.LocalizationService;

namespace Kuttab.Android;

[Activity(Label = "Hifz Mode", Theme = "@style/AppTheme")]
public class HifzActivity : AppCompatActivity
{
    private const int PERMISSION_REQUEST_RECORD_AUDIO = 2001;
    private const int REQUEST_INSTALL_SPEECH_ENGINE = 2002;
    private const int REQUEST_INSTALL_LANGUAGE_DATA = 2003;
    private bool _pendingStartAfterInstall = false;

    // Services
    private LocalizationService? _localizationService;
    private QuranSearchService? _searchService;
    private SpeechRecognitionService? _speechService;
    private Vibrator? _vibrator;

    // UI Elements
    private TextView? _titleTextView;
    private TextView? _surahLabel;
    private Spinner? _surahSpinner;
    private TextView? _fromAyaLabel;
    private EditText? _fromAyaInput;
    private TextView? _toAyaLabel;
    private EditText? _toAyaInput;
    private TextView? _statusLabel;
    private TextView? _currentAyaInfo;
    private ProgressBar? _wordProgressBar;
    private TextView? _progressText;
    private TextView? _ayaTextView;
    private TextView? _recognizedTextLabel;
    private TextView? _recognizedTextView;
    private Button? _micButton;
    private Button? _stopButton;
    private Button? _resetButton;
    private LinearLayout? _tajweedRulesPanelContainer;
    private TextView? _tajweedRulesTitle;
    private LinearLayout? _tajweedRulesContainer;
    private View? _wrongWordIndicator;
    private ScrollView? _completedAyaScrollView;
    private TextView? _completedAyaHistory;

    // State
    private int _selectedSurah = 1;
    private int _fromAya = 1;
    private int _toAya = 7;
    private int _currentAya = 1;
    private int _currentWordIndex = 0;
    private List<string> _currentAyaWords = new();
    private string _currentAyaFullText = "";
    private bool _isActive = false;
    private bool _showTajweedRules = true;
    private List<string>? _selectedTajweedRules;
    private string _lastProcessedText = "";
    private long _lastWrongVibrationTime = 0;
    private StringBuilder _completedAyaText = new();
    private Handler? _idleTimeoutHandler;
    private Java.Lang.Runnable? _idleTimeoutRunnable;
    private const int IDLE_TIMEOUT_MS = 60000; // 1 minute

    // Tajweed color maps (same as RecitationActivity)
    private static readonly Dictionary<string, int> GroupColors = new()
    {
        { "lam", unchecked((int)0xFF1565C0) },
        { "noon_tanween", unchecked((int)0xFF2E7D32) },
        { "meem_sakinah", unchecked((int)0xFF00838F) },
        { "noon_meem_mushaddad", unchecked((int)0xFF6A1B9A) },
        { "qalqalah", unchecked((int)0xFFE65100) },
        { "mad", unchecked((int)0xFF8E24AA) },
        { "waqf", unchecked((int)0xFFEF6C00) },
        { "tafkhim_tarqiq", unchecked((int)0xFFC62828) },
    };

    private static readonly Dictionary<string, int> GroupHighlightColors = new()
    {
        { "lam", unchecked((int)0x401565C0) },
        { "noon_tanween", unchecked((int)0x402E7D32) },
        { "meem_sakinah", unchecked((int)0x4000838F) },
        { "noon_meem_mushaddad", unchecked((int)0x406A1B9A) },
        { "qalqalah", unchecked((int)0x40E65100) },
        { "mad", unchecked((int)0x408E24AA) },
        { "waqf", unchecked((int)0x40EF6C00) },
        { "tafkhim_tarqiq", unchecked((int)0x40C62828) },
    };

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        if (Window != null)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                Window.SetStatusBarColor(global::Android.Graphics.Color.ParseColor("#0D3F13"));
                Window.SetNavigationBarColor(global::Android.Graphics.Color.ParseColor("#1B5E20"));
            }
        }

        SetContentView(Resource.Layout.activity_hifz);

        InitializeServices();
        InitializeViews();
        LoadSettings();
        SetupSpinners();
        LoadQuranData();
    }

    private void InitializeServices()
    {
        try
        {
            var fileService = new AndroidFileService(this);
            _localizationService = new LocalizationService(fileService);
            _searchService = new QuranSearchService(fileService);
            _speechService = new SpeechRecognitionService(this);
            _vibrator = (Vibrator?)GetSystemService(VibratorService);

            _speechService.PartialResultReceived += OnSpeechPartialResult;
            _speechService.FinalResultReceived += OnSpeechFinalResult;
            _speechService.ErrorOccurred += OnSpeechError;
            _speechService.SpeechEngineNotAvailable += OnSpeechEngineNotAvailable;
        }
        catch (Exception ex)
        {
            ShowToast($"Init error: {ex.Message}");
        }
    }

    private void InitializeViews()
    {
        _titleTextView = FindViewById<TextView>(Resource.Id.titleTextView);
        _surahLabel = FindViewById<TextView>(Resource.Id.surahLabel);
        _surahSpinner = FindViewById<Spinner>(Resource.Id.surahSpinner);
        _fromAyaLabel = FindViewById<TextView>(Resource.Id.fromAyaLabel);
        _fromAyaInput = FindViewById<EditText>(Resource.Id.fromAyaInput);
        _toAyaLabel = FindViewById<TextView>(Resource.Id.toAyaLabel);
        _toAyaInput = FindViewById<EditText>(Resource.Id.toAyaInput);
        _statusLabel = FindViewById<TextView>(Resource.Id.statusLabel);
        _currentAyaInfo = FindViewById<TextView>(Resource.Id.currentAyaInfo);
        _wordProgressBar = FindViewById<ProgressBar>(Resource.Id.wordProgressBar);
        _progressText = FindViewById<TextView>(Resource.Id.progressText);
        _ayaTextView = FindViewById<TextView>(Resource.Id.ayaTextView);
        _recognizedTextLabel = FindViewById<TextView>(Resource.Id.recognizedTextLabel);
        _recognizedTextView = FindViewById<TextView>(Resource.Id.recognizedTextView);
        _micButton = FindViewById<Button>(Resource.Id.micButton);
        _stopButton = FindViewById<Button>(Resource.Id.stopButton);
        _resetButton = FindViewById<Button>(Resource.Id.resetButton);
        _tajweedRulesPanelContainer = FindViewById<LinearLayout>(Resource.Id.tajweedRulesPanelContainer);
        _tajweedRulesTitle = FindViewById<TextView>(Resource.Id.tajweedRulesTitle);
        _tajweedRulesContainer = FindViewById<LinearLayout>(Resource.Id.tajweedRulesContainer);
        _wrongWordIndicator = FindViewById<View>(Resource.Id.wrongWordIndicator);
        _completedAyaScrollView = FindViewById<ScrollView>(Resource.Id.completedAyaScrollView);
        _completedAyaHistory = FindViewById<TextView>(Resource.Id.completedAyaHistory);

        if (_micButton != null)
            _micButton.Click += OnMicClick;
        if (_stopButton != null)
            _stopButton.Click += OnStopClick;
        if (_resetButton != null)
            _resetButton.Click += OnResetClick;

        if (_fromAyaInput != null)
            _fromAyaInput.FocusChange += (s, e) => { if (!e.HasFocus) ValidateAyaInputs(); };
        if (_toAyaInput != null)
            _toAyaInput.FocusChange += (s, e) => { if (!e.HasFocus) ValidateAyaInputs(); };
    }

    private void LoadSettings()
    {
        var prefs = GetSharedPreferences("QuranSearchSettings", FileCreationMode.Private);
        var language = Kuttab.Android.Utils.LanguageHelper.GetLanguageFromPreferences(this);

        if (_localizationService != null)
        {
            _localizationService.CurrentLanguage = language;
            UpdateUIStrings();
            UpdateLayoutDirection(language);
        }

        _showTajweedRules = prefs?.GetBoolean("ShowTajweedRules", true) ?? true;
        var savedRules = prefs?.GetStringSet("SelectedTajweedRules", null);
        _selectedTajweedRules = savedRules != null ? new List<string>(savedRules) : null;
    }

    private void UpdateUIStrings()
    {
        if (_localizationService == null) return;

        if (_titleTextView != null)
            _titleTextView.Text = _localizationService.HifzMode;
        if (_surahLabel != null)
            _surahLabel.Text = _localizationService["SelectSurah"];
        if (_fromAyaLabel != null)
            _fromAyaLabel.Text = _localizationService["FromAya"];
        if (_toAyaLabel != null)
            _toAyaLabel.Text = _localizationService["ToAya"];
        if (_statusLabel != null)
            _statusLabel.Text = _localizationService.HifzStatus;
        if (_recognizedTextLabel != null)
            _recognizedTextLabel.Text = _localizationService.HifzHeard;
        if (_tajweedRulesTitle != null)
            _tajweedRulesTitle.Text = _localizationService["TajweedRulesSelection"];
    }

    private void UpdateLayoutDirection(string language)
    {
        var layoutDirection = language == "ar" ? LayoutDirection.Rtl : LayoutDirection.Ltr;
        if (Window?.DecorView != null)
            Window.DecorView.LayoutDirection = layoutDirection;
    }

    private void SetupSpinners()
    {
        var surahNames = new List<string>();
        for (int i = 1; i <= SurahInfo.TotalSurahs; i++)
            surahNames.Add($"{i}. {SurahInfo.GetSurahName(i)}");

        var adapter = new ArrayAdapter<string>(this,
            global::Android.Resource.Layout.SimpleSpinnerItem, surahNames);
        adapter.SetDropDownViewResource(global::Android.Resource.Layout.SimpleSpinnerDropDownItem);

        if (_surahSpinner != null)
        {
            _surahSpinner.Adapter = adapter;
            _surahSpinner.ItemSelected += OnSurahSelected;
        }

        UpdateAyaInputsForSurah();
    }

    private async void LoadQuranData()
    {
        try
        {
            if (_searchService != null)
            {
                await _searchService.LoadQuranTextAsync();
                await _searchService.LoadRulesAsync();
            }
            RunOnUiThread(() => LoadCurrentAya());
        }
        catch (Exception ex)
        {
            RunOnUiThread(() => ShowToast($"Failed to load Quran data: {ex.Message}"));
        }
    }

    private void OnSurahSelected(object? sender, AdapterView.ItemSelectedEventArgs e)
    {
        _selectedSurah = e.Position + 1;
        UpdateAyaInputsForSurah();
        if (!_isActive) LoadCurrentAya();
    }

    private void UpdateAyaInputsForSurah()
    {
        int ayaCount = SurahInfo.GetAyaCount(_selectedSurah);
        _fromAya = 1;
        _toAya = ayaCount;
        if (_fromAyaInput != null) _fromAyaInput.Text = "1";
        if (_toAyaInput != null) _toAyaInput.Text = ayaCount.ToString();
    }

    private void ValidateAyaInputs()
    {
        int ayaCount = SurahInfo.GetAyaCount(_selectedSurah);

        if (!int.TryParse(_fromAyaInput?.Text, out int fromVal) || fromVal < 1)
            fromVal = 1;
        if (fromVal > ayaCount) fromVal = ayaCount;

        if (!int.TryParse(_toAyaInput?.Text, out int toVal) || toVal < 1)
            toVal = ayaCount;
        if (toVal > ayaCount) toVal = ayaCount;
        if (fromVal > toVal) toVal = fromVal;

        _fromAya = fromVal;
        _toAya = toVal;

        if (_fromAyaInput != null && _fromAyaInput.Text != fromVal.ToString())
            _fromAyaInput.Text = fromVal.ToString();
        if (_toAyaInput != null && _toAyaInput.Text != toVal.ToString())
            _toAyaInput.Text = toVal.ToString();
    }

    // ── Mic Control ──────────────────────────────────────────

    private void OnMicClick(object? sender, EventArgs e)
    {
        if (_speechService == null) return;

        // Step 1: Check microphone permission
        if (ContextCompat.CheckSelfPermission(this, global::Android.Manifest.Permission.RecordAudio)
            != Permission.Granted)
        {
            ActivityCompat.RequestPermissions(this,
                new[] { global::Android.Manifest.Permission.RecordAudio },
                PERMISSION_REQUEST_RECORD_AUDIO);
            return;
        }

        // Step 2: Just start — the error handler will catch if no engine is available
        StartHifzSession();
    }

    private void PromptInstallSpeechEngine()
    {
        var builder = new AndroidX.AppCompat.App.AlertDialog.Builder(this);
        builder.SetTitle(_localizationService?.HifzNoSpeechEngine ?? "Speech Recognition");
        builder.SetMessage("Arabic speech recognition is not available. Please ensure:\n\n1. Google app is installed and updated\n2. Go to Google app > Settings > Voice > Offline speech recognition > Download Arabic\n\nThen restart this app and try again.");
        builder.SetPositiveButton("Open Google App", (s, e) =>
        {
            try
            {
                var intent = new Intent(Intent.ActionView,
                    global::Android.Net.Uri.Parse("market://details?id=com.google.android.googlequicksearchbox"));
                intent.AddFlags(ActivityFlags.NewTask);
                StartActivity(intent);
            }
            catch
            {
                ShowToast("Could not open app store");
            }
        });
        builder.SetNegativeButton("OK", (s, e) => { });
        builder.Show();
    }

    private void OnStopClick(object? sender, EventArgs e)
    {
        StopHifzSession();
    }

    private void OnResetClick(object? sender, EventArgs e)
    {
        StopHifzSession();
        _currentWordIndex = 0;
        _lastProcessedText = "";
        _completedAyaText.Clear();
        if (_completedAyaHistory != null) _completedAyaHistory.Text = "";
        if (_completedAyaScrollView != null) _completedAyaScrollView.Visibility = global::Android.Views.ViewStates.Gone;
        ValidateAyaInputs();
        _currentAya = _fromAya;
        LoadCurrentAya();
        UpdateProgress();
        if (_recognizedTextView != null) _recognizedTextView.Text = "";
    }


    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
    {
        base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

        if (requestCode == PERMISSION_REQUEST_RECORD_AUDIO)
        {
            if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
            {
                StartHifzSession();
            }
            else
            {
                ShowToast(_localizationService?.HifzMicPermissionDenied ?? "Microphone permission required");
            }
        }
    }

    private void StartHifzSession()
    {
        ValidateAyaInputs();
        _currentAya = _fromAya;
        _currentWordIndex = 0;
        _lastProcessedText = "";
        _isActive = true;

        LoadCurrentAya();
        UpdateButtonStates();
        UpdateStatus(_localizationService?.HifzListening ?? "Listening...");

        _speechService?.StartListening();
        StartIdleTimeout();
    }

    private void StopHifzSession()
    {
        _isActive = false;
        _speechService?.StopListening();
        UpdateButtonStates();
        UpdateStatus(_localizationService?.HifzStopped ?? "Stopped");
        StopIdleTimeout();
    }

    private void UpdateButtonStates()
    {
        if (_micButton != null) _micButton.Enabled = !_isActive;
        if (_stopButton != null) _stopButton.Enabled = _isActive;
    }

    // ── Speech Recognition Callbacks ─────────────────────────

    private void OnSpeechPartialResult(object? sender, string text)
    {
        RunOnUiThread(() =>
        {
            if (!_isActive) return;

            if (_recognizedTextView != null)
                _recognizedTextView.Text = text;

            // Process partial results for faster feedback
            ResetIdleTimeout();
            ProcessRecognizedText(text);
        });
    }

    private void OnSpeechFinalResult(object? sender, string text)
    {
        RunOnUiThread(() =>
        {
            if (!_isActive) return;

            if (_recognizedTextView != null)
                _recognizedTextView.Text = text;

            ResetIdleTimeout();
            ProcessRecognizedText(text);
        });
    }

    private void OnSpeechError(object? sender, string error)
    {
        RunOnUiThread(() =>
        {
            global::Android.Util.Log.Debug("Hifz", $"Speech error: {error}");
            // Don't show transient errors in UI — they auto-recover
            // Only log them for debugging
        });
    }

    private bool _engineErrorShown = false;

    private void OnSpeechEngineNotAvailable(object? sender, EventArgs e)
    {
        RunOnUiThread(() =>
        {
            // Stop idle timeout first to prevent it from firing
            StopIdleTimeout();
            _isActive = false;
            _speechService?.StopListening();
            UpdateButtonStates();
            UpdateStatus(_localizationService?.HifzStopped ?? "Stopped");
            
            if (!_engineErrorShown)
            {
                _engineErrorShown = true;
                PromptInstallSpeechEngine();
            }
        });
    }

    // ── Word Matching Logic ──────────────────────────────────

    private void ProcessRecognizedText(string recognizedText)
    {
        if (string.IsNullOrWhiteSpace(recognizedText) || _currentAyaWords.Count == 0)
            return;

        // Avoid processing the same text twice
        if (recognizedText == _lastProcessedText)
            return;
        _lastProcessedText = recognizedText;

        try
        {
            // Try two matching strategies and take the best result:
            // 1) From position 0: handles cumulative text within a single recognition cycle
            //    e.g. "بسم" -> "بسم الله" -> "بسم الله الرحمن"
            // 2) From _currentWordIndex: handles new recognition cycles after restart
            //    e.g. after ServerDisconnected, ASR sends fresh text for remaining words
            int fromStart = QuranWordMatcher.CountMatchedFromStart(
                recognizedText, _currentAyaWords, 2, 0);
            int fromCurrent = QuranWordMatcher.CountMatchedFromStart(
                recognizedText, _currentAyaWords, 2, _currentWordIndex);
            int totalMatched = Math.Max(fromStart, fromCurrent);

            global::Android.Util.Log.Debug("Hifz", $"Recognized: '{recognizedText}' | fromStart={fromStart}, fromCurrent={fromCurrent}, currentIdx={_currentWordIndex}/{_currentAyaWords.Count}");

            if (totalMatched > _currentWordIndex)
            {
                // New words matched!
                int newlyMatched = totalMatched - _currentWordIndex;
                _currentWordIndex = totalMatched;
                global::Android.Util.Log.Debug("Hifz", $"Advanced to word {_currentWordIndex} (+{newlyMatched})");

                VibrateCorrect();
                DisplayAyaProgress();
                UpdateProgress();

                // Check if aya is complete
                if (_currentWordIndex >= _currentAyaWords.Count)
                {
                    global::Android.Util.Log.Debug("Hifz", $"Aya {_currentAya} complete! ({_currentWordIndex}/{_currentAyaWords.Count})");
                    OnAyaComplete();
                }
            }
        }
        catch (Exception ex)
        {
            global::Android.Util.Log.Debug("Hifz", $"Exception in ProcessRecognizedText: {ex.Message}");
        }
    }

    private void OnAyaComplete()
    {
        global::Android.Util.Log.Debug("Hifz", $"Aya {_currentAya} complete!");

        UpdateStatus(string.Format(
            _localizationService?.HifzAyaComplete ?? "Aya {0} complete!",
            _currentAya));

        // Add completed Aya to history display
        if (_completedAyaText.Length > 0)
            _completedAyaText.AppendLine();
        _completedAyaText.Append($"﴿{_currentAya}﴾ {_currentAyaFullText}");
        if (_completedAyaHistory != null)
        {
            _completedAyaHistory.Text = _completedAyaText.ToString();
        }
        if (_completedAyaScrollView != null)
        {
            _completedAyaScrollView.Visibility = global::Android.Views.ViewStates.Visible;
            _completedAyaScrollView.Post(() => _completedAyaScrollView.FullScroll(global::Android.Views.FocusSearchDirection.Down));
        }

        // Move to next aya
        if (_currentAya < _toAya)
        {
            _currentAya++;
            _currentWordIndex = 0;
            _lastProcessedText = "";
            LoadCurrentAya();
            global::Android.Util.Log.Debug("Hifz", $"Loaded next aya {_currentAya}, words: {_currentAyaWords.Count}");
        }
        else
        {
            // All ayas done
            StopHifzSession();
            UpdateStatus(_localizationService?.HifzSessionComplete ?? "Session complete!");
            ShowToast(_localizationService?.HifzSessionComplete ?? "Session complete!");
        }
    }

    // ── Quran Text Loading & Display ─────────────────────────

    private void LoadCurrentAya()
    {
        global::Android.Util.Log.Debug("Hifz", $"LoadCurrentAya: Surah {_selectedSurah}, Aya {_currentAya}");
        _currentAyaFullText = GetAyaText(_selectedSurah, _currentAya);
        _currentAyaWords = QuranWordMatcher.TokenizeWords(_currentAyaFullText);
        global::Android.Util.Log.Debug("Hifz", $"Loaded Aya text: '{_currentAyaFullText}' -> {_currentAyaWords.Count} words");

        if (_currentAyaInfo != null)
        {
            var surahName = SurahInfo.GetSurahName(_selectedSurah);
            var surahLabel = _localizationService?["Surah"] ?? "Surah";
            var ayaLabel = _localizationService?["Aya"] ?? "Aya";
            _currentAyaInfo.Text = $"{surahLabel} {surahName} - {ayaLabel} {_currentAya}";
        }

        DisplayAyaProgress();
        UpdateProgress();
    }

    private void DisplayAyaProgress()
    {
        if (_ayaTextView == null || _currentAyaWords.Count == 0) return;

        try
        {
            var spannable = new SpannableStringBuilder();

            for (int i = 0; i < _currentAyaWords.Count; i++)
            {
                var word = _currentAyaWords[i];
                int start = spannable.Length();

                if (i < _currentWordIndex)
                {
                    // Revealed word — show with tajweed colors
                    spannable.Append(word);
                    int end = spannable.Length();

                    // Apply tajweed coloring if available
                    bool colorApplied = false;
                    if (_showTajweedRules && _searchService != null)
                    {
                        try
                        {
                            var matches = _searchService.SearchAyaForRules(word, _selectedTajweedRules);
                            if (matches.Count > 0)
                            {
                                foreach (var match in matches)
                                {
                                    var mStart = start + match.MatchStart;
                                    var mEnd = mStart + match.MatchLength;
                                    if (mStart >= start && mEnd <= end)
                                    {
                                        var textColor = GroupColors.TryGetValue(match.GroupName, out var tc) ? tc : unchecked((int)0xFF333333);
                                        var bgColor = GroupHighlightColors.TryGetValue(match.GroupName, out var bc) ? bc : 0;

                                        spannable.SetSpan(
                                            new ForegroundColorSpan(new Color(textColor)),
                                            mStart, mEnd, SpanTypes.ExclusiveExclusive);
                                        if (bgColor != 0)
                                        {
                                            spannable.SetSpan(
                                                new BackgroundColorSpan(new Color(bgColor)),
                                                mStart, mEnd, SpanTypes.ExclusiveExclusive);
                                        }
                                        colorApplied = true;
                                    }
                                }
                            }
                        }
                        catch { }
                    }

                    if (!colorApplied)
                    {
                        spannable.SetSpan(
                            new ForegroundColorSpan(Color.ParseColor("#1B5E20")),
                            start, end, SpanTypes.ExclusiveExclusive);
                    }
                }
                else if (i == _currentWordIndex && _isActive)
                {
                    // Current expected word — show as dots placeholder
                    var placeholder = new string('\u25CF', Math.Min(word.Length, 6));
                    spannable.Append(placeholder);
                    int end = spannable.Length();
                    spannable.SetSpan(
                        new ForegroundColorSpan(Color.ParseColor("#FF9800")),
                        start, end, SpanTypes.ExclusiveExclusive);
                    spannable.SetSpan(
                        new RelativeSizeSpan(0.8f),
                        start, end, SpanTypes.ExclusiveExclusive);
                }
                else
                {
                    // Future words — hidden dots
                    var placeholder = new string('\u2022', Math.Min(word.Length, 4));
                    spannable.Append(placeholder);
                    int end = spannable.Length();
                    spannable.SetSpan(
                        new ForegroundColorSpan(Color.ParseColor("#CCCCCC")),
                        start, end, SpanTypes.ExclusiveExclusive);
                    spannable.SetSpan(
                        new RelativeSizeSpan(0.7f),
                        start, end, SpanTypes.ExclusiveExclusive);
                }

                // Add space between words
                if (i < _currentAyaWords.Count - 1)
                    spannable.Append(" ");
            }

            _ayaTextView.TextFormatted = spannable;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Display error: {ex.Message}");
            _ayaTextView.Text = _currentAyaFullText;
        }
    }

    private void UpdateProgress()
    {
        int total = _currentAyaWords.Count;
        int current = Math.Min(_currentWordIndex, total);

        if (_wordProgressBar != null)
        {
            _wordProgressBar.Max = total > 0 ? total : 1;
            _wordProgressBar.Progress = current;
        }

        if (_progressText != null)
            _progressText.Text = $"{current} / {total}";
    }

    private void UpdateStatus(string text)
    {
        if (_statusLabel != null)
            _statusLabel.Text = text;
    }

    private string GetAyaText(int surah, int aya)
    {
        try
        {
            var fileService = new AndroidFileService(this);
            var content = fileService.ReadAllText("quran-uthmani.txt");
            var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var parts = line.Split('|');
                if (parts.Length >= 3)
                {
                    if (int.TryParse(parts[0], out int s) && int.TryParse(parts[1], out int a))
                    {
                        if (s == surah && a == aya)
                            return parts[2];
                    }
                }
            }
        }
        catch { }

        return $"سورة {SurahInfo.GetSurahName(surah)} - آية {aya}";
    }

    // ── Feedback (Vibration) ─────────────────────────────────

    private void VibrateCorrect()
    {
        try
        {
            if (_vibrator == null || !_vibrator.HasVibrator) return;

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                _vibrator.Vibrate(VibrationEffect.CreateOneShot(50, VibrationEffect.DefaultAmplitude));
            }
            else
            {
#pragma warning disable CS0618
                _vibrator.Vibrate(50);
#pragma warning restore CS0618
            }
        }
        catch { }
    }

    private void VibrateWrong()
    {
        try
        {
            if (_vibrator == null || !_vibrator.HasVibrator) return;

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                // Two short buzzes for wrong word
                var timings = new long[] { 0, 150, 100, 150 };
                var amplitudes = new int[] { 0, 255, 0, 255 };
                _vibrator.Vibrate(VibrationEffect.CreateWaveform(timings, amplitudes, -1));
            }
            else
            {
#pragma warning disable CS0618
                var pattern = new long[] { 0, 150, 100, 150 };
                _vibrator.Vibrate(pattern, -1);
#pragma warning restore CS0618
            }
        }
        catch { }
    }

    // ── Idle Timeout Management ──────────────────────────────

    private void StartIdleTimeout()
    {
        StopIdleTimeout();
        _idleTimeoutHandler = new Handler(Looper.MainLooper!);
        _idleTimeoutRunnable = new Java.Lang.Runnable(() =>
        {
            RunOnUiThread(() =>
            {
                if (_isActive)
                {
                    StopHifzSession();
                    UpdateStatus("Stopped due to inactivity");
                    ShowToast("Session stopped after 1 minute of silence");
                }
            });
        });
        _idleTimeoutHandler.PostDelayed(_idleTimeoutRunnable, IDLE_TIMEOUT_MS);
    }

    private void ResetIdleTimeout()
    {
        if (_isActive && _idleTimeoutHandler != null && _idleTimeoutRunnable != null)
        {
            _idleTimeoutHandler.RemoveCallbacks(_idleTimeoutRunnable);
            _idleTimeoutHandler.PostDelayed(_idleTimeoutRunnable, IDLE_TIMEOUT_MS);
        }
    }

    private void StopIdleTimeout()
    {
        if (_idleTimeoutHandler != null && _idleTimeoutRunnable != null)
        {
            _idleTimeoutHandler.RemoveCallbacks(_idleTimeoutRunnable);
        }
    }

    private void ShowWrongWordIndicator()
    {
        if (_wrongWordIndicator == null) return;

        RunOnUiThread(() =>
        {
            _wrongWordIndicator.Visibility = ViewStates.Visible;
            
            // Hide after 500ms
            var handler = new Handler(Looper.MainLooper!);
            handler.PostDelayed(() =>
            {
                if (_wrongWordIndicator != null)
                    _wrongWordIndicator.Visibility = ViewStates.Invisible;
            }, 500);
        });
    }

    // ── Helpers ───────────────────────────────────────────────

    private void ShowToast(string message)
    {
        Toast.MakeText(this, message, ToastLength.Short)?.Show();
    }

    protected override void OnDestroy()
    {
        StopIdleTimeout();
        _speechService?.StopListening();
        _speechService?.Dispose();
        base.OnDestroy();
    }
}
