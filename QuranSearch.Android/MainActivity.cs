using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using Android.Graphics;
using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.RecyclerView.Widget;
using QuranSearch.Android.Adapters;
using QuranSearch.Android.Services;
using QuranSearch.Core.Services;
using QuranSearch.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuranSearch.Android;

[Activity(Label = "@string/app_name", MainLauncher = true, Theme = "@style/SplashTheme")]
public class MainActivity : AppCompatActivity
{
    private QuranSearchService? _searchService;
    private AndroidAudioService? _audioService;
    private AndroidPictureService? _pictureService;
    private LocalizationService? _localizationService;
    
    private LinearLayout? _languageContainer;
    private LinearLayout? _ruleContainer;
    private TextView? _languageIcon;
    private TextView? _ruleIcon;
    private Spinner? _languageSpinner;
    private Spinner? _ruleSpinner;
    private string? _selectedRuleName;
    private readonly Dictionary<string, string> _ruleDisplayToArabic = new();
    private readonly HashSet<string> _groupHeaders = new();
    private Button? _settingsButton;
    private Button? _searchButton;
    private Button? _playButton;
    private Button? _playRepeatButton;
    private Button? _playAllButton;
    private Button? _stopButton;
    private CheckBox? _useRemoteAudioCheckBox;
    private TextView? _statusText;
    private ImageView? _ayaImage;
    private RecyclerView? _recyclerView;
    private AyaAdapter? _adapter;
    
    private List<QuranAya> _searchResults = new();
    private int _currentPlayingIndex = -1;
    private bool _isPlayingSequence = false;
    private SearchDomainType _searchDomainType = SearchDomainType.WholeQuran;
    private int _searchStartSurah = 1;
    private int _searchEndSurah = 114;
    
    private const int SETTINGS_REQUEST_CODE = 1001;
    
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        // Switch from splash theme to app theme
        SetTheme(Resource.Style.AppTheme);
        
        base.OnCreate(savedInstanceState);
        SetContentView(Resource.Layout.activity_main);
        
