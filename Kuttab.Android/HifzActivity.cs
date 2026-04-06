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
            var text = GetAyaText(r.Sura, r.Aya);
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
        _isActive = true;
        _lastProcessedText = "";

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
    }

    private void StopHifzSession()
    {
        _isActive = false;
        _speechService?.StopListening();
        if (_setupPanel != null) _setupPanel.Visibility = ViewStates.Visible;
        UpdateButtonStates();
        UpdateStatus(_localizationService?.HifzStopped ?? "Stopped");
        StopIdleTimeout();
    }

    private void UpdateButtonStates()
    {
        if (_micButton != null) _micButton.Enabled = !_isActive;
        if (_stopButton != null) _stopButton.Enabled = _isActive;
    }

    // ── Speech Callbacks ─────────────────────────────────────

    private void OnSpeechPartialResult(object? sender, string text)
    {
        RunOnUiThread(() => { if (!_isActive) return; if (_recognizedTextView != null) _recognizedTextView.Text = text; ResetIdleTimeout(); ProcessRecognizedText(text); });
    }

    private void OnSpeechFinalResult(object? sender, string text)
    {
        RunOnUiThread(() => { if (!_isActive) return; if (_recognizedTextView != null) _recognizedTextView.Text = text; ResetIdleTimeout(); ProcessRecognizedText(text); _lastProcessedText = ""; });
    }

    private void OnSpeechError(object? sender, string error)
    {
        RunOnUiThread(() => global::Android.Util.Log.Debug("Hifz", $"Speech error: {error}"));
    }

    private void OnSpeechEngineNotAvailable(object? sender, EventArgs e)
    {
        RunOnUiThread(() =>
        {
            StopIdleTimeout(); _isActive = false; _speechService?.StopListening();
            if (_setupPanel != null) _setupPanel.Visibility = ViewStates.Visible;
            UpdateButtonStates(); UpdateStatus(_localizationService?.HifzStopped ?? "Stopped");
            if (!_engineErrorShown) { _engineErrorShown = true; PromptInstallSpeechEngine(); }
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

            bool anyProgress = false;

            foreach (var recWord in recognizedWords)
            {
                if (_currentAyaIdx >= _pageAyas.Count) break;
                currentAya = _pageAyas[_currentAyaIdx];

                if (_currentWordIdx >= currentAya.Words.Count)
                {
                    if (!AdvanceToNextInRangeAya()) break;
                    currentAya = _pageAyas[_currentAyaIdx];
                }

                // Strategy 1: Match current expected word
                if (_currentWordIdx < currentAya.Words.Count &&
                    QuranWordMatcher.IsWordMatch(recWord, currentAya.Words[_currentWordIdx], 2))
                {
                    currentAya.WordStates[_currentWordIdx] = WordState.Revealed;
                    _currentWordIdx++;
                    anyProgress = true;
                    if (_currentWordIdx >= currentAya.Words.Count) OnAyaComplete();
                    continue;
                }

                // Strategy 2: Lookahead within current aya (skip 1-3 words)
                bool foundInAya = false;
                int maxLook = Math.Min(3, currentAya.Words.Count - _currentWordIdx - 1);
                for (int ahead = 1; ahead <= maxLook; ahead++)
                {
                    int checkIdx = _currentWordIdx + ahead;
                    if (checkIdx < currentAya.Words.Count &&
                        QuranWordMatcher.IsWordMatch(recWord, currentAya.Words[checkIdx], 2))
                    {
                        for (int skip = _currentWordIdx; skip < checkIdx; skip++)
                            currentAya.WordStates[skip] = WordState.Skipped;
                        VibrateWrongWord();
                        currentAya.WordStates[checkIdx] = WordState.Revealed;
                        _currentWordIdx = checkIdx + 1;
                        anyProgress = true;
                        foundInAya = true;
                        if (_currentWordIdx >= currentAya.Words.Count) OnAyaComplete();
                        break;
                    }
                }
                if (foundInAya) continue;

                // Strategy 3: Match against next in-range ayahs (detect skipped aya)
                bool foundLater = false;
                for (int ayaAhead = 1; ayaAhead <= 3; ayaAhead++)
                {
                    int nextIdx = FindNextInRangeAyaIdx(_currentAyaIdx + ayaAhead);
                    if (nextIdx < 0) break;

                    var nextAya = _pageAyas[nextIdx];
                    for (int w = 0; w < Math.Min(3, nextAya.Words.Count); w++)
                    {
                        if (QuranWordMatcher.IsWordMatch(recWord, nextAya.Words[w], 2))
                        {
                            SkipRemainingWords(currentAya);
                            for (int mid = _currentAyaIdx + 1; mid < nextIdx; mid++)
                                if (_pageAyas[mid].InRange) SkipAllWords(_pageAyas[mid]);
                            VibrateMissedAyah();

                            _currentAyaIdx = nextIdx;
                            _currentWordIdx = 0;
                            for (int s = 0; s < w; s++) nextAya.WordStates[s] = WordState.Skipped;
                            nextAya.WordStates[w] = WordState.Revealed;
                            _currentWordIdx = w + 1;
                            anyProgress = true;
                            foundLater = true;
                            if (_currentWordIdx >= nextAya.Words.Count) OnAyaComplete();
                            break;
                        }
                    }
                    if (foundLater) break;
                }
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
                if (!firstAya) spannable.Append(" ");
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
                    spannable.Append(aya.FullText);
                    int end = spannable.Length();
                    spannable.SetSpan(new ForegroundColorSpan(Color.ParseColor("#555555")),
                        start, end, SpanTypes.ExclusiveExclusive);
                }

                // Aya end marker
                int markerStart = spannable.Length();
                spannable.Append($" \u06DD{ConvertToArabicNumber(aya.AyaNumber)} ");
                int markerEnd = spannable.Length();
                spannable.SetSpan(new ForegroundColorSpan(Color.ParseColor("#888888")),
                    markerStart, markerEnd, SpanTypes.ExclusiveExclusive);
                spannable.SetSpan(new RelativeSizeSpan(0.85f),
                    markerStart, markerEnd, SpanTypes.ExclusiveExclusive);
            }

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
    // Wrong/missed word: two short buzzes
    // Missed ayah: long sustained vibration

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
