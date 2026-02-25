using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.Core.View;
using AndroidX.RecyclerView.Widget;
using Kuttab.Android.Adapters;
using Kuttab.Android.Services;
using Kuttab.Android.Utils;
using Kuttab.Core.Interfaces;
using Kuttab.Core.Models;
using Kuttab.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Kuttab.Android;

[Activity(Label = "@string/app_name", MainLauncher = true, Theme = "@style/SplashTheme")]
public class MainActivity : AppCompatActivity
{
    private QuranSearchService? _searchService;
    private AndroidAudioService? _audioService;
    private AndroidPictureService? _pictureService;
    private LocalizationService? _localizationService;
    private AndroidAudioRecordingService? _recordingService;
    private SimpleNotificationService? _notificationService;
    private SimpleUpdateService? _updateService;
    private NotificationContentService? _contentService;
    
    private LinearLayout? _categoryContainer;
    private TextView? _categoryIcon;
    private Spinner? _categorySpinner;
    private LinearLayout? _ruleContainer;
    private TextView? _ruleIcon;
    private Spinner? _ruleSpinner;
    private string? _selectedRuleName;
    private string? _selectedCategoryId;
    private readonly Dictionary<string, string> _ruleDisplayToArabic = new();
    private readonly Dictionary<string, string> _categoryDisplayToId = new();
    private readonly HashSet<string> _groupHeaders = new();
    private Button? _settingsButton;
    private Button? _infoButton;
    private Button? _recitationButton;
    private Button? _donateButton;
    private Button? _ruleButton;
        private bool _showTajweedRules = true;
    private Button? _playButton;
    private Button? _playRepeatButton;
    private Button? _playAllButton;
    private Button? _stopButton;
    private TextView? _statusText;
    private ImageView? _ayaImage;
    private RecyclerView? _recyclerView;
    
    // Recording UI Elements
    private LinearLayout? _recordingSection;
    private LinearLayout? _audioControlsSection;
    private LinearLayout? _bottomControlsContainer;
    private TextView? _reciterLabel;
    private TextView? _yourRecordingLabel;
    private Button? _recordButton;
    private Button? _stopRecordingButton;
    private Button? _playRecordingButton;
    private Button? _deleteRecordingButton;
    private bool _recordingEnabled = true; // Default to enabled
    private AyaAdapter? _adapter;
    
    private List<QuranAya> _searchResults = new();
    private int _currentPlayingIndex = -1;
    private bool _isPlayingSequence = false;
    private SearchDomainType _searchDomainType = SearchDomainType.WholeQuran;
    private int _searchStartSurah = 1;
    private int _searchEndSurah = 114;
    private string _selectedReciter = "Husary_128kbps";
    private PowerManager.WakeLock? _wakeLock;
    
    private const int SETTINGS_REQUEST_CODE = 1001;
    private const int NOTIFICATION_PERMISSION_REQUEST_CODE = 1002;
    
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        // Switch from splash theme to app theme
        SetTheme(Resource.Style.AppTheme);
        
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
        
        SetContentView(Resource.Layout.activity_main);
        
        InitializeServices();
        InitializeViews();
        SetupRecyclerView();
        LoadSettings();
        LoadDataAsync();
        
        // Initialize notification and update services
        InitializeNotificationServices();
        
        // Check for app updates
        CheckForAppUpdates();
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
            _recordingService = new AndroidAudioRecordingService(this);
            
            // Initialize wake lock to keep screen on during playback
            var powerManager = (PowerManager?)GetSystemService(PowerService);
            if (powerManager != null)
            {
                _wakeLock = powerManager.NewWakeLock(WakeLockFlags.ScreenDim, "QuranSearch::PlaybackWakeLock");
            }
            
            // Setup audio event handlers
            if (_audioService != null)
            {
                _audioService.PlaybackStateChanged += OnPlaybackStateChanged;
                _audioService.SequencePlaybackEnded += OnSequencePlaybackEnded;
                _audioService.PlaybackError += (s, msg) => ShowError(msg);
            }
            
