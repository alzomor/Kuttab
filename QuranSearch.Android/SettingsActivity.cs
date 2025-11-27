using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
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
    private EditText? _singleSurahNumber;
    private EditText? _startSurahNumber;
    private EditText? _endSurahNumber;
    private CheckBox? _useRemoteAudioCheckBox;
    private CheckBox? _useRemoteImagesCheckBox;
    private Button? _saveButton;
    private Button? _cancelButton;

    private LocalizationService? _localizationService;
    private List<LanguageOption> _availableLanguages = new();
    private List<SearchDomainOption> _searchDomainOptions = new();

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
        _singleSurahNumber = FindViewById<EditText>(Resource.Id.singleSurahNumber);
        _startSurahNumber = FindViewById<EditText>(Resource.Id.startSurahNumber);
        _endSurahNumber = FindViewById<EditText>(Resource.Id.endSurahNumber);
        _useRemoteAudioCheckBox = FindViewById<CheckBox>(Resource.Id.useRemoteAudioCheckBox);
        _useRemoteImagesCheckBox = FindViewById<CheckBox>(Resource.Id.useRemoteImagesCheckBox);
        _saveButton = FindViewById<Button>(Resource.Id.saveButton);
        _cancelButton = FindViewById<Button>(Resource.Id.cancelButton);

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

        // Load surah numbers
        var startSurah = prefs?.GetInt("StartSurah", 1) ?? 1;
        var endSurah = prefs?.GetInt("EndSurah", 114) ?? 114;
        
        if (_singleSurahNumber != null)
        {
            _singleSurahNumber.Text = startSurah.ToString();
        }
        if (_startSurahNumber != null)
        {
            _startSurahNumber.Text = startSurah.ToString();
        }
        if (_endSurahNumber != null)
        {
            _endSurahNumber.Text = endSurah.ToString();
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
            // Validate surah numbers
            int startSurah = 1;
            int endSurah = 114;

            var searchDomainType = _searchDomainSpinner?.SelectedItemPosition ?? 0;

            if (searchDomainType == 1) // Single Surah
            {
                if (!int.TryParse(_singleSurahNumber?.Text, out startSurah) || startSurah < 1 || startSurah > 114)
                {
                    ShowError("Please enter a valid Surah number (1-114)");
                    return;
                }
                endSurah = startSurah;
            }
            else if (searchDomainType == 2) // Surah Range
            {
                if (!int.TryParse(_startSurahNumber?.Text, out startSurah) || startSurah < 1 || startSurah > 114)
                {
                    ShowError("Please enter a valid start Surah number (1-114)");
                    return;
                }
                if (!int.TryParse(_endSurahNumber?.Text, out endSurah) || endSurah < 1 || endSurah > 114)
                {
                    ShowError("Please enter a valid end Surah number (1-114)");
                    return;
                }
                if (startSurah > endSurah)
                {
                    ShowError("Start Surah must be less than or equal to End Surah");
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
