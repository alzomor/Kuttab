using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using Kuttab.Core.Models;
using Kuttab.Core.Services;
using Kuttab.Android.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Kuttab.Android;

[Activity(Label = "Settings", Theme = "@style/AppTheme")]
public class SettingsActivity : AppCompatActivity
{
    private Spinner? _languageSpinner;
    private Spinner? _searchDomainSpinner;
    private LinearLayout? _singleSurahContainer;
    private LinearLayout? _surahRangeContainer;
    private Spinner? _singleSurahSpinner;
    private Spinner? _startSurahSpinner;
    private Spinner? _endSurahSpinner;
    private CheckBox? _useRemoteAudioCheckBox;
    private CheckBox? _useRemoteImagesCheckBox;
    private CheckBox? _enableRecordingCheckBox;
    private CheckBox? _showTajweedRulesCheckBox;
    private LinearLayout? _reciterContainer;
    private TextView? _selectReciterLabel;
    private Spinner? _reciterSpinner;
    private Spinner? _fontSizeSpinner;
    private Spinner? _quranTextSpinner;
    private TextView? _quranTextLabel;
    private Button? _saveButton;
    private Button? _cancelButton;
    
    // Tajweed rule selection
    private global::AndroidX.CardView.Widget.CardView? _tajweedRuleSelectionCard;
    private TextView? _tajweedRuleSelectionTitle;
    private Button? _selectAllRulesButton;
    private Button? _deselectAllRulesButton;
    private LinearLayout? _tajweedRuleCheckboxContainer;
    private List<(CheckBox checkbox, string ruleName)> _ruleCheckboxes = new();
    
    // Text views for dynamic language update
    private TextView? _languageSettingsTitle;
    private TextView? _displaySettingsTitle;
    private TextView? _fontSizeLabel;
    private TextView? _selectLanguageLabel;
    private TextView? _searchDomainTitle;
    private TextView? _searchInLabel;
    private TextView? _surahNumberLabel;
    private TextView? _fromSurahLabel;
    private TextView? _toSurahLabel;
    private TextView? _onlineResourcesTitle;
    private TextView? _onlineResourcesNote;

    private LocalizationService? _localizationService;
    private bool _hasUnsavedChanges = false;
    private bool _initialLoadComplete = false;
    private List<LanguageOption> _availableLanguages = new();
    private List<SearchDomainOption> _searchDomainOptions = new();
    private List<string> _surahDisplayNames = new();
    private List<ReciterOption> _reciterOptions = new();
    private List<int> _fontSizeOptions = new() { 12, 14, 16, 18, 20, 22, 24, 26, 28, 30, 32 };
    private List<QuranTextOption> _quranTextOptions = new()
    {
        new QuranTextOption { FileName = "quran-uthmani-ver1.2.txt", DisplayKey = "QuranTextUthmaniClean" },
        new QuranTextOption { FileName = "quran-uthmani.txt", DisplayKey = "QuranTextUthmaniOriginal" }
    };

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        SetContentView(Resource.Layout.activity_settings);

