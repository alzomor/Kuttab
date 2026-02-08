using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using Kuttab.Android.Services;
using Kuttab.Core.Models;
using Kuttab.Core.Services;
using Kuttab.Android.Utils;
using LocalizationService = Kuttab.Core.Services.LocalizationService;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Text;

namespace Kuttab.Android;

[Activity(Label = "@string/recitation_mode", Theme = "@style/AppTheme")]
public class RecitationActivity : AppCompatActivity
{
    // Services
    private AndroidAudioService? _audioService;
    private AndroidPictureService? _pictureService;
    private QuranSearchService? _searchService;
    private LocalizationService? _localizationService;
    
    // UI Elements
    private Button? _backButton;
    private Spinner? _reciterSpinner;
    private Spinner? _surahSpinner;
    private EditText? _fromAyaInput;
    private EditText? _toAyaInput;
    private Button? _startButton;
    private Button? _stopButton;
    private EditText? _repeatCountInput;
    private Spinner? _repeatModeSpinner;
    private TextView? _currentAyaInfo;
    private TextView? _ayaTextView;
    private ImageView? _ayaImage;
    private CheckBox? _teacherModeCheckbox;
    
        
    // Labels for dynamic language update
    private TextView? _titleTextView;
    private TextView? _reciterLabel;
    private TextView? _surahLabel;
    private TextView? _fromAyaLabel;
    private TextView? _toAyaLabel;
    private TextView? _nowPlayingLabel;
    
    // State
    private int _selectedSurah = 1;
    private int _fromAya = 1;
    private int _toAya = 7;
    private int _currentSurah = 1;
    private int _currentAya = 1;
    private bool _isPlaying = false;
    private bool _needsBasmalah = false;
    private bool _playingBasmalah = false;
    private string _selectedReciter = "Husary_128kbps";
    private List<QuranAya> _quranData = new();
    
    // Repeat count state
    private int _repeatCount = 1;
    private int _currentRepeat = 1;
    private int _currentAyaRepeat = 1;
    private bool _repeatEachAya = false;
    private bool _teacherModeEnabled = false;
    private DateTime _currentAyaStartTime = DateTime.UtcNow;
    private PowerManager.WakeLock? _wakeLock;
    
    // Reciters list (folder key + localization display key)
    private readonly List<(string FolderKey, string DisplayKey)> _reciters = new()
    {
        ("Abdul_Basit_Murattal_192kbps", "ReciterAbdulBasit"),
        ("Ayman_Sowaid_64kbps", "ReciterAymanSowaid"),
        ("Husary_128kbps", "ReciterHusary"),
        ("Husary_Muallim_128kbps", "ReciterHusaryMuallim"),
        ("Menshawi_32kbps", "ReciterMenshawi"),
        ("Mohammad_al_Tablaway_128kbps", "ReciterTablaway"),
        ("Mustafa_Ismail_48kbps", "ReciterMustafaIsmail"),
        ("Muhammad_Ayyoub_128kbps", "ReciterAyyoub"),
        ("mahmoud_ali_al_banna_32kbps", "ReciterBanna")
    };
    
    private const int SETTINGS_REQUEST_CODE = 1002;
    
