using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Speech;
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
using System.Linq;
using System.Text;
using LocalizationService = Kuttab.Core.Services.LocalizationService;

namespace Kuttab.Android;

[Activity(Label = "Hifz Mode", Theme = "@style/AppTheme")]
public class HifzActivity : AppCompatActivity
{
    private const int PERMISSION_REQUEST_RECORD_AUDIO = 2001;

    // Services
    private LocalizationService? _localizationService;
    private QuranSearchService? _searchService;
    private SpeechRecognitionService? _speechService;
    private QuranPageService? _pageService;
    private Vibrator? _vibrator;
    private PowerManager.WakeLock? _wakeLock;

    // UI Elements
    private TextView? _titleTextView;
    private TextView? _pageInfoText;
    private TextView? _statusLabel;
    private LinearLayout? _setupPanel;
    private TextView? _surahLabel;
    private Spinner? _surahSpinner;
    private TextView? _fromAyaLabel;
    private EditText? _fromAyaInput;
    private TextView? _toAyaLabel;
    private EditText? _toAyaInput;
    private ScrollView? _pageScrollView;
    private TextView? _pageTextView;
    private ProgressBar? _wordProgressBar;
    private TextView? _progressText;
    private LinearLayout? _recognizedPanel;
    private TextView? _recognizedTextView;
    private Button? _micButton;
    private Button? _stopButton;
    private Button? _resetButton;
    private Button? _prevPageButton;
    private Button? _nextPageButton;

    // Word state enum
    private enum WordState { Hidden, Revealed, Skipped }

    // Per-aya data on current page
    private class PageAyaData
    {
        public int Surah;
        public int AyaNumber;
        public string FullText = "";
        public List<string> Words = new();
        public List<WordState> WordStates = new();
        public bool InRange; // true if this aya is in the memorization range
    }

    // State
    private int _selectedSurah = 1;
    private int _fromAya = 1;
    private int _toAya = 7;
    private int _currentPage = 1;
    private int _currentAyaIdx = 0; // index into _pageAyas for current aya being recited
    private int _currentWordIdx = 0; // index into current aya's words
    private List<PageAyaData> _pageAyas = new();
    private bool _isActive = false;
    private string _lastProcessedText = "";
    private Handler? _idleTimeoutHandler;
    private Java.Lang.Runnable? _idleTimeoutRunnable;
    private const int IDLE_TIMEOUT_MS = 60000;
    private bool _engineErrorShown = false;
    private bool _sessionIntentionallyStopped = false;
    private int _consecutiveMisses = 0; // how many recognized words in a row didn't match next expected
    private int _lastRecWordCount = 0; // how many words from the last cumulative ASR result were already processed

    // Quran text cache: "surah|aya" -> text
    private Dictionary<string, string> _quranTextCache = new();
    private bool _quranTextLoaded = false;

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
        InitializeWakeLock();
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

            // Load page data from quran-data.xml
            _pageService = new QuranPageService();
            var xmlContent = fileService.ReadAllText("quran-data.xml");
            _pageService.LoadFromXml(xmlContent);

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

    private void InitializeWakeLock()
    {
        try
        {
            var powerManager = (PowerManager?)GetSystemService(PowerService);
            if (powerManager != null)
            {
                _wakeLock = powerManager.NewWakeLock(WakeLockFlags.ScreenBright, "Kuttab::HifzWakeLock");
                _wakeLock?.SetReferenceCounted(false);
            }
        }
        catch (Exception ex)
        {
            global::Android.Util.Log.Error("Hifz", $"Failed to initialize wake lock: {ex.Message}");
        }
    }