            // Setup recording event handlers
            if (_recordingService != null)
            {
                _recordingService.RecordingStateChanged += OnRecordingStateChanged;
                _recordingService.RecordingError += (s, msg) => ShowError(msg);
            }
        }
        catch (Exception ex)
        {
            ShowError($"Failed to initialize services: {ex.Message}");
        }
    }
    
    private void InitializeNotificationServices()
    {
        try
        {
            _notificationService = new SimpleNotificationService(this);
            _updateService = new SimpleUpdateService(this, _notificationService);
            _contentService = new NotificationContentService(this, _notificationService);
            
            // Request notification permission for Android 13+
            _notificationService.RequestNotificationPermission(this, NOTIFICATION_PERMISSION_REQUEST_CODE);
            
            // Schedule periodic checks
            _updateService.SchedulePeriodicUpdateCheck();
            _contentService.SchedulePeriodicContentCheck();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to initialize notification services: {ex.Message}");
        }
    }
    
    private async void CheckForAppUpdates()
    {
        try
        {
            if (_updateService != null)
            {
                await _updateService.CheckForUpdatesAsync();
            }
            
            // Also check for content notifications
            if (_contentService != null)
            {
                await _contentService.CheckForContentNotificationsAsync(forceCheck: false);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to check for updates: {ex.Message}");
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
        _categoryContainer = FindViewById<LinearLayout>(Resource.Id.categoryContainer);
        _categoryIcon = FindViewById<TextView>(Resource.Id.categoryIcon);
        _categorySpinner = FindViewById<Spinner>(Resource.Id.categorySpinner);
        _ruleContainer = FindViewById<LinearLayout>(Resource.Id.ruleContainer);
        _ruleIcon = FindViewById<TextView>(Resource.Id.ruleIcon);
        _ruleSpinner = FindViewById<Spinner>(Resource.Id.ruleSpinner);
        _settingsButton = FindViewById<Button>(Resource.Id.settingsButton);
        _infoButton = FindViewById<Button>(Resource.Id.infoButton);
        _recitationButton = FindViewById<Button>(Resource.Id.recitationButton);
        _donateButton = FindViewById<Button>(Resource.Id.donateButton);
        _ruleButton = FindViewById<Button>(Resource.Id.ruleButton);
        _playButton = FindViewById<Button>(Resource.Id.playButton);
        _playRepeatButton = FindViewById<Button>(Resource.Id.playRepeatButton);
        _playAllButton = FindViewById<Button>(Resource.Id.playAllButton);
        _stopButton = FindViewById<Button>(Resource.Id.stopButton);
        _statusText = FindViewById<TextView>(Resource.Id.statusText);
        _ayaImage = FindViewById<ImageView>(Resource.Id.ayaImage);
        _recyclerView = FindViewById<RecyclerView>(Resource.Id.recyclerView);
        
        // Recording UI Elements
        _recordingSection = FindViewById<LinearLayout>(Resource.Id.recordingSection);
        _audioControlsSection = FindViewById<LinearLayout>(Resource.Id.audioControlsSection);
        _bottomControlsContainer = FindViewById<LinearLayout>(Resource.Id.bottomControlsContainer);
        _reciterLabel = FindViewById<TextView>(Resource.Id.reciterLabel);
        _yourRecordingLabel = FindViewById<TextView>(Resource.Id.yourRecordingLabel);
        _recordButton = FindViewById<Button>(Resource.Id.recordButton);
        _stopRecordingButton = FindViewById<Button>(Resource.Id.stopRecordingButton);
        _playRecordingButton = FindViewById<Button>(Resource.Id.playRecordingButton);
        _deleteRecordingButton = FindViewById<Button>(Resource.Id.deleteRecordingButton);
        
        // Setup button click handlers
        if (_settingsButton != null)
            _settingsButton.Click += OnSettingsClick;
        if (_infoButton != null)
            _infoButton.Click += OnInfoClick;
        if (_recitationButton != null)
            _recitationButton.Click += OnRecitationClick;
        if (_donateButton != null)
            _donateButton.Click += OnDonateClick;
        if (_ruleButton != null)
            _ruleButton.Click += OnRuleClick;
        if (_playButton != null)
            _playButton.Click += OnPlayClick;
        if (_playRepeatButton != null)
            _playRepeatButton.Click += OnPlayRepeatClick;
        if (_playAllButton != null)
            _playAllButton.Click += OnPlayAllClick;
        if (_stopButton != null)
            _stopButton.Click += OnStopClick;
        
        // Setup recording button handlers
        if (_recordButton != null)
            _recordButton.Click += OnRecordClick;
        if (_stopRecordingButton != null)
            _stopRecordingButton.Click += OnStopRecordingClick;
        if (_playRecordingButton != null)
            _playRecordingButton.Click += OnPlayRecordingClick;
        if (_deleteRecordingButton != null)
            _deleteRecordingButton.Click += OnDeleteRecordingClick;
        
        // Category spinner selection handler
        if (_categorySpinner != null)
            _categorySpinner.ItemSelected += OnCategorySpinnerItemSelected;
        
        // Hide currently displayed Aya when user picks a new rule (actual rule, not a group header)
        if (_ruleSpinner != null)
            _ruleSpinner.ItemSelected += OnRuleSpinnerItemSelected;
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
            
            PerformSearch(arabicRuleName);
        }
        catch (Exception ex)
        {
            ShowError($"Search failed: {ex.Message}");
        }
    }
    
    private async void PerformSearch(string arabicRuleName)
    {
        try
        {
            if (_searchService == null) return;
            
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
            
            // Scroll to top to show first result
            ScrollToTop();
            
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
    
    private void ClearSearchResults()
    {
        try
        {
            // Clear search results list
            _searchResults.Clear();
            
            // Reset UI state
            _currentPlayingIndex = -1;
            _isPlayingSequence = false;
            
            // Stop any playing audio
            _audioService?.StopPlayback();
            
            // Clear the adapter data
            _adapter?.UpdateData(new List<QuranAya>());
            
            // Hide the ayah image
            HideAyaImage();
            
            // Disable audio buttons
            UpdateAudioButtonsState(false);
            
            // Update status to show ready state
            if (_localizationService != null)
                UpdateStatus(_localizationService.GetString("Ready"));
            else
                UpdateStatus("Ready");
        }
        catch (Exception ex)
        {
            // Log error but don't show to user as this is a cleanup operation
            System.Diagnostics.Debug.WriteLine($"Error clearing search results: {ex.Message}");
        }
    }
    
    private void ScrollToTop()
    {
        try
        {
            if (_recyclerView != null)
            {
                RunOnUiThread(() =>
                {
                    _recyclerView.ScrollToPosition(0);
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error scrolling to top: {ex.Message}");
        }
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
                // Store the selected rule name
                _selectedRuleName = mapped;
                
                // Automatically trigger search
                PerformSearch(mapped);
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
            UpdateRecordingVisibility();
        }
        catch { /* no-op */ }
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
        UpdateRecordingVisibility();
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
        UpdateRecordingVisibility();
    }
    
    private void OnPlayAllClick(object? sender, EventArgs e)
    {
        if (_searchResults.Count > 0)
        {
            _isPlayingSequence = true;
            _currentPlayingIndex = 0;
            HighlightAndScrollToPlayingAya();
            PlayCurrentAya(false);
        }
        UpdateRecordingVisibility();
    }
    
    private void OnStopClick(object? sender, EventArgs e)
    {
        _audioService?.StopPlayback();
        _isPlayingSequence = false;
        _adapter?.ClearPlayingPosition();
        ReleaseWakeLock();
        UpdateStatus(GetString(Resource.String.stopped));
        UpdateRecordingVisibility();
    }
    
    private void UpdateLayoutDirection(string language)
    {
        // Set layout direction based on language
        var layoutDirection = language == "ar" ? LayoutDirection.Rtl : LayoutDirection.Ltr;
        
        // Update main containers
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
            // Audio buttons are now icon-only, no text updates needed
            
            // Update bottom control labels based on selected language
            if (_reciterLabel != null)
                _reciterLabel.Text = _localizationService.GetString("ReciterBottomLabel");
            if (_yourRecordingLabel != null)
                _yourRecordingLabel.Text = _localizationService.GetString("YourRecordingBottomLabel");
            
            // Update rule button text
            if (_ruleButton != null)
                _ruleButton.Text = _localizationService.GetString("RuleExplanation");
            
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
        if (_searchService == null || _localizationService == null)
            return;

        var languageCode = _localizationService.CurrentLanguage;
        
        // Update category spinner
        UpdateCategorySpinner(languageCode);
        
        // Update rule spinner based on selected category
        UpdateRuleSpinnerForCategory(languageCode);
    }

    private void UpdateRuleButtonVisibility()
    {
        if (_ruleButton == null) return;
        
        RunOnUiThread(() =>
        {
            _ruleButton.Visibility = _showTajweedRules ? ViewStates.Visible : ViewStates.Gone;
        });
    }

    private void UpdateCategorySpinner(string languageCode)
    {
        if (_categorySpinner == null) return;

        var categories = RuleGroupTranslator.GetAllGroups(languageCode);
        var displayNames = new List<string>();
        _categoryDisplayToId.Clear();

        foreach (var (groupId, title) in categories)
        {
            displayNames.Add(title);
            _categoryDisplayToId[title] = groupId;
        }

        var categoryAdapter = new ArrayAdapter<string>(this,
            Resource.Layout.item_rule, displayNames);
        categoryAdapter.SetDropDownViewResource(Resource.Layout.item_rule);
        _categorySpinner.Adapter = categoryAdapter;

        // Select first category by default if none selected
        if (_selectedCategoryId == null && categories.Count > 0)
        {
            _selectedCategoryId = categories[0].GroupId;
        }
    }

    private void UpdateRuleSpinnerForCategory(string languageCode)
    {
        if (_ruleSpinner == null || _searchService == null || string.IsNullOrEmpty(_selectedCategoryId))
            return;

        var rules = _searchService.GetRules();
        var displayNames = new List<string>();
        _ruleDisplayToArabic.Clear();
        _groupHeaders.Clear();

        // Filter rules by selected category
        var filteredRules = rules.Where(r => r?.Group == _selectedCategoryId).ToList();

        foreach (var rule in filteredRules)
        {
            if (rule == null) continue;
            var localizedName = RuleNameTranslator.GetLocalizedName(rule.Name, languageCode);
            displayNames.Add(localizedName);
            _ruleDisplayToArabic[localizedName] = rule.Name;
        }

        var ruleAdapter = new ArrayAdapter<string>(this,
            Resource.Layout.item_rule, displayNames);
        ruleAdapter.SetDropDownViewResource(Resource.Layout.item_rule);
        _ruleSpinner.Adapter = ruleAdapter;
    }

    private void OnCategorySpinnerItemSelected(object? sender, AdapterView.ItemSelectedEventArgs e)
    {
        try
        {
            if (_categorySpinner == null || _localizationService == null) return;
            var selected = _categorySpinner.GetItemAtPosition(e.Position)?.ToString();
            if (string.IsNullOrEmpty(selected)) return;

            // Get the category ID from the display name
            if (_categoryDisplayToId.TryGetValue(selected, out var categoryId))
            {
                _selectedCategoryId = categoryId;
                // Update the rule spinner with rules from this category
                UpdateRuleSpinnerForCategory(_localizationService.CurrentLanguage);
            }

            // Hide aya image when category changes
            if (_ayaImage != null)
                _ayaImage.Visibility = global::Android.Views.ViewStates.Gone;
        }
        catch { /* no-op */ }
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
            
            // Keep screen on during playback
            AcquireWakeLock();
            
            var aya = _searchResults[_currentPlayingIndex];
            _audioService.SetRepeatMode(repeat);
            
            UpdateStatus($"Playing: {aya.SurahNumber}:{aya.AyaNumber}");
            await _audioService.PlayAyaAsync(aya.SurahNumber, aya.AyaNumber);
            UpdateStatus($"{GetString(Resource.String.playing)} {aya.SurahNumber}:{aya.AyaNumber}");
        }
        catch (Exception ex)
        {
            ReleaseWakeLock();
            ShowError($"Playback failed: {ex.Message}");
        }
    }
    
    private void OnPlaybackStateChanged(object? sender, bool isPlaying)
    {
        RunOnUiThread(() =>
        {
            // Only update UI state - sequence playback is handled by OnSequencePlaybackEnded
            UpdateAudioButtonsState(_searchResults.Count > 0);
            
            // Release wake lock when single ayah playback ends (not in sequence mode)
            if (!isPlaying && !_isPlayingSequence)
            {
                ReleaseWakeLock();
            }
            
            // Update recording visibility when playback state changes
            UpdateRecordingVisibility();
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
                    HighlightAndScrollToPlayingAya();
                    PlayCurrentAya(false);
                }
                else
                {
                    _isPlayingSequence = false;
                    _adapter?.ClearPlayingPosition();
                    ReleaseWakeLock();
                    UpdateStatus(GetString(Resource.String.ready));
                }
            }
        });
    }
    
    private void HighlightAndScrollToPlayingAya()
    {
        if (_adapter == null || _recyclerView == null || _currentPlayingIndex < 0)
            return;
        
        // Highlight the currently playing ayah
        _adapter.SetPlayingPosition(_currentPlayingIndex);
        
        // Scroll to make the playing ayah visible
        var layoutManager = _recyclerView.GetLayoutManager() as LinearLayoutManager;
        if (layoutManager != null)
        {
            // Check if the item is visible
            var firstVisible = layoutManager.FindFirstCompletelyVisibleItemPosition();
            var lastVisible = layoutManager.FindLastCompletelyVisibleItemPosition();
            
            // If the playing item is not visible, scroll to it
            if (_currentPlayingIndex < firstVisible || _currentPlayingIndex > lastVisible)
            {
                // Use smooth scroll for better UX
                _recyclerView.SmoothScrollToPosition(_currentPlayingIndex);
            }
        }
    }
    
    private string GetRemoteAudioUrl(int surah, int aya)
    {
        var paddedSurah = surah.ToString("D3");
        var paddedAya = aya.ToString("D3");
        return $"https://everyayah.com/data/{_selectedReciter}/{paddedSurah}{paddedAya}.mp3";
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
    
    private void OnRecitationClick(object? sender, EventArgs e)
    {
        var intent = new Intent(this, typeof(RecitationActivity));
        StartActivity(intent);
    }

    private void OnDonateClick(object? sender, EventArgs e)
    {
        var intent = new Intent(this, typeof(DonateActivity));
        StartActivity(intent);
    }
    
    private void OnInfoClick(object? sender, EventArgs e)
    {
        var intent = new Intent(this, typeof(InfoActivity));
        StartActivity(intent);
    }
    
    private void OnRuleClick(object? sender, EventArgs e)
    {
        try
        {
            if (_ruleSpinner == null || _localizationService == null) return;
            
            // Get the selected rule
            var selectedDisplayName = _ruleSpinner.SelectedItem?.ToString();
            string? arabicRuleName = null;
            
            System.Diagnostics.Debug.WriteLine($"[RULE] Selected display name: '{selectedDisplayName}'");
            System.Diagnostics.Debug.WriteLine($"[RULE] Available mappings: {string.Join(", ", _ruleDisplayToArabic.Keys)}");
            
            if (!string.IsNullOrEmpty(selectedDisplayName) && _ruleDisplayToArabic.TryGetValue(selectedDisplayName, out var mappedName))
            {
                if (!string.IsNullOrEmpty(mappedName))
                {
                    arabicRuleName = mappedName;
                    System.Diagnostics.Debug.WriteLine($"[RULE] Found Arabic name: '{arabicRuleName}'");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[RULE] No mapping found for: '{selectedDisplayName}'");
            }
            
            // If we have a valid rule name, show the explanation
            if (!string.IsNullOrEmpty(arabicRuleName))
            {
                var intent = new Intent(this, typeof(TajweedRuleActivity));
                intent.PutExtra("ArabicRuleName", arabicRuleName);
                StartActivity(intent);
            }
            else
            {
                ShowError(_localizationService.GetString("NoRuleSelected"));
            }
        }
        catch (Exception ex)
        {
            ShowError($"Error showing rule explanation: {ex.Message}");
        }
    }

    protected override void OnActivityResult(int requestCode, Result resultCode, Intent? data)
    {
        base.OnActivityResult(requestCode, resultCode, data);

        if (requestCode == SETTINGS_REQUEST_CODE && resultCode == Result.Ok)
        {
            // Clear search results when settings are changed
            ClearSearchResults();
            
            // Reload settings from SharedPreferences
            LoadSettings();
            
            // Show confirmation
            Toast.MakeText(this, "Settings applied successfully", ToastLength.Short)?.Show();
        }
    }

    private void LoadSettings()
    {
        var prefs = GetSharedPreferences("QuranSearchSettings", FileCreationMode.Private);
        
        // Load language with system language detection and fallback
        var language = LanguageHelper.GetLanguageFromPreferences(this);
        if (_localizationService != null)
        {
            _localizationService.CurrentLanguage = language;
            UpdateLayoutDirection(language);
            UpdateUIStringsForCurrentLanguage();
            UpdateAdapterLocalization();
            UpdateRuleDisplayNames();
        }

        // Load search domain
        _searchDomainType = (SearchDomainType)(prefs?.GetInt("SearchDomainType", 0) ?? 0);
        _searchStartSurah = prefs?.GetInt("StartSurah", 1) ?? 1;
        _searchEndSurah = prefs?.GetInt("EndSurah", 114) ?? 114;

        // Load online resource settings
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
        
        // Load font size setting
        var fontSize = prefs?.GetInt("FontSize", 18) ?? 18;
        if (_adapter != null)
        {
            _adapter.SetFontSize(fontSize);
        }
        
        // Load Quran text file setting
        var quranTextFile = prefs?.GetString("QuranTextFile", "quran-uthmani-ver1.2.txt") ?? "quran-uthmani-ver1.2.txt";
        if (_searchService != null)
        {
            _searchService.SetQuranTextFile(quranTextFile);
        }
        
        // Load recording enabled setting
        _recordingEnabled = prefs?.GetBoolean("RecordingEnabled", true) ?? true;
        
        System.Diagnostics.Debug.WriteLine($"[MAIN] Recording enabled: {_recordingEnabled}");
        
                
        // Load show Tajweed rules setting
        _showTajweedRules = prefs?.GetBoolean("ShowTajweedRules", true) ?? true;
        
        // Update Rule button visibility
        UpdateRuleButtonVisibility();
    }

    // Recording event handlers
    private async void OnRecordClick(object? sender, EventArgs e)
    {
        if (_recordingService == null || _currentPlayingIndex < 0 || _currentPlayingIndex >= _searchResults.Count)
            return;
        
        // Check permission
        if (!await _recordingService.RequestRecordingPermissionAsync())
        {
            ShowError("Recording permission is required to record audio");
            return;
        }
        
        var aya = _searchResults[_currentPlayingIndex];
        
        // Start recording for the selected Ayah
        if (await _recordingService.StartRecordingAsync(aya.SurahNumber, aya.AyaNumber))
        {
            UpdateRecordingUI();
        }
    }
    
    private async void OnStopRecordingClick(object? sender, EventArgs e)
    {
        if (_recordingService == null) return;
        
        await _recordingService.StopRecordingAsync();
        UpdateRecordingUI();
    }
    
    private async void OnPlayRecordingClick(object? sender, EventArgs e)
    {
        if (_recordingService == null || _currentPlayingIndex < 0 || _currentPlayingIndex >= _searchResults.Count)
            return;
        
        var aya = _searchResults[_currentPlayingIndex];
        await _recordingService.PlayRecordingAsync(aya.SurahNumber, aya.AyaNumber);
    }
    
    private async void OnDeleteRecordingClick(object? sender, EventArgs e)
    {
        if (_recordingService == null || _currentPlayingIndex < 0 || _currentPlayingIndex >= _searchResults.Count)
            return;
        
        var aya = _searchResults[_currentPlayingIndex];
        await _recordingService.DeleteRecordingAsync(aya.SurahNumber, aya.AyaNumber);
        UpdateRecordingUI();
    }
    
    private void OnRecordingStateChanged(object? sender, bool isRecording)
    {
        RunOnUiThread(() => UpdateRecordingUI());
    }
    
    
    private void UpdateRecordingUI()
    {
        if (_recordingService == null) return;
        
        var isRecording = _recordingService.IsRecording;
        var hasAyaSelected = _currentPlayingIndex >= 0 && _currentPlayingIndex < _searchResults.Count;
        var hasRecording = false;
        
        // Check if current Ayah has a recording
        if (hasAyaSelected)
        {
            var aya = _searchResults[_currentPlayingIndex];
            hasRecording = _recordingService.HasRecordingForAya(aya.SurahNumber, aya.AyaNumber);
        }
        
        // Update button states - only enable when an Ayah is selected
        if (_recordButton != null)
            _recordButton.Enabled = hasAyaSelected && !isRecording;
        if (_stopRecordingButton != null)
            _stopRecordingButton.Enabled = isRecording;
        if (_playRecordingButton != null)
            _playRecordingButton.Enabled = hasAyaSelected && !isRecording && hasRecording;
        if (_deleteRecordingButton != null)
            _deleteRecordingButton.Enabled = hasAyaSelected && !isRecording && hasRecording;
        
    }
    
    private void UpdateRecordingVisibility()
    {
        if (_recordingSection != null)
        {
            // Show recording section only when:
            // 1. Single file is selected (_currentPlayingIndex >= 0)
            // 2. Playback is not active (audio finished or stopped)
            // 3. Recording is enabled in settings
            var hasSingleFileSelected = _currentPlayingIndex >= 0 && _currentPlayingIndex < _searchResults.Count;
            var isPlaybackActive = _audioService?.IsPlaying ?? false;
            var shouldShow = _recordingEnabled && hasSingleFileSelected && !isPlaybackActive;
            
            _recordingSection.Visibility = shouldShow ? global::Android.Views.ViewStates.Visible : global::Android.Views.ViewStates.Gone;
            System.Diagnostics.Debug.WriteLine($"[MAIN] Recording section visibility: enabled={_recordingEnabled}, hasFile={hasSingleFileSelected}, playing={isPlaybackActive}, show={shouldShow}");
            
            // Update audio controls alignment based on recording section visibility
            UpdateAudioControlsAlignment(shouldShow);
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("[MAIN] Recording section is NULL!");
        }
    }
    
    private void UpdateAudioControlsAlignment(bool recordingSectionVisible)
    {
        if (_bottomControlsContainer != null && _audioControlsSection != null && _recordingSection != null)
        {
            RunOnUiThread(() =>
            {
                var currentLanguage = System.Globalization.CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
                
                if (recordingSectionVisible)
                {
                    // Both sections are visible - handle layout direction based on language
                    if (currentLanguage == "ar")
                    {
                        // Arabic: Recording on left, Reciter on right
                        // Remove both views from container
                        _bottomControlsContainer.RemoveView(_audioControlsSection);
                        _bottomControlsContainer.RemoveView(_recordingSection);
                        
                        // Add Recording first (left), then Reciter (right)
                        _bottomControlsContainer.AddView(_recordingSection, 0);
                        _bottomControlsContainer.AddView(_audioControlsSection, 1);
                        
                        // Update margins
                        var recordingParams = _recordingSection.LayoutParameters as LinearLayout.LayoutParams;
                        var audioParams = _audioControlsSection.LayoutParameters as LinearLayout.LayoutParams;
                        
                        if (recordingParams != null)
                        {
                            recordingParams.RightMargin = (int)(8 * Resources.DisplayMetrics.Density);
                            recordingParams.LeftMargin = 0;
                            _recordingSection.LayoutParameters = recordingParams;
                        }
                        
                        if (audioParams != null)
                        {
                            audioParams.LeftMargin = (int)(8 * Resources.DisplayMetrics.Density);
                            audioParams.RightMargin = 0;
                            _audioControlsSection.LayoutParameters = audioParams;
                        }
                    }
                    else
                    {
                        // English/German: Reciter on left, Recording on right (default layout)
                        // Remove both views from container
                        _bottomControlsContainer.RemoveView(_audioControlsSection);
                        _bottomControlsContainer.RemoveView(_recordingSection);
                        
                        // Add Reciter first (left), then Recording (right)
                        _bottomControlsContainer.AddView(_audioControlsSection, 0);
                        _bottomControlsContainer.AddView(_recordingSection, 1);
                        
                        // Update margins
                        var audioParams = _audioControlsSection.LayoutParameters as LinearLayout.LayoutParams;
                        var recordingParams = _recordingSection.LayoutParameters as LinearLayout.LayoutParams;
                        
                        if (audioParams != null)
                        {
                            audioParams.RightMargin = (int)(8 * Resources.DisplayMetrics.Density);
                            audioParams.LeftMargin = 0;
                            _audioControlsSection.LayoutParameters = audioParams;
                        }
                        
                        if (recordingParams != null)
                        {
                            recordingParams.LeftMargin = (int)(8 * Resources.DisplayMetrics.Density);
                            recordingParams.RightMargin = 0;
                            _recordingSection.LayoutParameters = recordingParams;
                        }
                    }
                }
                else
                {
                    // Only reciter controls visible - center them
                    var audioParams = _audioControlsSection.LayoutParameters as LinearLayout.LayoutParams;
                    if (audioParams != null)
                    {
                        audioParams.Weight = 1;
                        audioParams.LeftMargin = 0;
                        audioParams.RightMargin = 0;
                        _audioControlsSection.LayoutParameters = audioParams;
                    }
                }
            });
        }
    }
    
    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
    {
        base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        
        try
        {
            if (requestCode == NOTIFICATION_PERMISSION_REQUEST_CODE)
            {
                if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                {
                    // Permission granted, can now show notifications
                    System.Diagnostics.Debug.WriteLine("Notification permission granted");
                    
                    // Check for updates now that we have permission
                    CheckForAppUpdates();
                }
                else
                {
                    // Permission denied
                    System.Diagnostics.Debug.WriteLine("Notification permission denied");
                    ShowError("Notifications are disabled. You won't receive update notifications.");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error handling permission result: {ex.Message}");
        }
    }

    protected override void OnDestroy()
    {
        ReleaseWakeLock();
        _audioService?.Dispose();
        _recordingService?.Dispose();
        base.OnDestroy();
    }

}