        // Enable back button in action bar
        if (SupportActionBar != null)
        {
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);
        }

        InitializeServices();
        InitializeViews();
        LoadCurrentSettings();
        SetupEventHandlers();
        _initialLoadComplete = true;
    }

    private void InitializeServices()
    {
        var fileService = new Services.AndroidFileService(this);
        _localizationService = new LocalizationService(fileService);
        
        // Load saved language from SharedPreferences with system language detection
        var language = LanguageHelper.GetLanguageFromPreferences(this);
        _localizationService.CurrentLanguage = language;
        
        _availableLanguages = _localizationService.AvailableLanguages;
        
        // Initialize search domain options
        _searchDomainOptions = new List<SearchDomainOption>
        {
            new SearchDomainOption { Type = SearchDomainType.WholeQuran, DisplayKey = "WholeQuran" },
            new SearchDomainOption { Type = SearchDomainType.SingleSurah, DisplayKey = "SingleSurah" },
            new SearchDomainOption { Type = SearchDomainType.SurahRange, DisplayKey = "SurahRange" }
        };
        
        // Initialize reciter options
        _reciterOptions = new List<ReciterOption>
        {
            new ReciterOption { FolderName = "Abdul_Basit_Murattal_192kbps", DisplayNameResId = Resource.String.reciter_abdul_basit },
            new ReciterOption { FolderName = "Ayman_Sowaid_64kbps", DisplayNameResId = Resource.String.reciter_ayman_sowaid },
            new ReciterOption { FolderName = "Husary_128kbps", DisplayNameResId = Resource.String.reciter_husary },
            new ReciterOption { FolderName = "Husary_Muallim_128kbps", DisplayNameResId = Resource.String.reciter_husary_muallim },
            new ReciterOption { FolderName = "Menshawi_32kbps", DisplayNameResId = Resource.String.reciter_menshawi },
            new ReciterOption { FolderName = "Mohammad_al_Tablaway_128kbps", DisplayNameResId = Resource.String.reciter_tablaway },
            new ReciterOption { FolderName = "Mustafa_Ismail_48kbps", DisplayNameResId = Resource.String.reciter_mustafa_ismail },
            new ReciterOption { FolderName = "Muhammad_Ayyoub_128kbps", DisplayNameResId = Resource.String.reciter_ayyoub },
            new ReciterOption { FolderName = "mahmoud_ali_al_banna_32kbps", DisplayNameResId = Resource.String.reciter_banna }
        };
    }

    private void InitializeViews()
    {
        _languageSpinner = FindViewById<Spinner>(Resource.Id.languageSpinner);
        _searchDomainSpinner = FindViewById<Spinner>(Resource.Id.searchDomainSpinner);
        _singleSurahContainer = FindViewById<LinearLayout>(Resource.Id.singleSurahContainer);
        _surahRangeContainer = FindViewById<LinearLayout>(Resource.Id.surahRangeContainer);
        _singleSurahSpinner = FindViewById<Spinner>(Resource.Id.singleSurahSpinner);
        _startSurahSpinner = FindViewById<Spinner>(Resource.Id.startSurahSpinner);
        _endSurahSpinner = FindViewById<Spinner>(Resource.Id.endSurahSpinner);
        _useRemoteAudioCheckBox = FindViewById<CheckBox>(Resource.Id.useRemoteAudioCheckBox);
        _reciterContainer = FindViewById<LinearLayout>(Resource.Id.reciterContainer);
        _selectReciterLabel = FindViewById<TextView>(Resource.Id.selectReciterLabel);
        _reciterSpinner = FindViewById<Spinner>(Resource.Id.reciterSpinner);
        _useRemoteImagesCheckBox = FindViewById<CheckBox>(Resource.Id.useRemoteImagesCheckBox);
        _enableRecordingCheckBox = FindViewById<CheckBox>(Resource.Id.enableRecordingCheckBox);
        _showTajweedRulesCheckBox = FindViewById<CheckBox>(Resource.Id.showTajweedRulesCheckBox);
        _tajweedRuleSelectionCard = FindViewById<global::AndroidX.CardView.Widget.CardView>(Resource.Id.tajweedRuleSelectionCard);
        _tajweedRuleSelectionTitle = FindViewById<TextView>(Resource.Id.tajweedRuleSelectionTitle);
        _selectAllRulesButton = FindViewById<Button>(Resource.Id.selectAllRulesButton);
        _deselectAllRulesButton = FindViewById<Button>(Resource.Id.deselectAllRulesButton);
        _tajweedRuleCheckboxContainer = FindViewById<LinearLayout>(Resource.Id.tajweedRuleCheckboxContainer);
        _saveButton = FindViewById<Button>(Resource.Id.saveButton);
        _cancelButton = FindViewById<Button>(Resource.Id.cancelButton);
        
        // Find text views for dynamic language update
        _languageSettingsTitle = FindViewById<TextView>(Resource.Id.languageSettingsTitle);
        _selectLanguageLabel = FindViewById<TextView>(Resource.Id.selectLanguageLabel);
        _searchDomainTitle = FindViewById<TextView>(Resource.Id.searchDomainTitle);
        _searchInLabel = FindViewById<TextView>(Resource.Id.searchInLabel);
        _surahNumberLabel = FindViewById<TextView>(Resource.Id.surahNumberLabel);
        _fromSurahLabel = FindViewById<TextView>(Resource.Id.fromSurahLabel);
        _toSurahLabel = FindViewById<TextView>(Resource.Id.toSurahLabel);
        _onlineResourcesTitle = FindViewById<TextView>(Resource.Id.onlineResourcesTitle);
        _onlineResourcesNote = FindViewById<TextView>(Resource.Id.onlineResourcesNote);
        _displaySettingsTitle = FindViewById<TextView>(Resource.Id.displaySettingsTitle);
        _fontSizeLabel = FindViewById<TextView>(Resource.Id.fontSizeLabel);
        _fontSizeSpinner = FindViewById<Spinner>(Resource.Id.fontSizeSpinner);
        _quranTextLabel = FindViewById<TextView>(Resource.Id.quranTextLabel);
        _quranTextSpinner = FindViewById<Spinner>(Resource.Id.quranTextSpinner);

        // Setup language spinner
        if (_languageSpinner != null)
        {
            var languageNames = _availableLanguages.Select(l => l.Name).ToList();
            var languageAdapter = new ArrayAdapter<string>(this, global::Android.Resource.Layout.SimpleSpinnerItem, languageNames);
            languageAdapter.SetDropDownViewResource(global::Android.Resource.Layout.SimpleSpinnerDropDownItem);
            _languageSpinner.Adapter = languageAdapter;
        }

        // Setup search domain spinner
        if (_searchDomainSpinner != null && _localizationService != null)
        {
            var domainNames = _searchDomainOptions.Select(d => _localizationService[d.DisplayKey]).ToList();
            var domainAdapter = new ArrayAdapter<string>(this, global::Android.Resource.Layout.SimpleSpinnerItem, domainNames);
            domainAdapter.SetDropDownViewResource(global::Android.Resource.Layout.SimpleSpinnerDropDownItem);
            _searchDomainSpinner.Adapter = domainAdapter;
        }
        
        // Setup surah spinners with number and name
        SetupSurahSpinners();
        
        // Setup reciter spinner
        SetupReciterSpinner();
        
        // Setup font size spinner
        SetupFontSizeSpinner();
        
        // Setup Quran text spinner
        SetupQuranTextSpinner();
        
        // Setup tajweed rule checkboxes
        SetupTajweedRuleCheckboxes();
    }
    
    private void SetupQuranTextSpinner()
    {
        if (_quranTextSpinner == null || _localizationService == null) return;
        
        var quranTextNames = _quranTextOptions.Select(q => _localizationService[q.DisplayKey]).ToList();
        var adapter = new ArrayAdapter<string>(this, global::Android.Resource.Layout.SimpleSpinnerItem, quranTextNames);
        adapter.SetDropDownViewResource(global::Android.Resource.Layout.SimpleSpinnerDropDownItem);
        _quranTextSpinner.Adapter = adapter;
    }
    
    private void SetupFontSizeSpinner()
    {
        if (_fontSizeSpinner == null) return;
        
        var fontSizeStrings = _fontSizeOptions.Select(s => $"{s} pt").ToList();
        var adapter = new ArrayAdapter<string>(this, global::Android.Resource.Layout.SimpleSpinnerItem, fontSizeStrings);
        adapter.SetDropDownViewResource(global::Android.Resource.Layout.SimpleSpinnerDropDownItem);
        _fontSizeSpinner.Adapter = adapter;
    }
    
    private void SetupReciterSpinner()
    {
        if (_reciterSpinner == null) return;
        
        var reciterNames = _reciterOptions.Select(r => GetString(r.DisplayNameResId)).ToList();
        var adapter = new ArrayAdapter<string>(this, global::Android.Resource.Layout.SimpleSpinnerItem, reciterNames);
        adapter.SetDropDownViewResource(global::Android.Resource.Layout.SimpleSpinnerDropDownItem);
        _reciterSpinner.Adapter = adapter;
    }
    
    private void SetupSurahSpinners()
    {
        // Build surah display names list (number + Arabic name)
        _surahDisplayNames.Clear();
        for (int i = 1; i <= 114; i++)
        {
            _surahDisplayNames.Add($"{i}. {SurahInfo.GetSurahName(i)}");
        }
        
        // Setup single surah spinner
        if (_singleSurahSpinner != null)
        {
            var adapter = new ArrayAdapter<string>(this, global::Android.Resource.Layout.SimpleSpinnerItem, _surahDisplayNames);
            adapter.SetDropDownViewResource(global::Android.Resource.Layout.SimpleSpinnerDropDownItem);
            _singleSurahSpinner.Adapter = adapter;
        }
        
        // Setup start surah spinner
        if (_startSurahSpinner != null)
        {
            var adapter = new ArrayAdapter<string>(this, global::Android.Resource.Layout.SimpleSpinnerItem, _surahDisplayNames);
            adapter.SetDropDownViewResource(global::Android.Resource.Layout.SimpleSpinnerDropDownItem);
            _startSurahSpinner.Adapter = adapter;
        }
        
        // Setup end surah spinner
        if (_endSurahSpinner != null)
        {
            var adapter = new ArrayAdapter<string>(this, global::Android.Resource.Layout.SimpleSpinnerItem, _surahDisplayNames);
            adapter.SetDropDownViewResource(global::Android.Resource.Layout.SimpleSpinnerDropDownItem);
            _endSurahSpinner.Adapter = adapter;
        }
    }
    
    private void SetupTajweedRuleCheckboxes()
    {
        if (_tajweedRuleCheckboxContainer == null) return;
        
        _ruleCheckboxes.Clear();
        _tajweedRuleCheckboxContainer.RemoveAllViews();
        
        try
        {
            var fileService = new Services.AndroidFileService(this);
            var searchService = new QuranSearchService(fileService);
            // Load rules synchronously for settings page
            searchService.LoadRulesAsync().GetAwaiter().GetResult();
            var rules = searchService.GetRules();
            
            var languageCode = _localizationService?.CurrentLanguage ?? "ar";
            string? lastGroup = null;
            
            foreach (var rule in rules)
            {
                // Add group header if group changed
                if (rule.Group != lastGroup)
                {
                    lastGroup = rule.Group;
                    var groupHeader = new TextView(this);
                    var groupDisplayName = GetGroupDisplayName(rule.Group ?? "");
                    groupHeader.Text = groupDisplayName;
                    groupHeader.SetTextSize(global::Android.Util.ComplexUnitType.Sp, 13);
                    groupHeader.SetTypeface(null, global::Android.Graphics.TypefaceStyle.Bold);
                    groupHeader.SetTextColor(global::Android.Graphics.Color.ParseColor("#2C5F2D"));
                    groupHeader.SetPadding(0, 12, 0, 4);
                    _tajweedRuleCheckboxContainer.AddView(groupHeader);
                }
                
                var checkbox = new CheckBox(this);
                var localizedName = RuleNameTranslator.GetLocalizedName(rule.Name, languageCode);
                checkbox.Text = localizedName;
                checkbox.SetTextSize(global::Android.Util.ComplexUnitType.Sp, 13);
                checkbox.Checked = true; // Default all selected
                checkbox.SetPadding(0, 0, 0, 0);
                checkbox.CheckedChange += (s, e) => { if (_initialLoadComplete) _hasUnsavedChanges = true; };
                
                _tajweedRuleCheckboxContainer.AddView(checkbox);
                _ruleCheckboxes.Add((checkbox, rule.Name));
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error setting up tajweed rule checkboxes: {ex.Message}");
        }
    }
    
    private string GetGroupDisplayName(string group)
    {
        return group switch
        {
            "lam" => "━━ Lam / اللام ━━",
            "noon_tanween" => "━━ Noon & Tanween / النون والتنوين ━━",
            "meem_sakinah" => "━━ Meem Sakinah / الميم الساكنة ━━",
            "noon_meem_mushaddad" => "━━ Noon & Meem Mushaddad / النون والميم المشددتين ━━",
            "qalqalah" => "━━ Qalqalah / القلقلة ━━",
            "mad" => "━━ Madd / المد ━━",
            "waqf" => "━━ Waqf / الوقف ━━",
            "tafkhim_tarqiq" => "━━ Tafkhim & Tarqiq / التفخيم والترقيق ━━",
            _ => $"━━ {group} ━━"
        };
    }
    
    private void UpdateTajweedRuleCardVisibility(bool showTajweed)
    {
        if (_tajweedRuleSelectionCard != null)
        {
            _tajweedRuleSelectionCard.Visibility = showTajweed ? ViewStates.Visible : ViewStates.Gone;
        }
    }

    private void LoadCurrentSettings()
    {
        var prefs = GetSharedPreferences("QuranSearchSettings", FileCreationMode.Private);
        
        // Load language with system language detection
        var currentLanguage = LanguageHelper.GetLanguageFromPreferences(this);
        var languageIndex = _availableLanguages.FindIndex(l => l.Code == currentLanguage);
        if (languageIndex >= 0 && _languageSpinner != null)
        {
            _languageSpinner.SetSelection(languageIndex);
        }

        // Load search domain
        var searchDomainType = prefs?.GetInt("SearchDomainType", 0) ?? 0;
        if (_searchDomainSpinner != null)
        {
            _searchDomainSpinner.SetSelection(searchDomainType);
        }

        // Load surah numbers (spinner index is 0-based, surah number is 1-based)
        var startSurah = prefs?.GetInt("StartSurah", 1) ?? 1;
        var endSurah = prefs?.GetInt("EndSurah", 114) ?? 114;
        
        if (_singleSurahSpinner != null)
        {
            _singleSurahSpinner.SetSelection(startSurah - 1);
        }
        if (_startSurahSpinner != null)
        {
            _startSurahSpinner.SetSelection(startSurah - 1);
        }
        if (_endSurahSpinner != null)
        {
            _endSurahSpinner.SetSelection(endSurah - 1);
        }

        // Load online resource settings
        var useRemoteAudio = prefs?.GetBoolean("UseRemoteAudio", true) ?? true;
        var useRemoteImages = prefs?.GetBoolean("UseRemoteImages", false) ?? false;
        var selectedReciter = prefs?.GetString("SelectedReciter", "Husary_128kbps") ?? "Husary_128kbps";
        
        if (_useRemoteAudioCheckBox != null)
        {
            _useRemoteAudioCheckBox.Checked = useRemoteAudio;
        }
        if (_useRemoteImagesCheckBox != null)
        {
            _useRemoteImagesCheckBox.Checked = useRemoteImages;
        }
        
        // Load recording enabled setting
        var recordingEnabled = prefs?.GetBoolean("RecordingEnabled", true) ?? true;
        if (_enableRecordingCheckBox != null)
        {
            _enableRecordingCheckBox.Checked = recordingEnabled;
        }
        
        // Load show Tajweed rules setting
        var showTajweedRules = prefs?.GetBoolean("ShowTajweedRules", true) ?? true;
        if (_showTajweedRulesCheckBox != null)
        {
            _showTajweedRulesCheckBox.Checked = showTajweedRules;
        }
        
        // Load reciter selection
        if (_reciterSpinner != null)
        {
            var reciterIndex = _reciterOptions.FindIndex(r => r.FolderName == selectedReciter);
            if (reciterIndex >= 0)
            {
                _reciterSpinner.SetSelection(reciterIndex);
            }
        }
        
        // Load font size setting
        var fontSize = prefs?.GetInt("FontSize", 18) ?? 18;
        if (_fontSizeSpinner != null)
        {
            var fontSizeIndex = _fontSizeOptions.IndexOf(fontSize);
            if (fontSizeIndex >= 0)
            {
                _fontSizeSpinner.SetSelection(fontSizeIndex);
            }
            else
            {
                // Default to 18pt (index 3)
                _fontSizeSpinner.SetSelection(3);
            }
        }
        
        // Load Quran text file setting
        var quranTextFile = prefs?.GetString("QuranTextFile", "quran-uthmani-ver1.2.txt") ?? "quran-uthmani-ver1.2.txt";
        if (_quranTextSpinner != null)
        {
            var quranTextIndex = _quranTextOptions.FindIndex(q => q.FileName == quranTextFile);
            if (quranTextIndex >= 0)
            {
                _quranTextSpinner.SetSelection(quranTextIndex);
            }
        }
        
        // Update reciter container visibility based on remote audio setting
        UpdateReciterVisibility(useRemoteAudio);

        // Update visibility based on search domain
        UpdateSearchDomainVisibility(searchDomainType);
        
        // Load selected tajweed rules and update card visibility
        var savedRules = prefs?.GetStringSet("SelectedTajweedRules", null);
        if (savedRules != null)
        {
            foreach (var (checkbox, ruleName) in _ruleCheckboxes)
            {
                checkbox.Checked = savedRules.Contains(ruleName);
            }
        }
        else
        {
            // Default: all selected
            foreach (var (checkbox, _) in _ruleCheckboxes)
            {
                checkbox.Checked = true;
            }
        }
        UpdateTajweedRuleCardVisibility(showTajweedRules);
    }

    private void SetupEventHandlers()
    {
        if (_languageSpinner != null)
        {
            _languageSpinner.ItemSelected += OnLanguageSelected;
        }

        if (_searchDomainSpinner != null)
        {
            _searchDomainSpinner.ItemSelected += (s, e) =>
            {
                UpdateSearchDomainVisibility(e.Position);
                if (_initialLoadComplete) _hasUnsavedChanges = true;
            };
        }
        
        if (_useRemoteAudioCheckBox != null)
        {
            _useRemoteAudioCheckBox.CheckedChange += (s, e) =>
            {
                UpdateReciterVisibility(e.IsChecked);
                if (_initialLoadComplete) _hasUnsavedChanges = true;
            };
        }
        
        if (_useRemoteImagesCheckBox != null)
            _useRemoteImagesCheckBox.CheckedChange += (s, e) => { if (_initialLoadComplete) _hasUnsavedChanges = true; };
        if (_enableRecordingCheckBox != null)
            _enableRecordingCheckBox.CheckedChange += (s, e) => { if (_initialLoadComplete) _hasUnsavedChanges = true; };
        if (_showTajweedRulesCheckBox != null)
            _showTajweedRulesCheckBox.CheckedChange += (s, e) => 
            { 
                if (_initialLoadComplete) _hasUnsavedChanges = true;
                UpdateTajweedRuleCardVisibility(e.IsChecked);
            };
        if (_selectAllRulesButton != null)
            _selectAllRulesButton.Click += (s, e) => 
            {
                foreach (var (checkbox, _) in _ruleCheckboxes) checkbox.Checked = true;
                if (_initialLoadComplete) _hasUnsavedChanges = true;
            };
        if (_deselectAllRulesButton != null)
            _deselectAllRulesButton.Click += (s, e) => 
            {
                foreach (var (checkbox, _) in _ruleCheckboxes) checkbox.Checked = false;
                if (_initialLoadComplete) _hasUnsavedChanges = true;
            };
        if (_singleSurahSpinner != null)
            _singleSurahSpinner.ItemSelected += (s, e) => { if (_initialLoadComplete) _hasUnsavedChanges = true; };
        if (_startSurahSpinner != null)
            _startSurahSpinner.ItemSelected += (s, e) => { if (_initialLoadComplete) _hasUnsavedChanges = true; };
        if (_endSurahSpinner != null)
            _endSurahSpinner.ItemSelected += (s, e) => { if (_initialLoadComplete) _hasUnsavedChanges = true; };
        if (_reciterSpinner != null)
            _reciterSpinner.ItemSelected += (s, e) => { if (_initialLoadComplete) _hasUnsavedChanges = true; };
        if (_fontSizeSpinner != null)
            _fontSizeSpinner.ItemSelected += (s, e) => { if (_initialLoadComplete) _hasUnsavedChanges = true; };
        if (_quranTextSpinner != null)
            _quranTextSpinner.ItemSelected += (s, e) => { if (_initialLoadComplete) _hasUnsavedChanges = true; };

        if (_saveButton != null)
        {
            _saveButton.Click += SaveButton_Click;
        }

        if (_cancelButton != null)
        {
            _cancelButton.Click += (s, e) => Finish();
        }
    }

    private void OnLanguageSelected(object? sender, AdapterView.ItemSelectedEventArgs e)
    {
        if (e.Position >= 0 && e.Position < _availableLanguages.Count && _localizationService != null)
        {
            var selectedLanguage = _availableLanguages[e.Position];
            _localizationService.CurrentLanguage = selectedLanguage.Code;
            
            if (_initialLoadComplete) _hasUnsavedChanges = true;
            
            // Reload the UI with the new language
            RefreshUIForLanguageChange();
        }
    }

    private void RefreshUIForLanguageChange()
    {
        if (_localizationService == null) return;
        
        // Update layout direction based on language (RTL for Arabic)
        var isRtl = _localizationService.IsRightToLeft;
        var layoutDirection = isRtl ? LayoutDirection.Rtl : LayoutDirection.Ltr;
        
        // Set layout direction on the scroll view and root layout
        var scrollView = FindViewById<ScrollView>(Resource.Id.settingsScrollView);
        var rootLayout = FindViewById<LinearLayout>(Resource.Id.settingsRootLayout);
        
        if (scrollView != null)
        {
            scrollView.LayoutDirection = layoutDirection;
        }
        if (rootLayout != null)
        {
            rootLayout.LayoutDirection = layoutDirection;
        }
        
        // Also set on Window if available
        if (Window?.DecorView != null)
        {
            Window.DecorView.LayoutDirection = layoutDirection;
        }
        
        // Update all text views with new language strings
        if (_languageSettingsTitle != null)
            _languageSettingsTitle.Text = _localizationService["LanguageSettingsTitle"];
        if (_selectLanguageLabel != null)
            _selectLanguageLabel.Text = _localizationService["SelectLanguageLabel"];
        if (_searchDomainTitle != null)
            _searchDomainTitle.Text = _localizationService["SearchDomainTitle"];
        if (_searchInLabel != null)
            _searchInLabel.Text = _localizationService["SearchInLabel"];
        if (_surahNumberLabel != null)
            _surahNumberLabel.Text = _localizationService["SurahNumberLabel"];
        if (_fromSurahLabel != null)
            _fromSurahLabel.Text = _localizationService["FromSurahLabel"];
        if (_toSurahLabel != null)
            _toSurahLabel.Text = _localizationService["ToSurahLabel"];
        if (_onlineResourcesTitle != null)
            _onlineResourcesTitle.Text = _localizationService["OnlineResourcesTitle"];
        if (_onlineResourcesNote != null)
            _onlineResourcesNote.Text = _localizationService["OnlineResourcesNoteText"];
        if (_displaySettingsTitle != null)
            _displaySettingsTitle.Text = _localizationService["DisplaySettingsTitle"];
        if (_fontSizeLabel != null)
            _fontSizeLabel.Text = _localizationService["FontSizeLabel"];
        if (_quranTextLabel != null)
            _quranTextLabel.Text = _localizationService["QuranTextLabel"];
        
        // Update Quran text spinner with localized names
        SetupQuranTextSpinner();
        
        // Update checkboxes
        if (_useRemoteAudioCheckBox != null)
            _useRemoteAudioCheckBox.Text = _localizationService["GetAudioFromInternetLabel"];
        if (_useRemoteImagesCheckBox != null)
            _useRemoteImagesCheckBox.Text = _localizationService["GetImagesFromInternetLabel"];
        
        // Update reciter label
        if (_selectReciterLabel != null)
            _selectReciterLabel.Text = GetString(Resource.String.select_reciter_label);
        
        // Update reciter spinner with localized names
        SetupReciterSpinner();
        
        // Restore reciter selection after refreshing spinner
        var prefs = GetSharedPreferences("QuranSearchSettings", FileCreationMode.Private);
        var selectedReciter = prefs?.GetString("SelectedReciter", "Husary_128kbps") ?? "Husary_128kbps";
        if (_reciterSpinner != null)
        {
            var reciterIndex = _reciterOptions.FindIndex(r => r.FolderName == selectedReciter);
            if (reciterIndex >= 0)
            {
                _reciterSpinner.SetSelection(reciterIndex);
            }
        }
        
        // Update buttons
        if (_saveButton != null)
            _saveButton.Text = _localizationService["SaveSettingsButton"];
        if (_cancelButton != null)
            _cancelButton.Text = _localizationService["CloseButton"];
        
        // Update search domain spinner with new language
        if (_searchDomainSpinner != null)
        {
            var currentSelection = _searchDomainSpinner.SelectedItemPosition;
            var domainNames = _searchDomainOptions.Select(d => _localizationService[d.DisplayKey]).ToList();
            var domainAdapter = new ArrayAdapter<string>(this, global::Android.Resource.Layout.SimpleSpinnerItem, domainNames);
            domainAdapter.SetDropDownViewResource(global::Android.Resource.Layout.SimpleSpinnerDropDownItem);
            _searchDomainSpinner.Adapter = domainAdapter;
            _searchDomainSpinner.SetSelection(currentSelection);
        }
    }

    private void UpdateSearchDomainVisibility(int domainType)
    {
        if (_singleSurahContainer == null || _surahRangeContainer == null)
            return;

        _singleSurahContainer.Visibility = domainType == 1 ? ViewStates.Visible : ViewStates.Gone;
        _surahRangeContainer.Visibility = domainType == 2 ? ViewStates.Visible : ViewStates.Gone;
    }
    
    private void UpdateReciterVisibility(bool useRemoteAudio)
    {
        if (_reciterContainer == null) return;
        _reciterContainer.Visibility = useRemoteAudio ? ViewStates.Visible : ViewStates.Gone;
    }

    private void SaveButton_Click(object? sender, EventArgs e)
    {
        try
        {
            // Get surah numbers from spinners (spinner index is 0-based, surah number is 1-based)
            int startSurah = 1;
            int endSurah = 114;

            var searchDomainType = _searchDomainSpinner?.SelectedItemPosition ?? 0;

            if (searchDomainType == 1) // Single Surah
            {
                startSurah = (_singleSurahSpinner?.SelectedItemPosition ?? 0) + 1;
                endSurah = startSurah;
            }
            else if (searchDomainType == 2) // Surah Range
            {
                startSurah = (_startSurahSpinner?.SelectedItemPosition ?? 0) + 1;
                endSurah = (_endSurahSpinner?.SelectedItemPosition ?? 113) + 1;
                
                if (startSurah > endSurah)
                {
                    ShowError(_localizationService?["StartSurahMustBeLessThanEnd"] ?? "Start Surah must be less than or equal to End Surah");
                    return;
                }
            }

            // Save settings
            var prefs = GetSharedPreferences("QuranSearchSettings", FileCreationMode.Private);
            var editor = prefs?.Edit();

            if (editor != null)
            {
                // Save language
                var selectedLanguageIndex = _languageSpinner?.SelectedItemPosition ?? 0;
                var selectedLanguage = _availableLanguages[selectedLanguageIndex];
                editor.PutString("Language", selectedLanguage.Code);

                // Save search domain
                editor.PutInt("SearchDomainType", searchDomainType);
                editor.PutInt("StartSurah", startSurah);
                editor.PutInt("EndSurah", endSurah);

                // Save online resource settings
                editor.PutBoolean("UseRemoteAudio", _useRemoteAudioCheckBox?.Checked ?? true);
                editor.PutBoolean("UseRemoteImages", _useRemoteImagesCheckBox?.Checked ?? false);
                editor.PutBoolean("RecordingEnabled", _enableRecordingCheckBox?.Checked ?? true);
                editor.PutBoolean("ShowTajweedRules", _showTajweedRulesCheckBox?.Checked ?? true);
                
                // Save selected tajweed rules
                var selectedRules = new HashSet<string>();
                foreach (var (checkbox, ruleName) in _ruleCheckboxes)
                {
                    if (checkbox.Checked)
                        selectedRules.Add(ruleName);
                }
                editor.PutStringSet("SelectedTajweedRules", selectedRules as ICollection<string>);
                
                // Save reciter selection
                var selectedReciterIndex = _reciterSpinner?.SelectedItemPosition ?? 2; // Default to Husary
                if (selectedReciterIndex >= 0 && selectedReciterIndex < _reciterOptions.Count)
                {
                    editor.PutString("SelectedReciter", _reciterOptions[selectedReciterIndex].FolderName);
                }
                
                // Save font size
                var selectedFontSizeIndex = _fontSizeSpinner?.SelectedItemPosition ?? 3; // Default to 18pt
                if (selectedFontSizeIndex >= 0 && selectedFontSizeIndex < _fontSizeOptions.Count)
                {
                    editor.PutInt("FontSize", _fontSizeOptions[selectedFontSizeIndex]);
                }
                
                // Save Quran text file
                var selectedQuranTextIndex = _quranTextSpinner?.SelectedItemPosition ?? 0; // Default to clean version
                if (selectedQuranTextIndex >= 0 && selectedQuranTextIndex < _quranTextOptions.Count)
                {
                    editor.PutString("QuranTextFile", _quranTextOptions[selectedQuranTextIndex].FileName);
                }

                editor.Apply();
            }

            // Return result to MainActivity
            var resultIntent = new Intent();
            resultIntent.PutExtra("SettingsChanged", true);
            SetResult(Result.Ok, resultIntent);
            Finish();
        }
        catch (Exception ex)
        {
            ShowError($"Error saving settings: {ex.Message}");
        }
    }

    private void ShowError(string message)
    {
        Toast.MakeText(this, message, ToastLength.Long)?.Show();
    }

    public override bool OnOptionsItemSelected(IMenuItem item)
    {
        if (item.ItemId == global::Android.Resource.Id.Home)
        {
            HandleBackNavigation();
            return true;
        }
        return base.OnOptionsItemSelected(item);
    }
    
    public override void OnBackPressed()
    {
        HandleBackNavigation();
    }
    
    private void HandleBackNavigation()
    {
        if (_hasUnsavedChanges)
        {
            var saveText = _localizationService?["SaveSettingsButton"] ?? "Save";
            var discardText = _localizationService?["DiscardChanges"] ?? "Discard";
            var messageText = _localizationService?["UnsavedChangesMessage"] ?? "You have unsaved changes. Would you like to save before leaving?";
            var titleText = _localizationService?["UnsavedChangesTitle"] ?? "Unsaved Changes";
            
            new global::AndroidX.AppCompat.App.AlertDialog.Builder(this)
                .SetTitle(titleText)
                .SetMessage(messageText)
                .SetPositiveButton(saveText, (s, e) => { SaveButton_Click(null, EventArgs.Empty); })
                .SetNegativeButton(discardText, (s, e) => { Finish(); })
                .SetCancelable(true)
                .Show();
        }
        else
        {
            Finish();
        }
    }
}

public class SearchDomainOption
{
    public SearchDomainType Type { get; set; }
    public string DisplayKey { get; set; } = string.Empty;
}

public enum SearchDomainType
{
    WholeQuran = 0,
    SingleSurah = 1,
    SurahRange = 2
}

public class ReciterOption
{
    public string FolderName { get; set; } = string.Empty;
    public int DisplayNameResId { get; set; }
}

public class QuranTextOption
{
    public string FileName { get; set; } = string.Empty;
    public string DisplayKey { get; set; } = string.Empty;
}
