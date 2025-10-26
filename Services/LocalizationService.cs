using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using ReactiveUI;

namespace QuranSearchApp.Services;

public class LocalizationService : ReactiveObject
{
    private static LocalizationService? _instance;
    private Dictionary<string, string> _currentStrings = new();
    private string _currentLanguage = "ar"; // Default to Arabic

    public static LocalizationService Instance => _instance ??= new LocalizationService();

    private LocalizationService()
    {
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
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Localization", $"Strings.{languageCode}.json");
            
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Language file not found: {filePath}");
                return;
            }

            var jsonContent = File.ReadAllText(filePath);
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
}
