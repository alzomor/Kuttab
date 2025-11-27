using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using QuranSearch.Core.Models;
using QuranSearch.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QuranSearch.Android;

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
    private Button? _saveButton;
    private Button? _cancelButton;
    
    // Text views for dynamic language update
    private TextView? _languageSettingsTitle;
    private TextView? _selectLanguageLabel;
    private TextView? _searchDomainTitle;
    private TextView? _searchInLabel;
    private TextView? _surahNumberLabel;
    private TextView? _fromSurahLabel;
    private TextView? _toSurahLabel;
    private TextView? _onlineResourcesTitle;
    private TextView? _onlineResourcesNote;

    private LocalizationService? _localizationService;
    private List<LanguageOption> _availableLanguages = new();
    private List<SearchDomainOption> _searchDomainOptions = new();
    private List<string> _surahDisplayNames = new();

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
    }

    private void InitializeServices()
    {
        var fileService = new Services.AndroidFileService(this);
        _localizationService = new LocalizationService(fileService);
        _availableLanguages = _localizationService.AvailableLanguages;
        
        // Initialize search domain options
        _searchDomainOptions = new List<SearchDomainOption>
        {
            new SearchDomainOption { Type = SearchDomainType.WholeQuran, DisplayKey = "WholeQuran" },
            new SearchDomainOption { Type = SearchDomainType.SingleSurah, DisplayKey = "SingleSurah" },
            new SearchDomainOption { Type = SearchDomainType.SurahRange, DisplayKey = "SurahRange" }
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
        _useRemoteImagesCheckBox = FindViewById<CheckBox>(Resource.Id.useRemoteImagesCheckBox);
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

    private void LoadCurrentSettings()
    {
        var prefs = GetSharedPreferences("QuranSearchSettings", FileCreationMode.Private);
        
        // Load language
        var currentLanguage = prefs?.GetString("Language", "en") ?? "en";
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
        
        if (_useRemoteAudioCheckBox != null)
        {
            _useRemoteAudioCheckBox.Checked = useRemoteAudio;
        }
        if (_useRemoteImagesCheckBox != null)
        {
            _useRemoteImagesCheckBox.Checked = useRemoteImages;
        }

        // Update visibility based on search domain
        UpdateSearchDomainVisibility(searchDomainType);
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
            };
        }

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
        
        // Update checkboxes
        if (_useRemoteAudioCheckBox != null)
            _useRemoteAudioCheckBox.Text = _localizationService["GetAudioFromInternetLabel"];
        if (_useRemoteImagesCheckBox != null)
            _useRemoteImagesCheckBox.Text = _localizationService["GetImagesFromInternetLabel"];
        
        // Update buttons
        if (_saveButton != null)
            _saveButton.Text = _localizationService["SaveSettingsButton"];
        if (_cancelButton != null)
            _cancelButton.Text = _localizationService["CancelButton"];
        
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
            Finish();
            return true;
        }
        return base.OnOptionsItemSelected(item);
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