    private void InitializeViews()
    {
        _titleTextView = FindViewById<TextView>(Resource.Id.titleTextView);
        _pageInfoText = FindViewById<TextView>(Resource.Id.pageInfoText);
        _statusLabel = FindViewById<TextView>(Resource.Id.statusLabel);
        _setupPanel = FindViewById<LinearLayout>(Resource.Id.setupPanel);
        _surahLabel = FindViewById<TextView>(Resource.Id.surahLabel);
        _surahSpinner = FindViewById<Spinner>(Resource.Id.surahSpinner);
        _fromAyaLabel = FindViewById<TextView>(Resource.Id.fromAyaLabel);
        _fromAyaInput = FindViewById<EditText>(Resource.Id.fromAyaInput);
        _toAyaLabel = FindViewById<TextView>(Resource.Id.toAyaLabel);
        _toAyaInput = FindViewById<EditText>(Resource.Id.toAyaInput);
        _pageScrollView = FindViewById<ScrollView>(Resource.Id.pageScrollView);
        _pageTextView = FindViewById<TextView>(Resource.Id.pageTextView);
        _wordProgressBar = FindViewById<ProgressBar>(Resource.Id.wordProgressBar);
        _progressText = FindViewById<TextView>(Resource.Id.progressText);
        _recognizedPanel = FindViewById<LinearLayout>(Resource.Id.recognizedPanel);
        _recognizedTextView = FindViewById<TextView>(Resource.Id.recognizedTextView);
        _micButton = FindViewById<Button>(Resource.Id.micButton);
        _stopButton = FindViewById<Button>(Resource.Id.stopButton);
        _resetButton = FindViewById<Button>(Resource.Id.resetButton);
        _prevPageButton = FindViewById<Button>(Resource.Id.prevPageButton);
        _nextPageButton = FindViewById<Button>(Resource.Id.nextPageButton);

        if (_micButton != null) _micButton.Click += OnMicClick;
        if (_stopButton != null) _stopButton.Click += OnStopClick;
        if (_resetButton != null) _resetButton.Click += OnResetClick;
        if (_prevPageButton != null) _prevPageButton.Click += OnPrevPageClick;
        if (_nextPageButton != null) _nextPageButton.Click += OnNextPageClick;

        if (_fromAyaInput != null)
            _fromAyaInput.FocusChange += (s, e) => { if (!e.HasFocus) ValidateAndLoadPage(); };
        if (_toAyaInput != null)
            _toAyaInput.FocusChange += (s, e) => { if (!e.HasFocus) ValidateAndLoadPage(); };
    }