        InitializeServices();
        InitializeViews();
        SetupRecyclerView();
        LoadSettings();
        LoadDataAsync();
    }
    
    private void InitializeServices()
    {
        try
        {
            var fileService = new AndroidFileService(this);
            _searchService = new QuranSearchService(fileService);
            _audioService = new AndroidAudioService(this);
            _pictureService = new AndroidPictureService(this);
            _localizationService = new LocalizationService(fileService);
            
            // Setup audio event handlers
            if (_audioService != null)
            {
                _audioService.PlaybackStateChanged += OnPlaybackStateChanged;
                _audioService.SequencePlaybackEnded += OnSequencePlaybackEnded;
                _audioService.PlaybackError += (s, msg) => ShowError(msg);
            }
        }
        catch (Exception ex)
        {
            ShowError($"Failed to initialize services: {ex.Message}");
        }
    }
    
    private void InitializeViews()
    {
        _languageContainer = FindViewById<LinearLayout>(Resource.Id.languageContainer);
        _ruleContainer = FindViewById<LinearLayout>(Resource.Id.ruleContainer);
        _languageIcon = FindViewById<TextView>(Resource.Id.languageIcon);
        _ruleIcon = FindViewById<TextView>(Resource.Id.ruleIcon);
        _languageSpinner = FindViewById<Spinner>(Resource.Id.languageSpinner);
        _ruleSpinner = FindViewById<Spinner>(Resource.Id.ruleSpinner);
        _settingsButton = FindViewById<Button>(Resource.Id.settingsButton);
        _searchButton = FindViewById<Button>(Resource.Id.searchButton);
        _playButton = FindViewById<Button>(Resource.Id.playButton);
        _playRepeatButton = FindViewById<Button>(Resource.Id.playRepeatButton);
        _playAllButton = FindViewById<Button>(Resource.Id.playAllButton);
        _stopButton = FindViewById<Button>(Resource.Id.stopButton);
        _useRemoteAudioCheckBox = FindViewById<CheckBox>(Resource.Id.useRemoteAudioCheckBox);
        _statusText = FindViewById<TextView>(Resource.Id.statusText);
        _ayaImage = FindViewById<ImageView>(Resource.Id.ayaImage);
        _recyclerView = FindViewById<RecyclerView>(Resource.Id.recyclerView);
        
        // Setup button click handlers
        if (_settingsButton != null)
            _settingsButton.Click += OnSettingsClick;
        if (_searchButton != null)
            _searchButton.Click += OnSearchClick;
        if (_playButton != null)
            _playButton.Click += OnPlayClick;
        if (_playRepeatButton != null)
            _playRepeatButton.Click += OnPlayRepeatClick;
        if (_playAllButton != null)
            _playAllButton.Click += OnPlayAllClick;
        if (_stopButton != null)
            _stopButton.Click += OnStopClick;
        if (_useRemoteAudioCheckBox != null)
            _useRemoteAudioCheckBox.CheckedChange += OnUseRemoteAudioChanged;
        
        // Hide currently displayed Aya when user picks a new rule (actual rule, not a group header)
        if (_ruleSpinner != null)
            _ruleSpinner.ItemSelected += OnRuleSpinnerItemSelected;
        
        // Setup language spinner
        if (_languageSpinner != null)
        {
            var languages = new[] { "العربية", "English", "Deutsch" };
            var languageAdapter = new ArrayAdapter<string>(this, 
                global::Android.Resource.Layout.SimpleSpinnerItem, languages);
            languageAdapter.SetDropDownViewResource(global::Android.Resource.Layout.SimpleSpinnerDropDownItem);
            _languageSpinner.Adapter = languageAdapter;
            _languageSpinner.ItemSelected += OnLanguageSelected;
            // Set default to English (index 1)
            _languageSpinner.SetSelection(1);
        }
    }
    
    private void SetupRecyclerView()
    {
        if (_recyclerView != null)
        {
            _recyclerView.SetLayoutManager(new LinearLayoutManager(this));
            _adapter = new AyaAdapter();
            _adapter.ItemClick += OnAyaItemClick;
            _recyclerView.SetAdapter(_adapter);
            // Initialize adapter localization if services are ready
            UpdateAdapterLocalization();
        }
    }
    
    private async void LoadDataAsync()
    {
        try
        {
            UpdateStatus(GetString(Resource.String.loading));
            
            if (_searchService != null)
            {
                await _searchService.LoadQuranTextAsync();
                await _searchService.LoadRulesAsync();
                
                // Populate rules spinner
                UpdateRuleDisplayNames();
                
                UpdateStatus(GetString(Resource.String.ready));
            }
        }
        catch (Exception ex)
        {
            ShowError($"{GetString(Resource.String.error_loading)}: {ex.Message}");
        }
    }
    
    private async void OnSearchClick(object? sender, EventArgs e)
    {
        try
        {
            if (_searchService == null || _ruleSpinner == null) return;
            
            // Try spinner first, then fall back to button selection
            var selectedDisplayName = _ruleSpinner.SelectedItem?.ToString();
            string arabicRuleName;
            
            if (!string.IsNullOrEmpty(selectedDisplayName) && _ruleDisplayToArabic.TryGetValue(selectedDisplayName, out var mappedName))
            {
                // Check if it's a group header
                if (string.IsNullOrEmpty(mappedName))
                {
                    ShowError(GetString(Resource.String.SelectSpecificRule));
                    return;
                }
                arabicRuleName = mappedName;
            }
            else if (!string.IsNullOrEmpty(_selectedRuleName))
            {
                arabicRuleName = _selectedRuleName;
            }
            else
            {
                ShowError(GetString(Resource.String.SelectRuleFirst));
                return;
            }
            
            // Reset UI from any previously displayed Aya before running a new search
            HideAyaImage();
            _currentPlayingIndex = -1;
            _isPlayingSequence = false;
            _audioService?.StopPlayback();
            _adapter?.UpdateData(new List<QuranAya>());
            UpdateAudioButtonsState(false);

            UpdateStatus(GetString(Resource.String.searching));
            
            // Pass domain parameters to search service for efficient filtering BEFORE searching
            if (_searchDomainType == SearchDomainType.SingleSurah)
            {
                _searchResults = _searchService.SearchByRuleName(arabicRuleName, _searchStartSurah, _searchStartSurah);
            }
            else if (_searchDomainType == SearchDomainType.SurahRange)
            {
                _searchResults = _searchService.SearchByRuleName(arabicRuleName, _searchStartSurah, _searchEndSurah);
            }
            else
            {
                _searchResults = _searchService.SearchByRuleName(arabicRuleName);
            }
            
            // Reset highlighted/selected index for new results
            _currentPlayingIndex = -1;
            _adapter?.UpdateData(_searchResults);
            
            var matchCount = _searchResults.Count;
            // Use Android formatting (resource uses %d)
            UpdateStatus(GetString(Resource.String.found_matches, matchCount));
            
            // Enable audio buttons if there are results (none selected yet)
            UpdateAudioButtonsState(matchCount > 0);
        }
        catch (Exception ex)
        {
            ShowError($"Search failed: {ex.Message}");
        }
    }
    
    private void HideAyaImage()
    {
        try
        {
            if (_ayaImage == null) return;
            RunOnUiThread(() =>
            {
                _ayaImage.SetImageDrawable(null);
                _ayaImage.Visibility = ViewStates.Gone;
            });
        }
        catch { /* no-op */ }
    }

    private void OnRuleSpinnerItemSelected(object? sender, AdapterView.ItemSelectedEventArgs e)
    {
        try
        {
            if (_ruleSpinner == null) return;
            var selected = _ruleSpinner.GetItemAtPosition(e.Position)?.ToString();
            if (string.IsNullOrEmpty(selected)) return;
            
            // Only react for real rules (mapping exists and not empty). Group headers map to empty string.
            if (_ruleDisplayToArabic.TryGetValue(selected, out var mapped) && !string.IsNullOrEmpty(mapped))
            {
                // Hide any currently shown Aya and stop playback
                HideAyaImage();
                _audioService?.StopPlayback();
                _currentPlayingIndex = -1;
                UpdateAudioButtonsState(_searchResults.Count > 0);
            }
        }
        catch { /* no-op */ }
    }
    
    private void OnAyaItemClick(object? sender, int position)
    {
        try
        {
            UpdateStatus($"Clicked on item at position: {position}");
            _currentPlayingIndex = position;
            if (position >= 0 && position < _searchResults.Count)
            {
                ShowAyaImage(_searchResults[position]);
            }
            PlayCurrentAya(false);
        }
        catch (Exception ex)
        {
            UpdateStatus($"Click error: {ex.Message}");
        }
    }
    
    private void OnPlayClick(object? sender, EventArgs e)
    {
        if (_currentPlayingIndex >= 0 && _currentPlayingIndex < _searchResults.Count)
        {
            PlayCurrentAya(false);
        }
        else if (_searchResults.Count > 0)
        {
            _currentPlayingIndex = 0;
            PlayCurrentAya(false);
        }
    }
    
    private void OnPlayRepeatClick(object? sender, EventArgs e)
    {
        if (_currentPlayingIndex >= 0 && _currentPlayingIndex < _searchResults.Count)
        {
            PlayCurrentAya(true);
        }
        else if (_searchResults.Count > 0)
        {
            _currentPlayingIndex = 0;
            PlayCurrentAya(true);
        }
    }
    
    private void OnPlayAllClick(object? sender, EventArgs e)
    {
        if (_searchResults.Count > 0)
        {
            _isPlayingSequence = true;
            _currentPlayingIndex = 0;
            PlayCurrentAya(false);
        }
    }
    
    private void OnStopClick(object? sender, EventArgs e)
    {
        _audioService?.StopPlayback();
        _isPlayingSequence = false;
        UpdateStatus(GetString(Resource.String.stopped));
    }
    
    private void OnUseRemoteAudioChanged(object? sender, CompoundButton.CheckedChangeEventArgs e)
    {
        if (_audioService != null)
        {
            _audioService.UseRemoteSource = e.IsChecked;
        }
    }
    
    private void OnLanguageSelected(object? sender, AdapterView.ItemSelectedEventArgs e)
    {
        var languages = new[] { "ar", "en", "de" };
        if (e.Position >= 0 && e.Position < languages.Length && _localizationService != null)
        {
            _localizationService.CurrentLanguage = languages[e.Position];
            UpdateLayoutDirection(languages[e.Position]);
            UpdateUIStringsForCurrentLanguage();
            UpdateAdapterLocalization();
            UpdateRuleDisplayNames(); // Refresh rule spinner with translated group titles
        }
    }
    
    private void UpdateLayoutDirection(string language)
    {
        // Set layout direction based on language
        var layoutDirection = language == "ar" ? LayoutDirection.Rtl : LayoutDirection.Ltr;
        
        // Update main containers
        if (_languageContainer != null)
        {
            _languageContainer.LayoutDirection = layoutDirection;
        }
        if (_ruleContainer != null)
        {
            _ruleContainer.LayoutDirection = layoutDirection;
        }
        
        // Update the RecyclerView for RTL support
        if (_recyclerView != null)
        {
            _recyclerView.LayoutDirection = layoutDirection;
        }
    }
    
    private void UpdateUIStringsForCurrentLanguage()
    {
        if (_localizationService == null) return;
        
        try
        {
            // Update button texts using LocalizationService
            if (_searchButton != null)
                _searchButton.Text = _localizationService.GetString("Search");
            if (_playButton != null)
                _playButton.Text = _localizationService.GetString("Play");
            if (_playRepeatButton != null)
                _playRepeatButton.Text = _localizationService.GetString("PlayRepeat");
            if (_playAllButton != null)
                _playAllButton.Text = _localizationService.GetString("PlayAll");
            if (_stopButton != null)
                _stopButton.Text = _localizationService.GetString("Stop");
            if (_useRemoteAudioCheckBox != null)
                _useRemoteAudioCheckBox.Text = _localizationService.GetString("UseRemoteAudio");
            
            // Update status
            UpdateStatus(_localizationService.GetString("Ready"));
            
            // Reload rules spinner with localized names
            UpdateRuleDisplayNames();

            // Ensure adapter reflects current language and direction
            UpdateAdapterLocalization();
        }
        catch (Exception ex)
        {
            ShowError($"Error updating UI strings: {ex.Message}");
        }
    }

    private void UpdateAdapterLocalization()
    {
        try
        {
            if (_adapter == null || _localizationService == null)
                return;
            // Determine RTL by current language code
            var isRtl = _localizationService.CurrentLanguage == "ar";
            _adapter.SetLocalization(_localizationService, isRtl);
        }
        catch { /* no-op */ }
    }

    private void UpdateRuleDisplayNames()
    {
        if (_searchService == null || _ruleSpinner == null || _localizationService == null)
            return;

        var rules = _searchService.GetRules();
        var displayNames = new List<string>();
        _ruleDisplayToArabic.Clear();
        var languageCode = _localizationService.CurrentLanguage;

        // Group rules by category
        var groupMap = new Dictionary<string, List<TajweedRule>>();
        foreach (var rule in rules)
        {
            if (rule == null) continue;

            var groupTitle = string.Empty;
            if (!string.IsNullOrWhiteSpace(rule.Group))
            {
                groupTitle = RuleGroupTranslator.GetGroupTitle(rule.Group, languageCode);
            }
            
            if (string.IsNullOrWhiteSpace(groupTitle))
            {
                groupTitle = languageCode == "ar" ? "قواعد أخرى" : 
                             languageCode == "de" ? "Andere Regeln" : "Other Rules";
            }

            if (!groupMap.ContainsKey(groupTitle))
            {
                groupMap[groupTitle] = new List<TajweedRule>();
            }
            
            groupMap[groupTitle].Add(rule);
        }

        // Build hierarchical display list
        _groupHeaders.Clear();
        foreach (var kvp in groupMap)
        {
            // Add group header (non-selectable, will be displayed in bold)
            var groupHeader = kvp.Key; // Just the group name, no decorators
            displayNames.Add(groupHeader);
            _ruleDisplayToArabic[groupHeader] = ""; // Empty mapping for headers
            _groupHeaders.Add(groupHeader); // Track as group header

            // Add rules in this group with indentation
            foreach (var rule in kvp.Value)
            {
                var localizedName = RuleNameTranslator.GetLocalizedName(rule.Name, languageCode);
                var indentedName = $"    {localizedName}";
                displayNames.Add(indentedName);
                _ruleDisplayToArabic[indentedName] = rule.Name;
            }
        }

        var ruleAdapter = new RuleSpinnerAdapter(this,
            Resource.Layout.item_rule, displayNames, _groupHeaders);
        _ruleSpinner.Adapter = ruleAdapter;
    }

    private void ShowRuleSelectionDialog()
    {
        try
        {
            if (_searchService == null || _localizationService == null)
            {
                ShowError("Services not initialized");
                return;
            }

            var rules = _searchService.GetRules();
            if (rules == null || rules.Count == 0)
            {
                ShowError("No rules available");
                return;
            }

            var languageCode = _localizationService.CurrentLanguage;

            // Build grouped display list with separators
            var displayItems = new List<string>();
            var ruleMapping = new List<TajweedRule?>(); // null for group headers
            var groupMap = new Dictionary<string, List<TajweedRule>>();

            // Group rules
            foreach (var rule in rules)
            {
                if (rule == null) continue;

                var groupTitle = string.Empty;
                
                if (!string.IsNullOrWhiteSpace(rule.Group))
                {
                    groupTitle = RuleGroupTranslator.GetGroupTitle(rule.Group, languageCode);
                }
                
                if (string.IsNullOrWhiteSpace(groupTitle))
                {
                    groupTitle = languageCode == "ar" ? "قواعد أخرى" : 
                                 languageCode == "de" ? "Andere Regeln" : "Other Rules";
                }

                if (!groupMap.ContainsKey(groupTitle))
                {
                    groupMap[groupTitle] = new List<TajweedRule>();
                }
                
                groupMap[groupTitle].Add(rule);
            }

            // Build flat list with group headers
            foreach (var kvp in groupMap)
            {
                // Add group header
                displayItems.Add($"──── {kvp.Key} ────");
                ruleMapping.Add(null);

                // Add rules in this group
                foreach (var rule in kvp.Value)
                {
                    var localizedName = RuleNameTranslator.GetLocalizedName(rule.Name, languageCode);
                    displayItems.Add($"    {localizedName}");
                    ruleMapping.Add(rule);
                }
            }

            if (displayItems.Count == 0)
            {
                ShowError("No rules available");
                return;
            }

            // Create simple list dialog
            var builder = new global::Android.App.AlertDialog.Builder(this);
            var title = _localizationService.GetString("SelectTajweedRule");
            builder.SetTitle(title);
            
            builder.SetItems(displayItems.ToArray(), (sender, args) =>
            {
                try
                {
                    var selectedIndex = args.Which;
                    if (selectedIndex >= 0 && selectedIndex < ruleMapping.Count)
                    {
                        var selectedRule = ruleMapping[selectedIndex];
                        if (selectedRule != null) // Not a group header
                        {
                            _selectedRuleName = selectedRule.Name;
                            var localizedName = RuleNameTranslator.GetLocalizedName(selectedRule.Name, languageCode);
                            
                            // Sync with spinner if available (match indented format)
                            if (_ruleSpinner?.Adapter is ArrayAdapter<string> spinnerAdapter)
                            {
                                var spinnerDisplayName = $"    {localizedName}";
                                var pos = spinnerAdapter.GetPosition(spinnerDisplayName);
                                if (pos >= 0)
                                {
                                    _ruleSpinner.SetSelection(pos);
                                }
                            }
                            
                            UpdateStatus($"Selected: {localizedName}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    ShowError($"Error selecting rule: {ex.Message}");
                }
            });
            
            builder.SetNegativeButton("Cancel", (sender, args) => { });
            builder.Show();
        }
        catch (Exception ex)
        {
            ShowError($"Error showing rule selection: {ex.Message}\nStack: {ex.StackTrace}");
        }
    }
    
    private async Task PlayCurrentAya(bool repeat)
    {
        try
        {
            UpdateStatus($"PlayCurrentAya called: index={_currentPlayingIndex}, repeat={repeat}, results.Count={_searchResults.Count}");
            
            if (_audioService == null || _currentPlayingIndex < 0 || _currentPlayingIndex >= _searchResults.Count)
            {
                UpdateStatus($"Cannot play: audioService={_audioService != null}, index={_currentPlayingIndex}, count={_searchResults.Count}");
                return;
            }
            
            var aya = _searchResults[_currentPlayingIndex];
            _audioService.SetRepeatMode(repeat);
            
            UpdateStatus($"Playing: {aya.SurahNumber}:{aya.AyaNumber}");
            await _audioService.PlayAyaAsync(aya.SurahNumber, aya.AyaNumber);
            UpdateStatus($"{GetString(Resource.String.playing)} {aya.SurahNumber}:{aya.AyaNumber}");
        }
        catch (Exception ex)
        {
            ShowError($"Playback failed: {ex.Message}");
        }
    }
    
    private void OnPlaybackStateChanged(object? sender, bool isPlaying)
    {
        RunOnUiThread(() =>
        {
            // Only update UI state - sequence playback is handled by OnSequencePlaybackEnded
            UpdateAudioButtonsState(_searchResults.Count > 0);
        });
    }
    
    private async void ShowAyaImage(QuranAya aya)
    {
        try
        {
            if (_pictureService == null || _ayaImage == null)
                return;
            // Ensure image exists locally (download if allowed and online)
            var ok = await _pictureService.EnsurePictureAvailableAsync(aya.SurahNumber, aya.AyaNumber);
            var path = _pictureService.GetPicturePath(aya.SurahNumber, aya.AyaNumber);
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
                    if (!ok)
                        UpdateStatus("Image not available (offline or cannot download).");
                }
            });
        }
        catch (Exception ex)
        {
            ShowError($"Image load failed: {ex.Message}");
        }
    }
    
    private void OnSequencePlaybackEnded(object? sender, EventArgs e)
    {
        RunOnUiThread(() =>
        {
            if (_isPlayingSequence)
            {
                _currentPlayingIndex++;
                if (_currentPlayingIndex < _searchResults.Count)
                {
                    PlayCurrentAya(false);
                }
                else
                {
                    _isPlayingSequence = false;
                    UpdateStatus(GetString(Resource.String.ready));
                }
            }
        });
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
    
    private void UpdateStatus(string message)
    {
        RunOnUiThread(() =>
        {
            if (_statusText != null)
            {
                _statusText.Text = message;
            }
        });
    }
    
    private void ShowError(string message)
    {
        RunOnUiThread(() =>
        {
            Toast.MakeText(this, message, ToastLength.Long)?.Show();
            UpdateStatus(message);
        });
    }
    
    private void UpdateAudioButtonsState(bool hasResults)
    {
        var isPlaying = _audioService?.IsPlaying ?? false;
        
        if (_playButton != null)
            _playButton.Enabled = hasResults && !isPlaying;
        if (_playRepeatButton != null)
            _playRepeatButton.Enabled = hasResults && !isPlaying;
        if (_playAllButton != null)
            _playAllButton.Enabled = hasResults && !isPlaying;
        if (_stopButton != null)
            _stopButton.Enabled = isPlaying;
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
            // Reload settings from SharedPreferences
            LoadSettings();
            
            // Show confirmation
            Toast.MakeText(this, "Settings applied successfully", ToastLength.Short)?.Show();
        }
    }

    private void LoadSettings()
    {
        var prefs = GetSharedPreferences("QuranSearchSettings", FileCreationMode.Private);
        
        // Load language
        var language = prefs?.GetString("Language", "en") ?? "en";
        if (_localizationService != null)
        {
            _localizationService.CurrentLanguage = language;
        }
        
        // Update language spinner to match
        var languageIndex = language switch
        {
            "ar" => 0,
            "en" => 1,
            "de" => 2,
            _ => 1
        };
        _languageSpinner?.SetSelection(languageIndex);

        // Load search domain
        _searchDomainType = (SearchDomainType)(prefs?.GetInt("SearchDomainType", 0) ?? 0);
        _searchStartSurah = prefs?.GetInt("StartSurah", 1) ?? 1;
        _searchEndSurah = prefs?.GetInt("EndSurah", 114) ?? 114;

        // Load online resource settings
        var useRemoteAudio = prefs?.GetBoolean("UseRemoteAudio", true) ?? true;
        var useRemoteImages = prefs?.GetBoolean("UseRemoteImages", false) ?? false;
        
        if (_audioService != null)
        {
            _audioService.UseRemoteSource = useRemoteAudio;
        }
        if (_pictureService != null)
        {
            _pictureService.UseRemoteSource = useRemoteImages;
        }
        if (_useRemoteAudioCheckBox != null)
        {
            _useRemoteAudioCheckBox.Checked = useRemoteAudio;
        }
    }

    protected override void OnDestroy()
    {
        _audioService?.Dispose();
        base.OnDestroy();
    }
}