    // Surah At-Tawbah number (no Basmalah)
    private const int SURAH_TAWBAH = 9;
    // Surah Al-Fatiha number (Basmalah is part of it)
    private const int SURAH_FATIHA = 1;
    
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        
        // Handle system UI for edge-to-edge on Android 15/16
        if (Window != null)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                Window.SetStatusBarColor(global::Android.Graphics.Color.ParseColor("#0D3F13"));
                Window.SetNavigationBarColor(global::Android.Graphics.Color.ParseColor("#1B5E20"));
            }
        }
        
        SetContentView(Resource.Layout.activity_recitation);
        
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
            _audioService = new AndroidAudioService(this);
            _pictureService = new AndroidPictureService(this);
            _searchService = new QuranSearchService(fileService);
            _localizationService = new LocalizationService(fileService);
            
            // Initialize wake lock to keep screen on during recitation
            var powerManager = (PowerManager?)GetSystemService(PowerService);
            if (powerManager != null)
            {
                _wakeLock = powerManager.NewWakeLock(WakeLockFlags.ScreenDim, "QuranSearch::RecitationWakeLock");
            }
            
            if (_audioService != null)
            {
                _audioService.PlaybackStateChanged += OnPlaybackStateChanged;
                _audioService.SequencePlaybackEnded += OnAyaPlaybackEnded;
                _audioService.PlaybackError += (s, msg) => ShowError(msg);
            }
        }
        catch (Exception ex)
        {
            ShowError($"Failed to initialize services: {ex.Message}");
        }
    }
    
    private void AcquireWakeLock()
    {
        try
        {
            if (_wakeLock != null && !_wakeLock.IsHeld)
            {
                _wakeLock.Acquire();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to acquire wake lock: {ex.Message}");
        }
    }
    
    private void ReleaseWakeLock()
    {
        try
        {
            if (_wakeLock != null && _wakeLock.IsHeld)
            {
                _wakeLock.Release();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to release wake lock: {ex.Message}");
        }
    }
    
    private void InitializeViews()
    {
        _reciterSpinner = FindViewById<Spinner>(Resource.Id.reciterSpinner);
        _surahSpinner = FindViewById<Spinner>(Resource.Id.surahSpinner);
        _fromAyaInput = FindViewById<EditText>(Resource.Id.fromAyaInput);
        _toAyaInput = FindViewById<EditText>(Resource.Id.toAyaInput);
        _startButton = FindViewById<Button>(Resource.Id.startButton);
        _stopButton = FindViewById<Button>(Resource.Id.stopButton);
        _repeatCountInput = FindViewById<EditText>(Resource.Id.repeatCountInput);
        _repeatModeSpinner = FindViewById<Spinner>(Resource.Id.repeatModeSpinner);
        _currentAyaInfo = FindViewById<TextView>(Resource.Id.currentAyaInfo);
        _ayaTextView = FindViewById<TextView>(Resource.Id.ayaTextView);
        _ayaImage = FindViewById<ImageView>(Resource.Id.ayaImage);
        _teacherModeCheckbox = FindViewById<CheckBox>(Resource.Id.teacherModeCheckbox);
        
                
        // Labels for dynamic language update
        _titleTextView = FindViewById<TextView>(Resource.Id.titleTextView);
        _reciterLabel = FindViewById<TextView>(Resource.Id.reciterLabel);
        _surahLabel = FindViewById<TextView>(Resource.Id.surahLabel);
        _fromAyaLabel = FindViewById<TextView>(Resource.Id.fromAyaLabel);
        _toAyaLabel = FindViewById<TextView>(Resource.Id.toAyaLabel);
        _nowPlayingLabel = FindViewById<TextView>(Resource.Id.nowPlayingLabel);
        
        // Setup click handlers
        if (_startButton != null)
            _startButton.Click += OnStartClick;
        if (_stopButton != null)
            _stopButton.Click += OnStopClick;
        if (_teacherModeCheckbox != null)
            _teacherModeCheckbox.CheckedChange += (s, e) => _teacherModeEnabled = e.IsChecked;
    }
    
    private void LoadSettings()
    {
        var prefs = GetSharedPreferences("QuranSearchSettings", FileCreationMode.Private);
        
        // Load language setting with system language detection
        var language = LanguageHelper.GetLanguageFromPreferences(this);
        if (_localizationService != null)
        {
            _localizationService.CurrentLanguage = language;
            UpdateUIStrings();
            UpdateLayoutDirection(language);
        }
        
        var useRemoteAudio = prefs?.GetBoolean("UseRemoteAudio", true) ?? true;
        var useRemoteImages = prefs?.GetBoolean("UseRemoteImages", false) ?? false;
        _selectedReciter = prefs?.GetString("SelectedReciter", "Husary_128kbps") ?? "Husary_128kbps";
        
        if (_audioService != null)
        {
            _audioService.UseRemoteSource = useRemoteAudio;
            _audioService.SelectedReciter = _selectedReciter;
        }
        if (_pictureService != null)
        {
            _pictureService.UseRemoteSource = useRemoteImages;
        }
    }
    
    private void UpdateUIStrings()
    {
        if (_localizationService == null) return;
        
        // Update title
        if (_titleTextView != null)
            _titleTextView.Text = _localizationService["RecitationMode"];
        
        // Update labels
        if (_reciterLabel != null)
            _reciterLabel.Text = _localizationService["SelectReciter"];
        if (_surahLabel != null)
            _surahLabel.Text = _localizationService["SelectSurah"];
        if (_fromAyaLabel != null)
            _fromAyaLabel.Text = _localizationService["FromAya"];
        if (_toAyaLabel != null)
            _toAyaLabel.Text = _localizationService["ToAya"];
        if (_nowPlayingLabel != null)
            _nowPlayingLabel.Text = _localizationService["NowPlaying"];
        if (_teacherModeCheckbox != null)
            _teacherModeCheckbox.Text = _localizationService["TeacherMode"];
        
        // Start/Stop buttons are icon-only, no text update needed
    }
    
    private void UpdateLayoutDirection(string language)
    {
        var layoutDirection = language == "ar" ? LayoutDirection.Rtl : LayoutDirection.Ltr;
        
        // Set layout direction on the root view
        if (Window?.DecorView != null)
        {
            Window.DecorView.LayoutDirection = layoutDirection;
        }
    }
    
    private void SetupSpinners()
    {
        // Setup Reciter Spinner (localized names)
        var reciterNames = new List<string>();
        int selectedReciterIndex = 0;
        for (int i = 0; i < _reciters.Count; i++)
        {
            var displayName = _localizationService?[_reciters[i].DisplayKey] ?? _reciters[i].DisplayKey;
            reciterNames.Add(displayName);
            if (_reciters[i].FolderKey == _selectedReciter)
                selectedReciterIndex = i;
        }
        
        var reciterAdapter = new ArrayAdapter<string>(this, 
            global::Android.Resource.Layout.SimpleSpinnerItem, reciterNames);
        reciterAdapter.SetDropDownViewResource(global::Android.Resource.Layout.SimpleSpinnerDropDownItem);
        if (_reciterSpinner != null)
        {
            _reciterSpinner.Adapter = reciterAdapter;
            _reciterSpinner.SetSelection(selectedReciterIndex);
            _reciterSpinner.ItemSelected += OnReciterSelected;
        }
        
        // Setup Surah Spinner
        var surahNames = new List<string>();
        for (int i = 1; i <= SurahInfo.TotalSurahs; i++)
        {
            surahNames.Add($"{i}. {SurahInfo.GetSurahName(i)}");
        }
        
        var surahAdapter = new ArrayAdapter<string>(this,
            global::Android.Resource.Layout.SimpleSpinnerItem, surahNames);
        surahAdapter.SetDropDownViewResource(global::Android.Resource.Layout.SimpleSpinnerDropDownItem);
        if (_surahSpinner != null)
        {
            _surahSpinner.Adapter = surahAdapter;
            _surahSpinner.ItemSelected += OnSurahSelected;
        }
        
        // Initial Aya inputs setup
        SetupAyaInputs();
        
        // Setup Repeat Spinners
        SetupRepeatSpinners();
    }
    
    private void SetupRepeatSpinners()
    {
        // Setup Repeat Count EditText (1-100)
        if (_repeatCountInput != null)
        {
            _repeatCountInput.Text = "1";
            _repeatCountInput.FocusChange += (s, e) =>
            {
                if (!e.HasFocus) ValidateRepeatCount();
            };
        }
        
        // Setup Repeat Mode Spinner
        var repeatModes = new List<string>
        {
            _localizationService?["RepeatWholeRange"] ?? "Repeat Whole Range",
            _localizationService?["RepeatEachAya"] ?? "Repeat Each Aya"
        };
        
        var modeAdapter = new ArrayAdapter<string>(this,
            global::Android.Resource.Layout.SimpleSpinnerItem, repeatModes);
        modeAdapter.SetDropDownViewResource(global::Android.Resource.Layout.SimpleSpinnerDropDownItem);
        
        if (_repeatModeSpinner != null)
        {
            _repeatModeSpinner.Adapter = modeAdapter;
            _repeatModeSpinner.SetSelection(0); // Default to Repeat Whole Range
            _repeatModeSpinner.ItemSelected += OnRepeatModeSelected;
        }
    }
    
    private void ValidateRepeatCount()
    {
        int origVal;
        if (!int.TryParse(_repeatCountInput?.Text, out int val) || val < 1)
        {
            origVal = -1;
            val = 1;
        }
        else
        {
            origVal = val;
        }
        if (val > 99)
        {
            origVal = val;
            val = 99;
        }
        
        _repeatCount = val;
        
        if (_repeatCountInput != null && _repeatCountInput.Text != val.ToString())
            _repeatCountInput.Text = val.ToString();
        
        if (origVal != val)
        {
            var message = $"{_localizationService?["RepeatRangeError"] ?? "Valid repeat range"}: 1 - 99";
            Toast.MakeText(this, message, ToastLength.Short)?.Show();
        }
    }
    
    private void OnRepeatModeSelected(object? sender, AdapterView.ItemSelectedEventArgs e)
    {
        _repeatEachAya = (e.Position == 1); // 0 = Whole Range, 1 = Each Aya
    }
    
    private void SetupAyaInputs()
    {
        int ayaCount = SurahInfo.GetAyaCount(_selectedSurah);
        _fromAya = 1;
        _toAya = ayaCount;
        
        if (_fromAyaInput != null)
        {
            _fromAyaInput.Text = "1";
            _fromAyaInput.AfterTextChanged -= OnFromAyaTextChanged;
            _fromAyaInput.AfterTextChanged += OnFromAyaTextChanged;
            _fromAyaInput.FocusChange -= OnFromAyaFocusChange;
            _fromAyaInput.FocusChange += OnFromAyaFocusChange;
        }
        
        if (_toAyaInput != null)
        {
            _toAyaInput.Text = ayaCount.ToString();
            _toAyaInput.AfterTextChanged -= OnToAyaTextChanged;
            _toAyaInput.AfterTextChanged += OnToAyaTextChanged;
            _toAyaInput.FocusChange -= OnToAyaFocusChange;
            _toAyaInput.FocusChange += OnToAyaFocusChange;
        }
    }
    
    private void UpdateAyaInputsForSurah()
    {
        int ayaCount = SurahInfo.GetAyaCount(_selectedSurah);
        _fromAya = 1;
        _toAya = ayaCount;
        
        if (_fromAyaInput != null)
            _fromAyaInput.Text = "1";
        if (_toAyaInput != null)
            _toAyaInput.Text = ayaCount.ToString();
    }
    
    private void OnFromAyaTextChanged(object? sender, AfterTextChangedEventArgs e)
    {
        // Validation happens on focus change to avoid interfering while typing
    }
    
    private void OnToAyaTextChanged(object? sender, AfterTextChangedEventArgs e)
    {
        // Validation happens on focus change to avoid interfering while typing
    }
    
    private void OnFromAyaFocusChange(object? sender, View.FocusChangeEventArgs e)
    {
        if (!e.HasFocus) ValidateAyaInputs();
    }
    
    private void OnToAyaFocusChange(object? sender, View.FocusChangeEventArgs e)
    {
        if (!e.HasFocus) ValidateAyaInputs();
    }
    
    private void ValidateAyaInputs()
    {
        int ayaCount = SurahInfo.GetAyaCount(_selectedSurah);
        bool showNotification = false;
        string message = "";
        
        // Parse and clamp fromAya
        int origFrom;
        if (!int.TryParse(_fromAyaInput?.Text, out int fromVal) || fromVal < 1)
        {
            origFrom = -1;
            fromVal = 1;
        }
        else
        {
            origFrom = fromVal;
        }
        if (fromVal > ayaCount)
            fromVal = ayaCount;
        
        // Parse and clamp toAya
        int origTo;
        if (!int.TryParse(_toAyaInput?.Text, out int toVal) || toVal < 1)
        {
            origTo = -1;
            toVal = ayaCount;
        }
        else
        {
            origTo = toVal;
        }
        if (toVal > ayaCount)
            toVal = ayaCount;
        
        // Check if values were corrected
        if (origFrom != fromVal || origTo != toVal)
        {
            showNotification = true;
            message = $"{_localizationService?["AyaRangeError"] ?? "Valid Aya range"}: 1 - {ayaCount}";
        }
        
        // Ensure fromAya <= toAya
        if (fromVal > toVal)
        {
            toVal = fromVal;
            showNotification = true;
            message = $"{_localizationService?["FromAyaLessOrEqual"] ?? "From Aya must be ≤ To Aya"}";
        }
        
        _fromAya = fromVal;
        _toAya = toVal;
        
        // Update UI without re-triggering events
        if (_fromAyaInput != null && _fromAyaInput.Text != fromVal.ToString())
            _fromAyaInput.Text = fromVal.ToString();
        if (_toAyaInput != null && _toAyaInput.Text != toVal.ToString())
            _toAyaInput.Text = toVal.ToString();
        
        if (showNotification)
            Toast.MakeText(this, message, ToastLength.Short)?.Show();
    }
    
    private async void LoadQuranData()
    {
        try
        {
            if (_searchService != null)
            {
                await _searchService.LoadQuranTextAsync();
            }
            
            // Display 1st aya of the default selected surah (must run on UI thread)
            RunOnUiThread(() => DisplayDefaultAya());
        }
        catch (Exception ex)
        {
            RunOnUiThread(() => ShowError($"Failed to load Quran data: {ex.Message}"));
        }
    }
    
    private void DisplayDefaultAya()
    {
        var ayaText = GetAyaText(_selectedSurah, 1);
        if (_ayaTextView != null)
            _ayaTextView.Text = ayaText;
        if (_currentAyaInfo != null)
            _currentAyaInfo.Text = $"{SurahInfo.GetSurahName(_selectedSurah)} - 1";
        ShowAyaImage(_selectedSurah, 1);
    }
    
    private void OnReciterSelected(object? sender, AdapterView.ItemSelectedEventArgs e)
    {
        if (e.Position >= 0 && e.Position < _reciters.Count)
        {
            _selectedReciter = _reciters[e.Position].FolderKey;
            if (_audioService != null)
            {
                _audioService.SelectedReciter = _selectedReciter;
            }
            
            // Save to preferences
            var prefs = GetSharedPreferences("QuranSearchSettings", FileCreationMode.Private);
            var editor = prefs?.Edit();
            editor?.PutString("SelectedReciter", _selectedReciter);
            editor?.Apply();
        }
    }
    
    private void OnSurahSelected(object? sender, AdapterView.ItemSelectedEventArgs e)
    {
        _selectedSurah = e.Position + 1; // 1-based
        UpdateAyaInputsForSurah();
        DisplayDefaultAya();
    }
    
    private void OnStartClick(object? sender, EventArgs e)
    {
        StartRecitation();
    }
    
    private void OnStopClick(object? sender, EventArgs e)
    {
        StopRecitation();
    }
    
    private void StartRecitation()
    {
        // Validate inputs before starting
        ValidateAyaInputs();
        ValidateRepeatCount();
        
        _currentSurah = _selectedSurah;
        _currentAya = _fromAya;
        _currentRepeat = 1;
        _currentAyaRepeat = 1;
        _currentAyaStartTime = DateTime.UtcNow;
        _isPlaying = true;
        
        // Keep screen on during recitation
        AcquireWakeLock();
        
        // Check if we need Basmalah for the first Aya
        _needsBasmalah = ShouldPlayBasmalah(_currentSurah, _currentAya, true);
        
        UpdateButtonStates();
        PlayCurrentAya();
    }
    
    private void StopRecitation()
    {
        _isPlaying = false;
        _playingBasmalah = false;
        _currentRepeat = 1;
        _currentAyaRepeat = 1;
        _audioService?.StopAsync();
        ReleaseWakeLock();
        UpdateButtonStates();
        
        if (_currentAyaInfo != null)
            _currentAyaInfo.Text = _localizationService?["Stopped"] ?? GetString(Resource.String.stopped);
    }
    
    private bool ShouldPlayBasmalah(int surah, int aya, bool isStartOfRecitation)
    {
        // Play Basmalah before the first Aya of each Surah, except:
        // - Surah Al-Fatiha (1): Basmalah is already part of the Surah's audio
        // - Surah At-Tawbah (9): No Basmalah for this Surah
        // - Menshawi reciter: His recordings already include Basmalah
        if (aya != 1)
            return false;
        
        if (surah == SURAH_FATIHA || surah == SURAH_TAWBAH)
            return false;
        
        // Skip Basmalah for reciters whose recordings already include it
        // Menshawi and Mustafa Ismail always include Basmalah
        if (_selectedReciter == "Menshawi_32kbps" ||
            _selectedReciter == "Mustafa_Ismail_48kbps")
            return false;
        
        // Mahmoud Ali Al-Banna includes Basmalah except for Surah 108 (Al-Kawthar)
        if (_selectedReciter == "mahmoud_ali_al_banna_32kbps")
            return surah == 108; // Only play Basmalah for Surah 108
        
        return true;
    }
    
    private async void PlayCurrentAya()
    {
        if (_audioService == null) return;
        
        try
        {
            // If we need to play Basmalah first
            if (_needsBasmalah && !_playingBasmalah)
            {
                _playingBasmalah = true;
                UpdateCurrentAyaDisplay(true);
                
                // Play Basmalah (Surah 1, Aya 1)
                await _audioService.PlayAyaAsync(1, 1);
                return;
            }
            
            _playingBasmalah = false;
            _needsBasmalah = false;
            
            UpdateCurrentAyaDisplay(false);
            _currentAyaStartTime = DateTime.UtcNow;
            await _audioService.PlayAyaAsync(_currentSurah, _currentAya);
            
            // Show the Aya image
            ShowAyaImage(_currentSurah, _currentAya);
        }
        catch (Exception ex)
        {
            ShowError($"Playback error: {ex.Message}");
        }
    }
    
    private void UpdateCurrentAyaDisplay(bool isBasmalah)
    {
        RunOnUiThread(() =>
        {
            if (isBasmalah)
            {
                if (_currentAyaInfo != null)
                    _currentAyaInfo.Text = _localizationService?["PlayingBasmala"] ?? GetString(Resource.String.playing_basmalah);
                if (_ayaTextView != null)
                    _ayaTextView.Text = _localizationService?["Basmalah"] ?? GetString(Resource.String.basmalah);
            }
            else
            {
                string surahName = SurahInfo.GetSurahName(_currentSurah);
                if (_currentAyaInfo != null)
                {
                    var surahLabel = _localizationService?["Surah"] ?? "Surah";
                    var ayaLabel = _localizationService?["Aya"] ?? "Aya";
                    
                    // Add repeat info if repeating more than once
                    string repeatInfo = "";
                    if (_repeatCount > 1)
                    {
                        if (_repeatEachAya)
                        {
                            // Show aya repeat info: "Aya 5 - Repeat 2 of 3"
                            var ayaRepeatStr = _localizationService?["AyaRepeat"] ?? "Aya {0} - Repeat {1} of {2}";
                            repeatInfo = $" - {string.Format(ayaRepeatStr, _currentAya, _currentAyaRepeat, _repeatCount)}";
                            _currentAyaInfo.Text = $"{surahLabel} {surahName}{repeatInfo}";
                        }
                        else
                        {
                            // Show range repeat info: "Repeat 2 of 3"
                            var currentRepeatStr = _localizationService?["CurrentRepeat"] ?? "Repeat {0} of {1}";
                            repeatInfo = $" - {string.Format(currentRepeatStr, _currentRepeat, _repeatCount)}";
                            _currentAyaInfo.Text = $"{surahLabel} {surahName} - {ayaLabel} {_currentAya}{repeatInfo}";
                        }
                    }
                    else
                    {
                        _currentAyaInfo.Text = $"{surahLabel} {surahName} - {ayaLabel} {_currentAya}";
                    }
                }
                
                // Get and display the Aya text
                var ayaText = GetAyaText(_currentSurah, _currentAya);
                if (_ayaTextView != null)
                    _ayaTextView.Text = ayaText;
            }
        });
    }
    
    private string GetAyaText(int surah, int aya)
    {
        // Try to get from loaded Quran data
        // For now, return a placeholder - the actual text would come from the search service
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
    
    private async void ShowAyaImage(int surah, int aya)
    {
        try
        {
            if (_pictureService == null || _ayaImage == null)
                return;
            
            var ok = await _pictureService.EnsurePictureAvailableAsync(surah, aya);
            var path = _pictureService.GetPicturePath(surah, aya);
            var bmp = await _pictureService.LoadBitmapAsync(path) as Bitmap;
            
            RunOnUiThread(() =>
            {
                if (bmp != null)
                {
                    _ayaImage.SetImageBitmap(bmp);
                    _ayaImage.Visibility = ViewStates.Visible;
                }
                else
                {
                    _ayaImage.Visibility = ViewStates.Gone;
                }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Image load failed: {ex.Message}");
        }
    }
    
    private void OnPlaybackStateChanged(object? sender, bool isPlaying)
    {
        RunOnUiThread(() =>
        {
            UpdateButtonStates();
        });
    }
    
    private void OnAyaPlaybackEnded(object? sender, EventArgs e)
    {
        HandleAyaPlaybackEnded();
    }

    private async void HandleAyaPlaybackEnded()
    {
        if (!_isPlaying) return;

        // If we just finished playing Basmalah, now play the actual Aya
        if (_playingBasmalah)
        {
            RunOnUiThread(() =>
            {
                if (!_isPlaying) return;
                _playingBasmalah = false;
                _needsBasmalah = false; // Clear the flag so we don't play Basmalah again
                PlayCurrentAya();
            });
            return;
        }

        if (_teacherModeEnabled)
        {
            var ayaDuration = DateTime.UtcNow - _currentAyaStartTime;
            if (ayaDuration < TimeSpan.FromSeconds(1))
            {
                ayaDuration = TimeSpan.FromSeconds(1);
            }
            var pauseDuration = ayaDuration + TimeSpan.FromSeconds(1);
            try
            {
                await Task.Delay(pauseDuration);
            }
            catch
            {
                // Ignore delay cancellation
            }
            if (!_isPlaying)
                return;
        }

        RunOnUiThread(() =>
        {
            if (!_isPlaying) return;
            MoveToNextAya();

            if (_isPlaying)
            {
                PlayCurrentAya();
            }
        });
    }
    
    private void MoveToNextAya()
    {
        if (_repeatEachAya)
        {
            // Repeat Each Aya mode
            if (_currentAyaRepeat < _repeatCount)
            {
                // Repeat current aya
                _currentAyaRepeat++;
            }
            else
            {
                // Move to next aya
                _currentAyaRepeat = 1;
                if (_currentAya < _toAya)
                {
                    _currentAya++;
                    _needsBasmalah = ShouldPlayBasmalah(_currentSurah, _currentAya, false);
                }
                else
                {
                    // All ayas complete
                    _isPlaying = false;
                    ReleaseWakeLock();
                    UpdateButtonStates();
                    
                    var completeText = _localizationService?["RecitationComplete"] ?? GetString(Resource.String.recitation_complete);
                    if (_currentAyaInfo != null)
                        _currentAyaInfo.Text = completeText;
                    
                    Toast.MakeText(this, completeText, ToastLength.Short)?.Show();
                }
            }
        }
        else
        {
            // Repeat Whole Range mode
            int maxAyaInCurrentSurah = (_currentSurah == _selectedSurah) ? _toAya : SurahInfo.GetAyaCount(_currentSurah);
            
            if (_currentAya < maxAyaInCurrentSurah)
            {
                _currentAya++;
                _needsBasmalah = ShouldPlayBasmalah(_currentSurah, _currentAya, false);
            }
            else
            {
                // Check if we should continue to next Surah or repeat
                if (_currentSurah == _selectedSurah && _currentAya >= _toAya)
                {
                    // Finished one repeat cycle
                    if (_currentRepeat < _repeatCount)
                    {
                        // Start next repeat cycle
                        _currentRepeat++;
                        _currentAya = _fromAya;
                        _needsBasmalah = ShouldPlayBasmalah(_currentSurah, _currentAya, true);
                    }
                    else
                    {
                        // All repeats complete
                        _isPlaying = false;
                        ReleaseWakeLock();
                        UpdateButtonStates();
                        
                        var completeText = _localizationService?["RecitationComplete"] ?? GetString(Resource.String.recitation_complete);
                        if (_currentAyaInfo != null)
                            _currentAyaInfo.Text = completeText;
                        
                        Toast.MakeText(this, completeText, ToastLength.Short)?.Show();
                    }
                    return;
                }
                
                // Move to next Surah
                _currentSurah++;
                _currentAya = 1;
                _needsBasmalah = ShouldPlayBasmalah(_currentSurah, _currentAya, false);
            }
        }
    }
    
    private void UpdateButtonStates()
    {
        if (_startButton != null)
            _startButton.Enabled = !_isPlaying;
        if (_stopButton != null)
            _stopButton.Enabled = _isPlaying;
    }
    
    private void OnSettingsClick(object? sender, EventArgs e)
    {
        var intent = new Intent(this, typeof(SettingsActivity));
        StartActivityForResult(intent, SETTINGS_REQUEST_CODE);
    }
    
    protected override void OnActivityResult(int requestCode, Result resultCode, Intent? data)
    {
        base.OnActivityResult(requestCode, resultCode, data);
        
        if (requestCode == SETTINGS_REQUEST_CODE && resultCode == Result.Ok)
        {
            LoadSettings();
            
            // Update reciter spinner selection
            for (int i = 0; i < _reciters.Count; i++)
            {
                if (_reciters[i].FolderKey == _selectedReciter)
                {
                    _reciterSpinner?.SetSelection(i);
                    break;
                }
            }
            
            Toast.MakeText(this, "Settings applied", ToastLength.Short)?.Show();
        }
    }
    
    private void ShowError(string message)
    {
        RunOnUiThread(() =>
        {
            Toast.MakeText(this, message, ToastLength.Long)?.Show();
        });
    }
    
    protected override void OnDestroy()
    {
        ReleaseWakeLock();
        _audioService?.Dispose();
        base.OnDestroy();
    }
}
