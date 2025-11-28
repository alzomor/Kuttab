using System;
using System.Collections.Generic;
using System.Text.Json;
using ReactiveUI;
using QuranSearch.Core.Interfaces;

namespace QuranSearch.Core.Services;

public class LocalizationService : ReactiveObject
{
    private readonly IFileService _fileService;
    private Dictionary<string, string> _currentStrings = new();
    private string _currentLanguage = "en"; // Default to English

    public LocalizationService(IFileService fileService)
    {
        _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        LoadLanguage(_currentLanguage);
    }

    public string CurrentLanguage
    {
        get => _currentLanguage;
        set
        {
            if (_currentLanguage != value)
            {
                _currentLanguage = value;
                LoadLanguage(value);
                this.RaisePropertyChanged(nameof(CurrentLanguage));
                OnLanguageChanged();
            }
        }
    }

    public event EventHandler? LanguageChanged;

    private void OnLanguageChanged()
    {
        LanguageChanged?.Invoke(this, EventArgs.Empty);
        
        // Raise property changed for all string properties to update bindings
        foreach (var key in _currentStrings.Keys)
        {
            this.RaisePropertyChanged(key);
        }
        
        // Notify that RTL status may have changed
        this.RaisePropertyChanged(nameof(IsRightToLeft));
    }

    private void LoadLanguage(string languageCode)
    {
        try
        {
            var filePath = System.IO.Path.Combine("Localization", $"Strings.{languageCode}.json");
            
            if (!_fileService.FileExists(filePath))
            {
                Console.WriteLine($"Language file not found: {filePath}");
                return;
            }

            var jsonContent = _fileService.ReadAllText(filePath); // Use synchronous version to avoid deadlock
            _currentStrings = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent) ?? new Dictionary<string, string>();
            
            Console.WriteLine($"Loaded language: {languageCode} with {_currentStrings.Count} strings");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading language file: {ex.Message}");
            _currentStrings = new Dictionary<string, string>();
        }
    }

    public string this[string key]
    {
        get
        {
            if (_currentStrings.TryGetValue(key, out var value))
            {
                return value;
            }
            return $"[{key}]"; // Return key in brackets if not found
        }
    }

    public string GetString(string key, params object[] args)
    {
        var template = this[key];
        
        if (args.Length > 0)
        {
            try
            {
                return string.Format(template, args);
            }
            catch
            {
                return template;
            }
        }
        
        return template;
    }

    // Convenience properties for common strings (enables XAML binding)
    public string WindowTitle => this["WindowTitle"];
    public string SelectTajweedRule => this["SelectTajweedRule"];
    public string Search => this["Search"];
    public string OnlineImages => this["OnlineImages"];
    public string OnlineAudio => this["OnlineAudio"];
    public string Ready => this["Ready"];
    public string Surah => this["Surah"];
    public string Aya => this["Aya"];
    public string AudioControls => this["AudioControls"];
    public string PlaySingle => this["PlaySingle"];
    public string PlayRepeat => this["PlayRepeat"];
    public string PlayAllSequence => this["PlayAllSequence"];
    public string PauseSequence => this["PauseSequence"];
    public string ResumeSequence => this["ResumeSequence"];
    public string Stop => this["Stop"];
    public string Playing => this["Playing"];
    public string Repeat => this["Repeat"];
    public string SequencePaused => this["SequencePaused"];
    public string QuranicPageView => this["QuranicPageView"];
    public string SelectAyaToViewImage => this["SelectAyaToViewImage"];
    public string Language => this["Language"];
    public string Arabic => this["Arabic"];
    public string English => this["English"];
    public string German => this["German"];
    public string Matched => this["Matched"];
    
    // Settings window properties
    public string SettingsTitle => this["SettingsTitle"];
    public string LanguageSettingsTitle => this["LanguageSettingsTitle"];
    public string SelectLanguageLabel => this["SelectLanguageLabel"];
    public string SearchDomainTitle => this["SearchDomainTitle"];
    public string SearchInLabel => this["SearchInLabel"];
    public string SurahNumberLabel => this["SurahNumberLabel"];
    public string FromSurahLabel => this["FromSurahLabel"];
    public string ToSurahLabel => this["ToSurahLabel"];
    public string OnlineResourcesTitle => this["OnlineResourcesTitle"];
    public string GetAudioFromInternetLabel => this["GetAudioFromInternetLabel"];
    public string GetImagesFromInternetLabel => this["GetImagesFromInternetLabel"];
    public string OnlineResourcesNoteText => this["OnlineResourcesNoteText"];
    public string SelectReciterLabel => this["SelectReciterLabel"];
    public string SaveSettingsButton => this["SaveSettingsButton"];
    public string CancelButton => this["CancelButton"];

    public bool IsRightToLeft => _currentLanguage == "ar";

    public List<LanguageOption> AvailableLanguages => new()
    {
        new LanguageOption { Code = "ar", Name = "العربية", IsRightToLeft = true },
        new LanguageOption { Code = "en", Name = "English", IsRightToLeft = false },
        new LanguageOption { Code = "de", Name = "Deutsch", IsRightToLeft = false }
    };
}

public class LanguageOption
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsRightToLeft { get; set; }
    
    public override string ToString() => Name;
}