    private void LoadSettings()
    {
        var language = Kuttab.Android.Utils.LanguageHelper.GetLanguageFromPreferences(this);

        if (_localizationService != null)
        {
            _localizationService.CurrentLanguage = language;
            UpdateUIStrings();
            UpdateLayoutDirection(language);
        }
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
            // Cache all Quran text for fast page loading
            LoadQuranTextCache();
            RunOnUiThread(() => LoadCurrentPage());
        }
        catch (Exception ex)
        {
            RunOnUiThread(() => ShowToast($"Failed to load Quran data: {ex.Message}"));
        }
    }

    private void LoadQuranTextCache()
    {
        try
        {
            var fileService = new AndroidFileService(this);
            var content = fileService.ReadAllText("quran-uthmani.txt");
            var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var parts = line.Split('|');
                if (parts.Length >= 3 && int.TryParse(parts[0], out int s) && int.TryParse(parts[1], out int a))
                {
                    _quranTextCache[$"{s}|{a}"] = parts[2];
                }
            }
            _quranTextLoaded = true;
        }
        catch (Exception ex)
        {
            global::Android.Util.Log.Error("Hifz", $"Failed to cache Quran text: {ex.Message}");
        }
    }

    private string GetAyaText(int surah, int aya)
    {
        if (_quranTextCache.TryGetValue($"{surah}|{aya}", out var text))
            return text;
        return $"آية {aya}";
    }

    private void OnSurahSelected(object? sender, AdapterView.ItemSelectedEventArgs e)
    {
        _selectedSurah = e.Position + 1;
        UpdateAyaInputsForSurah();
        if (!_isActive) LoadCurrentPage();
    }

    private void UpdateAyaInputsForSurah()
    {
        int ayaCount = SurahInfo.GetAyaCount(_selectedSurah);
        _fromAya = 1;
        _toAya = ayaCount;
        if (_fromAyaInput != null) _fromAyaInput.Text = "1";
        if (_toAyaInput != null) _toAyaInput.Text = ayaCount.ToString();
    }

    private void ValidateAndLoadPage()
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

        if (!_isActive) LoadCurrentPage();
    }

    // ── Page Loading ────────────────────────────────────────

    private void LoadCurrentPage()
    {
        if (_pageService == null || !_quranTextLoaded) return;
        _currentPage = _pageService.GetPageForAya(_selectedSurah, _fromAya);
        LoadPageAyas(_currentPage);
        DisplayPage();
        UpdatePageInfo();
        UpdateProgress();
    }

    private void LoadPageAyas(int pageIndex)
    {
        _pageAyas.Clear();
        if (_pageService == null) return;

        var ayaRefs = _pageService.GetAyasOnPage(pageIndex);
        foreach (var r in ayaRefs)
        {
            var text = GetAyaText(r.Sura, r.Aya).TrimEnd();
            var words = QuranWordMatcher.TokenizeWords(text);
            bool inRange = (r.Sura == _selectedSurah && r.Aya >= _fromAya && r.Aya <= _toAya);

            var data = new PageAyaData
            {
                Surah = r.Sura, AyaNumber = r.Aya, FullText = text,
                Words = words, InRange = inRange,
                WordStates = words.Select(_ => inRange ? WordState.Hidden : WordState.Revealed).ToList()
            };
            _pageAyas.Add(data);
        }

        _currentAyaIdx = _pageAyas.FindIndex(a => a.InRange);
        if (_currentAyaIdx < 0) _currentAyaIdx = 0;
        _currentWordIdx = 0;
    }

    private void UpdatePageInfo()
    {
        if (_pageInfoText != null)
            _pageInfoText.Text = $"Page {_currentPage} / {_pageService?.TotalPages ?? 604}";
    }

    private void OnPrevPageClick(object? sender, EventArgs e)
    {
        if (_currentPage > 1) { _currentPage--; LoadPageAyas(_currentPage); DisplayPage(); UpdatePageInfo(); UpdateProgress(); }
    }

    private void OnNextPageClick(object? sender, EventArgs e)
    {
        if (_currentPage < (_pageService?.TotalPages ?? 604)) { _currentPage++; LoadPageAyas(_currentPage); DisplayPage(); UpdatePageInfo(); UpdateProgress(); }
    }

    // ── Mic Control & Session ────────────────────────────────

    private void OnMicClick(object? sender, EventArgs e)
    {
        if (_speechService == null) return;
        if (ContextCompat.CheckSelfPermission(this, global::Android.Manifest.Permission.RecordAudio) != Permission.Granted)
        {
            ActivityCompat.RequestPermissions(this, new[] { global::Android.Manifest.Permission.RecordAudio }, PERMISSION_REQUEST_RECORD_AUDIO);
            return;
        }
        StartHifzSession();
    }

    private void OnStopClick(object? sender, EventArgs e) => StopHifzSession();

    private void OnResetClick(object? sender, EventArgs e)
    {
        StopHifzSession();
        _lastProcessedText = "";
        _lastRecWordCount = 0;
        _consecutiveMisses = 0;
        LoadPageAyas(_currentPage);
        DisplayPage();
        UpdateProgress();
        if (_recognizedTextView != null) _recognizedTextView.Text = "";
        if (_recognizedPanel != null) _recognizedPanel.Visibility = ViewStates.Gone;
    }

    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
    {
        base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        if (requestCode == PERMISSION_REQUEST_RECORD_AUDIO)
        {
            if (grantResults.Length > 0 && grantResults[0] == Permission.Granted) StartHifzSession();
            else ShowToast(_localizationService?.HifzMicPermissionDenied ?? "Microphone permission required");
        }
    }

    private void StartHifzSession()
    {
        _engineErrorShown = false;
        _sessionIntentionallyStopped = false;
        _isActive = true;
        _lastProcessedText = "";
        _lastRecWordCount = 0;
        _consecutiveMisses = 0;

        foreach (var aya in _pageAyas)
            if (aya.InRange)
                for (int i = 0; i < aya.WordStates.Count; i++)
                    aya.WordStates[i] = WordState.Hidden;

        _currentAyaIdx = _pageAyas.FindIndex(a => a.InRange);
        if (_currentAyaIdx < 0) _currentAyaIdx = 0;
        _currentWordIdx = 0;

        if (_setupPanel != null) _setupPanel.Visibility = ViewStates.Gone;
        if (_recognizedPanel != null) _recognizedPanel.Visibility = ViewStates.Visible;

        DisplayPage();
        UpdateButtonStates();
        UpdateStatus(_localizationService?.HifzListening ?? "Listening...");
        _speechService?.StartListening();
        StartIdleTimeout();
        AcquireWakeLock();
        
        // Additional screen-on mechanism using Window flag
        if (Window != null)
            Window.AddFlags(WindowManagerFlags.KeepScreenOn);
    }

    private void StopHifzSession()
    {
        _sessionIntentionallyStopped = true;
        _isActive = false;
        _speechService?.StopListening();
        if (_setupPanel != null) _setupPanel.Visibility = ViewStates.Visible;
        UpdateButtonStates();
        UpdateStatus(_localizationService?.HifzStopped ?? "Stopped");
        StopIdleTimeout();
        ReleaseWakeLock();
        
        // Remove screen-on flag
        if (Window != null)
            Window.ClearFlags(WindowManagerFlags.KeepScreenOn);
    }

    private void UpdateButtonStates()
    {
        if (_micButton != null) _micButton.Enabled = !_isActive;
        if (_stopButton != null) _stopButton.Enabled = _isActive;
    }

    // ── Speech Callbacks ─────────────────────────────────────

    private void OnSpeechPartialResult(object? sender, List<string> alternatives)
    {
        RunOnUiThread(() => 
        { 
            if (!_isActive) return; 
            if (_recognizedTextView != null) _recognizedTextView.Text = alternatives[0]; 
            ResetIdleTimeout(); 
            ProcessRecognizedTextWithAlternatives(alternatives); 
        });
    }

    private void OnSpeechFinalResult(object? sender, List<string> alternatives)
    {
        RunOnUiThread(() => 
        { 
            if (!_isActive) return; 
            if (_recognizedTextView != null) _recognizedTextView.Text = alternatives[0]; 
            ResetIdleTimeout(); 
            ProcessRecognizedTextWithAlternatives(alternatives); 
            _lastProcessedText = ""; 
            _lastRecWordCount = 0; 
        });
    }

    private void OnSpeechError(object? sender, string error)
    {
        RunOnUiThread(() => global::Android.Util.Log.Debug("Hifz", $"Speech error: {error}"));
    }

    private void OnSpeechEngineNotAvailable(object? sender, EventArgs e)
    {
        RunOnUiThread(() =>
        {
            global::Android.Util.Log.Debug("Hifz", "SpeechEngineNotAvailable event received (suppressed dialog)");
            // Just stop cleanly — never show the install dialog
            // ServerDisconnected errors are transient and don't mean the engine is missing
            StopIdleTimeout(); _isActive = false; _speechService?.StopListening();
            if (_setupPanel != null) _setupPanel.Visibility = ViewStates.Visible;
            UpdateButtonStates(); UpdateStatus(_localizationService?.HifzStopped ?? "Stopped");
        });
    }

    private void PromptInstallSpeechEngine()
    {
        var builder = new AndroidX.AppCompat.App.AlertDialog.Builder(this);
        builder.SetTitle(_localizationService?.HifzNoSpeechEngine ?? "Speech Recognition");
        builder.SetMessage("Arabic speech recognition is not available.\n\n1. Install/update Google app\n2. Download Arabic offline speech data\n\nThen restart and try again.");
        builder.SetPositiveButton("Open Store", (s, ev) => {
            try { StartActivity(new Intent(Intent.ActionView, global::Android.Net.Uri.Parse("market://details?id=com.google.android.googlequicksearchbox")).AddFlags(ActivityFlags.NewTask)); }
            catch { ShowToast("Could not open store"); }
        });
        builder.SetNegativeButton("OK", (s, ev) => { });
        builder.Show();
    }

    // ── Word Matching with Skip Support ─────────────────────

    private void ProcessRecognizedTextWithAlternatives(List<string> alternatives)
    {
        if (alternatives == null || alternatives.Count == 0) return;

        // Try each alternative until one produces a match
        int initialWordIdx = _currentWordIdx;
        int initialAyaIdx = _currentAyaIdx;
        
        foreach (var alternative in alternatives)
        {
            // Try this alternative
            ProcessRecognizedText(alternative);
            
            // If we made progress, stop trying alternatives
            if (_currentWordIdx > initialWordIdx || _currentAyaIdx > initialAyaIdx)
            {
                global::Android.Util.Log.Debug("Hifz", $"Alternative matched: '{alternative}' (tried {alternatives.IndexOf(alternative) + 1}/{alternatives.Count})");
                return;
            }
        }
        
        // No alternative produced a match
        global::Android.Util.Log.Debug("Hifz", $"No alternative matched from {alternatives.Count} options");
    }

    private void ProcessRecognizedText(string recognizedText)
    {
        if (string.IsNullOrWhiteSpace(recognizedText) || _pageAyas.Count == 0) return;
        if (recognizedText == _lastProcessedText) return;
        _lastProcessedText = recognizedText;
        if (_currentAyaIdx < 0 || _currentAyaIdx >= _pageAyas.Count) return;

        try
        {
            var currentAya = _pageAyas[_currentAyaIdx];
            if (!currentAya.InRange || currentAya.Words.Count == 0)
            {
                AdvanceToNextInRangeAya();
                return;
            }

            var recognizedWords = QuranWordMatcher.TokenizeWords(recognizedText);
            if (recognizedWords.Count == 0) return;

            // ASR sends cumulative partial results: "بسم" -> "بسم الله" -> "بسم الله الرحمن"
            // Only process NEW words to avoid re-matching already-processed words
            int startFrom = _lastRecWordCount;
            if (recognizedWords.Count < _lastRecWordCount)
                startFrom = 0; // new recognition cycle — ASR restarted
            _lastRecWordCount = recognizedWords.Count;

            bool anyProgress = false;

            for (int ri = startFrom; ri < recognizedWords.Count; ri++)
            {
                var recWord = recognizedWords[ri];
                if (_currentAyaIdx >= _pageAyas.Count) break;
                currentAya = _pageAyas[_currentAyaIdx];

                if (_currentWordIdx >= currentAya.Words.Count)
                {
                    if (!AdvanceToNextInRangeAya()) break;
                    currentAya = _pageAyas[_currentAyaIdx];
                }

                // Priority 1: Match the NEXT expected word
                if (_currentWordIdx < currentAya.Words.Count &&
                    QuranWordMatcher.IsWordMatch(recWord, currentAya.Words[_currentWordIdx], 2))
                {
                    currentAya.WordStates[_currentWordIdx] = WordState.Revealed;
                    _currentWordIdx++;
                    // No vibration on correct match - only vibrate on errors
                    anyProgress = true;
                    if (_currentWordIdx >= currentAya.Words.Count) OnAyaComplete();
                    continue;
                }

                // Priority 2: Recovery - check if recognized word matches any of the next words
                // This handles ASR mistakes (missed first word, wrong word detection)
                // Use larger lookahead (5 words) to handle speech recognition startup delay
                bool foundAhead = false;
                int lookAhead = Math.Min(5, currentAya.Words.Count - _currentWordIdx);
                for (int ahead = 1; ahead < lookAhead; ahead++)
                {
                    int checkIdx = _currentWordIdx + ahead;
                    if (QuranWordMatcher.IsWordMatch(recWord, currentAya.Words[checkIdx], 2))
                    {
                        // Found a match ahead - auto-skip the missed words
                        int skippedCount = 0;
                        for (int skip = _currentWordIdx; skip < checkIdx; skip++)
                        {
                            currentAya.WordStates[skip] = WordState.Skipped;
                            skippedCount++;
                        }
                        currentAya.WordStates[checkIdx] = WordState.Revealed;
                        _currentWordIdx = checkIdx + 1;
                        
                        // Vibrate for skipped words
                        if (skippedCount > 0)
                            VibrateWrongWord();
                        
                        anyProgress = true;
                        foundAhead = true;
                        if (_currentWordIdx >= currentAya.Words.Count) OnAyaComplete();
                        break;
                    }
                }
                if (foundAhead) continue;
                
                // No match found - vibrate for unmatched recognized word
                VibrateWrongWord();
            }

            if (anyProgress) { DisplayPage(); UpdateProgress(); }
        }
        catch (Exception ex)
        {
            global::Android.Util.Log.Debug("Hifz", $"ProcessRecognizedText error: {ex.Message}");
        }
    }

    private void OnAyaComplete()
    {
        if (_currentAyaIdx >= 0 && _currentAyaIdx < _pageAyas.Count)
        {
            var aya = _pageAyas[_currentAyaIdx];
            UpdateStatus($"Aya {aya.AyaNumber} complete!");
        }

        if (!AdvanceToNextInRangeAya())
        {
            // Check if there are more pages with in-range ayas
            bool allDone = true;
            foreach (var a in _pageAyas)
                if (a.InRange && a.WordStates.Any(ws => ws == WordState.Hidden))
                { allDone = false; break; }

            if (allDone)
            {
                StopHifzSession();
                UpdateStatus(_localizationService?.HifzSessionComplete ?? "Session complete!");
                ShowToast(_localizationService?.HifzSessionComplete ?? "Session complete!");
            }
        }
    }

    private bool AdvanceToNextInRangeAya()
    {
        for (int i = _currentAyaIdx + 1; i < _pageAyas.Count; i++)
        {
            if (_pageAyas[i].InRange && _pageAyas[i].WordStates.Any(ws => ws == WordState.Hidden))
            {
                _currentAyaIdx = i;
                _currentWordIdx = 0;
                _lastProcessedText = "";
                _lastRecWordCount = 0;
                _consecutiveMisses = 0;
                return true;
            }
        }
        return false;
    }

    private int FindNextInRangeAyaIdx(int startFrom)
    {
        for (int i = startFrom; i < _pageAyas.Count; i++)
            if (_pageAyas[i].InRange) return i;
        return -1;
    }

    private void SkipRemainingWords(PageAyaData aya)
    {
        for (int i = _currentWordIdx; i < aya.WordStates.Count; i++)
            if (aya.WordStates[i] == WordState.Hidden) aya.WordStates[i] = WordState.Skipped;
    }

    private void SkipAllWords(PageAyaData aya)
    {
        for (int i = 0; i < aya.WordStates.Count; i++)
            if (aya.WordStates[i] == WordState.Hidden) aya.WordStates[i] = WordState.Skipped;
    }

    // ── Display ──────────────────────────────────────────────

    private void DisplayPage()
    {
        if (_pageTextView == null || _pageAyas.Count == 0) return;

        try
        {
            var spannable = new SpannableStringBuilder();
            bool firstAya = true;

            foreach (var aya in _pageAyas)
            {
                // Check if this is Bismillah (first ayah of every surah except At-Tawba/9)
                bool isBismillah = (aya.AyaNumber == 1 && aya.Surah != 9);
                // Check if this is the last ayah of a surah
                bool isLastAyahOfSurah = (aya.AyaNumber == SurahInfo.GetAyaCount(aya.Surah));
                
                // Display surah name for ayah 1 (all surahs including Surah 9)
                if (aya.AyaNumber == 1)
                {
                    // Only insert a newline before Surah name if previous character is not whitespace
                    if (!firstAya && spannable.Length() > 0)
                    {
                        char prev = spannable.ToString()[spannable.Length() - 1];
                        if (prev != '\n' && prev != ' ')
                            spannable.Append("\n");
                        else if (prev == ' ')
                        {
                            // Replace trailing space with newline
                            spannable.Delete(spannable.Length() - 1, spannable.Length());
                            spannable.Append("\n");
                        }
                    }
                    global::Android.Util.Log.Info("HifzSurah", $"=== SURAH {aya.Surah} NAME BLOCK ===");
                    string surahName = $"﴿ سورة {SurahInfo.GetSurahName(aya.Surah)} ﴾";
                    int nameStart = spannable.Length();
                    spannable.Append(surahName);
                    int nameEnd = spannable.Length();
                    global::Android.Util.Log.Info("HifzSurah", $"Setting green color span from {nameStart} to {nameEnd - 1}");
                    spannable.SetSpan(new ForegroundColorSpan(Color.ParseColor("#1B5E20")),
                        nameStart, nameEnd - 1, SpanTypes.ExclusiveExclusive);
                    spannable.SetSpan(new RelativeSizeSpan(1.15f),
                        nameStart, nameEnd - 1, SpanTypes.ExclusiveExclusive);
                    spannable.SetSpan(new StyleSpan(global::Android.Graphics.TypefaceStyle.Bold),
                        nameStart, nameEnd - 1, SpanTypes.ExclusiveExclusive);
                    spannable.SetSpan(new global::Android.Text.Style.AlignmentSpanStandard(global::Android.Text.Layout.Alignment.AlignCenter),
                        nameStart, nameEnd, SpanTypes.ExclusiveExclusive);
                    // Only add newline if this is not the last ayah of the surah on the last line of the page
                    bool isLastAyaOnPage = aya == _pageAyas.Last();
                    bool isLastAyahOfSurahForPage = (aya.AyaNumber == SurahInfo.GetAyaCount(aya.Surah));
                    if (!(isLastAyaOnPage && isLastAyahOfSurahForPage))
                    {
                        if (spannable.Length() > 0 && spannable.ToString()[spannable.Length() - 1] != '\n')
                            spannable.Append("\n");
                    }
                    firstAya = false;
                }
                
                if (isBismillah)
                {
                    // Bismillah is the first 4 words of ayah 1: بِسْمِ ٱللَّهِ ٱلرَّحْمَـٰنِ ٱلرَّحِيمِ
                    // Split them onto their own line, then render remaining words as ayah text
                    int bismillahWordCount = 4;
                    
                    // --- Render the Bismillah phrase (first 4 words) on its own line ---
                    int bismillahStart = spannable.Length();
                    if (aya.InRange)
                    {
                        for (int i = 0; i < Math.Min(bismillahWordCount, aya.Words.Count); i++)
                        {
                            int start = spannable.Length();
                            var word = aya.Words[i];
                            var state = aya.WordStates[i];

                            if (state == WordState.Revealed)
                            {
                                spannable.Append(word);
                                int end = spannable.Length();
                                spannable.SetSpan(new ForegroundColorSpan(Color.ParseColor("#1B5E20")),
                                    start, end, SpanTypes.ExclusiveExclusive);
                            }
                            else if (state == WordState.Skipped)
                            {
                                spannable.Append(word);
                                int end = spannable.Length();
                                spannable.SetSpan(new ForegroundColorSpan(Color.ParseColor("#C62828")),
                                    start, end, SpanTypes.ExclusiveExclusive);
                            }
                            else
                            {
                                var dots = new string('\u00B7', Math.Max(word.Length / 2, 1));
                                spannable.Append(dots);
                                int end = spannable.Length();
                                spannable.SetSpan(new ForegroundColorSpan(Color.ParseColor("#D0D0D0")),
                                    start, end, SpanTypes.ExclusiveExclusive);
                            }

                            // Use non-breaking space between Bismillah words to keep on single line
                            if (i < bismillahWordCount - 1 && i < aya.Words.Count - 1)
                                spannable.Append("\u00A0");
                        }
                    }
                    else
                    {
                        // Not in range - show Bismillah text in gray
                        string bismillahText = string.Join("\u00A0", aya.Words.Take(Math.Min(bismillahWordCount, aya.Words.Count)));
                        spannable.Append(bismillahText);
                        int end = spannable.Length();
                        spannable.SetSpan(new ForegroundColorSpan(Color.ParseColor("#555555")),
                            bismillahStart, end, SpanTypes.ExclusiveExclusive);
                    }
                    
                    // Center-align the Bismillah line
                    spannable.SetSpan(new global::Android.Text.Style.AlignmentSpanStandard(global::Android.Text.Layout.Alignment.AlignCenter),
                        bismillahStart, spannable.Length(), SpanTypes.ExclusiveExclusive);
                    
                    // Newline after Bismillah phrase
                    if (spannable.Length() > 0 && spannable.ToString()[spannable.Length() - 1] != '\n')
                        spannable.Append("\n");
                    
                    firstAya = false;
                    
                    // --- Render remaining words of ayah 1 (after Bismillah) as normal ayah text ---
                    if (aya.Words.Count > bismillahWordCount)
                    {
                        if (aya.InRange)
                        {
                            for (int i = bismillahWordCount; i < aya.Words.Count; i++)
                            {
                                int start = spannable.Length();
                                var word = aya.Words[i];
                                var state = aya.WordStates[i];

                                if (state == WordState.Revealed)
                                {
                                    spannable.Append(word);
                                    int end = spannable.Length();
                                    spannable.SetSpan(new ForegroundColorSpan(Color.ParseColor("#1B5E20")),
                                        start, end, SpanTypes.ExclusiveExclusive);
                                }
                                else if (state == WordState.Skipped)
                                {
                                    spannable.Append(word);
                                    int end = spannable.Length();
                                    spannable.SetSpan(new ForegroundColorSpan(Color.ParseColor("#C62828")),
                                        start, end, SpanTypes.ExclusiveExclusive);
                                }
                                else
                                {
                                    var dots = new string('\u00B7', Math.Max(word.Length / 2, 1));
                                    spannable.Append(dots);
                                    int end = spannable.Length();
                                    spannable.SetSpan(new ForegroundColorSpan(Color.ParseColor("#D0D0D0")),
                                        start, end, SpanTypes.ExclusiveExclusive);
                                }

                                if (i < aya.Words.Count - 1) spannable.Append(" ");
                            }
                        }
                        else
                        {
                            // Not in range - show remaining text in gray
                            string remainingText = string.Join(" ", aya.Words.Skip(bismillahWordCount));
                            int start = spannable.Length();
                            spannable.Append(remainingText);
                            int end = spannable.Length();
                            spannable.SetSpan(new ForegroundColorSpan(Color.ParseColor("#555555")),
                                start, end, SpanTypes.ExclusiveExclusive);
                        }
                        
                        // Add aya marker after the remaining text
                        int ayaMarkerStart = spannable.Length();
                        spannable.Append($" \u06DD{ConvertToArabicNumber(aya.AyaNumber)} ");
                        int ayaMarkerEnd = spannable.Length();
                        spannable.SetSpan(new ForegroundColorSpan(Color.ParseColor("#888888")),
                            ayaMarkerStart, ayaMarkerEnd, SpanTypes.ExclusiveExclusive);
                        spannable.SetSpan(new RelativeSizeSpan(0.85f),
                            ayaMarkerStart, ayaMarkerEnd, SpanTypes.ExclusiveExclusive);
                        
                        // Newline after last ayah handled by next surah's name block
                    }
                    
                    continue;
                }
                
                if (!firstAya && aya.AyaNumber != 1) spannable.Append(" ");
                firstAya = false;

                if (aya.InRange)
                {
                    // Render words based on their state
                    for (int i = 0; i < aya.Words.Count; i++)
                    {
                        int start = spannable.Length();
                        var word = aya.Words[i];
                        var state = aya.WordStates[i];

                        if (state == WordState.Revealed)
                        {
                            spannable.Append(word);
                            int end = spannable.Length();
                            spannable.SetSpan(new ForegroundColorSpan(Color.ParseColor("#1B5E20")),
                                start, end, SpanTypes.ExclusiveExclusive);
                        }
                        else if (state == WordState.Skipped)
                        {
                            // Show skipped words in red so user sees what they missed
                            spannable.Append(word);
                            int end = spannable.Length();
                            spannable.SetSpan(new ForegroundColorSpan(Color.ParseColor("#C62828")),
                                start, end, SpanTypes.ExclusiveExclusive);
                        }
                        else
                        {
                            // Hidden: show dots matching word length
                            var dots = new string('\u00B7', Math.Max(word.Length / 2, 1));
                            spannable.Append(dots);
                            int end = spannable.Length();
                            spannable.SetSpan(new ForegroundColorSpan(Color.ParseColor("#D0D0D0")),
                                start, end, SpanTypes.ExclusiveExclusive);
                        }

                        if (i < aya.Words.Count - 1) spannable.Append(" ");
                    }
                }
                else
                {
                    // Not in range — show full text in normal color
                    int start = spannable.Length();
                    spannable.Append(aya.FullText.TrimEnd());
                    int end = spannable.Length();
                    spannable.SetSpan(new ForegroundColorSpan(Color.ParseColor("#555555")),
                        start, end, SpanTypes.ExclusiveExclusive);
                }

                // Aya end marker (skip for Bismillah as it's already added above)
                int markerStart = spannable.Length();
                spannable.Append($" \u06DD{ConvertToArabicNumber(aya.AyaNumber)} ");
                int markerEnd = spannable.Length();
                spannable.SetSpan(new ForegroundColorSpan(Color.ParseColor("#888888")),
                    markerStart, markerEnd, SpanTypes.ExclusiveExclusive);
                spannable.SetSpan(new RelativeSizeSpan(0.85f),
                    markerStart, markerEnd, SpanTypes.ExclusiveExclusive);
                
                // Newline after last ayah handled by next surah's name block
            }

            // Debug: dump spannable to find hidden newlines
            string dbg = spannable.ToString().Replace("\n", "⏎");
            global::Android.Util.Log.Info("HifzSpan", $"FULL TEXT: {dbg}");
            _pageTextView.TextFormatted = spannable;
        }
        catch (Exception ex)
        {
            global::Android.Util.Log.Error("Hifz", $"DisplayPage error: {ex.Message}");
        }
    }

    private static string ConvertToArabicNumber(int number)
    {
        var arabicDigits = new[] { '٠', '١', '٢', '٣', '٤', '٥', '٦', '٧', '٨', '٩' };
        var sb = new StringBuilder();
        foreach (char c in number.ToString())
            sb.Append(char.IsDigit(c) ? arabicDigits[c - '0'] : c);
        return sb.ToString();
    }

    private void UpdateProgress()
    {
        int totalWords = 0, revealedWords = 0;
        foreach (var aya in _pageAyas)
        {
            if (!aya.InRange) continue;
            totalWords += aya.Words.Count;
            revealedWords += aya.WordStates.Count(s => s == WordState.Revealed);
        }

        if (_wordProgressBar != null)
        {
            _wordProgressBar.Max = totalWords > 0 ? totalWords : 1;
            _wordProgressBar.Progress = revealedWords;
        }
        if (_progressText != null)
            _progressText.Text = totalWords > 0 ? $"{revealedWords}/{totalWords}" : "";
    }

    private void UpdateStatus(string text)
    {
        if (_statusLabel != null) _statusLabel.Text = text;
    }

    // ── Vibration Patterns ───────────────────────────────────
    // Correct word: single short buzz
    // Wrong/missed word: two short buzzes
    // Missed ayah: long sustained vibration

    private void VibrateCorrect()
    {
        try
        {
            if (_vibrator == null || !_vibrator.HasVibrator) return;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                _vibrator.Vibrate(VibrationEffect.CreateOneShot(40, VibrationEffect.DefaultAmplitude));
            else
            {
#pragma warning disable CS0618
                _vibrator.Vibrate(40);
#pragma warning restore CS0618
            }
        }
        catch { }
    }

    private void VibrateWrongWord()
    {
        try
        {
            if (_vibrator == null || !_vibrator.HasVibrator) return;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var timings = new long[] { 0, 100, 80, 100 };
                var amplitudes = new int[] { 0, 200, 0, 200 };
                _vibrator.Vibrate(VibrationEffect.CreateWaveform(timings, amplitudes, -1));
            }
            else
            {
#pragma warning disable CS0618
                _vibrator.Vibrate(new long[] { 0, 100, 80, 100 }, -1);
#pragma warning restore CS0618
            }
        }
        catch { }
    }

    private void VibrateMissedAyah()
    {
        try
        {
            if (_vibrator == null || !_vibrator.HasVibrator) return;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var timings = new long[] { 0, 300, 100, 300 };
                var amplitudes = new int[] { 0, 255, 0, 255 };
                _vibrator.Vibrate(VibrationEffect.CreateWaveform(timings, amplitudes, -1));
            }
            else
            {
#pragma warning disable CS0618
                _vibrator.Vibrate(new long[] { 0, 300, 100, 300 }, -1);
#pragma warning restore CS0618
            }
        }
        catch { }
    }

    // ── Idle Timeout ─────────────────────────────────────────

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
            _idleTimeoutHandler.RemoveCallbacks(_idleTimeoutRunnable);
    }

    // ── Wake Lock Management ─────────────────────────────────

    private void AcquireWakeLock()
    {
        try
        {
            if (_wakeLock != null && !_wakeLock.IsHeld)
            {
                _wakeLock.Acquire();
                global::Android.Util.Log.Debug("Hifz", "Wake lock acquired - screen will stay on");
            }
        }
        catch (Exception ex)
        {
            global::Android.Util.Log.Error("Hifz", $"Failed to acquire wake lock: {ex.Message}");
        }
    }

    private void ReleaseWakeLock()
    {
        try
        {
            if (_wakeLock != null && _wakeLock.IsHeld)
            {
                _wakeLock.Release();
                global::Android.Util.Log.Debug("Hifz", "Wake lock released");
            }
        }
        catch (Exception ex)
        {
            global::Android.Util.Log.Error("Hifz", $"Failed to release wake lock: {ex.Message}");
        }
    }

    // ── Helpers ───────────────────────────────────────────────

    private void ShowToast(string message)
    {
        Toast.MakeText(this, message, ToastLength.Short)?.Show();
    }

    protected override void OnDestroy()
    {
        StopIdleTimeout();
        ReleaseWakeLock();
        
        // Ensure screen-on flag is cleared
        if (Window != null)
            Window.ClearFlags(WindowManagerFlags.KeepScreenOn);
        
        _speechService?.StopListening();
        _speechService?.Dispose();
        base.OnDestroy();
    }
}
