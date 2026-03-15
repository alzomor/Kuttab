using System;
using System.Collections.Generic;
using System.Text.Json;
using ReactiveUI;
using Kuttab.Core.Interfaces;

namespace Kuttab.Core.Services;

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
        
        // Raise property changed for all convenience properties (for XAML bindings)
        RaiseAllConveniencePropertiesChanged();
    }
    
    private void RaiseAllConveniencePropertiesChanged()
    {
        // Main window properties
        this.RaisePropertyChanged(nameof(WindowTitle));
        this.RaisePropertyChanged(nameof(SelectTajweedRule));
        this.RaisePropertyChanged(nameof(Search));
        this.RaisePropertyChanged(nameof(OnlineImages));
        this.RaisePropertyChanged(nameof(OnlineAudio));
        this.RaisePropertyChanged(nameof(Ready));
        this.RaisePropertyChanged(nameof(Surah));
        this.RaisePropertyChanged(nameof(Aya));
        this.RaisePropertyChanged(nameof(AudioControls));
        this.RaisePropertyChanged(nameof(PlaySingle));
        this.RaisePropertyChanged(nameof(PlayRepeat));
        this.RaisePropertyChanged(nameof(PlayAllSequence));
        this.RaisePropertyChanged(nameof(PauseSequence));
        this.RaisePropertyChanged(nameof(ResumeSequence));
        this.RaisePropertyChanged(nameof(Stop));
        this.RaisePropertyChanged(nameof(Playing));
        this.RaisePropertyChanged(nameof(Repeat));
        this.RaisePropertyChanged(nameof(SequencePaused));
        this.RaisePropertyChanged(nameof(QuranicPageView));
        this.RaisePropertyChanged(nameof(SelectAyaToViewImage));
        this.RaisePropertyChanged(nameof(Language));
        this.RaisePropertyChanged(nameof(Arabic));
        this.RaisePropertyChanged(nameof(English));
        this.RaisePropertyChanged(nameof(German));
        this.RaisePropertyChanged(nameof(Matched));
        
        // Settings window properties
        this.RaisePropertyChanged(nameof(SettingsTitle));
        this.RaisePropertyChanged(nameof(LanguageSettingsTitle));
        this.RaisePropertyChanged(nameof(SelectLanguageLabel));
        this.RaisePropertyChanged(nameof(SearchDomainTitle));
        this.RaisePropertyChanged(nameof(SearchInLabel));
        this.RaisePropertyChanged(nameof(SurahNumberLabel));
        this.RaisePropertyChanged(nameof(FromSurahLabel));
        this.RaisePropertyChanged(nameof(ToSurahLabel));
        this.RaisePropertyChanged(nameof(OnlineResourcesTitle));
        this.RaisePropertyChanged(nameof(GetAudioFromInternetLabel));
        this.RaisePropertyChanged(nameof(GetImagesFromInternetLabel));
        this.RaisePropertyChanged(nameof(OnlineResourcesNoteText));
        this.RaisePropertyChanged(nameof(SelectReciterLabel));
        this.RaisePropertyChanged(nameof(SaveSettingsButton));
        this.RaisePropertyChanged(nameof(CancelButton));
        
        // Recitation mode properties
        this.RaisePropertyChanged(nameof(OpenRecitation));
        this.RaisePropertyChanged(nameof(RecitationMode));
        this.RaisePropertyChanged(nameof(SelectReciter));
        this.RaisePropertyChanged(nameof(SelectSurah));
        this.RaisePropertyChanged(nameof(FromAya));
        this.RaisePropertyChanged(nameof(ToAya));
        this.RaisePropertyChanged(nameof(StartRecitation));
        this.RaisePropertyChanged(nameof(StopRecitation));
        this.RaisePropertyChanged(nameof(PreviousAya));
        this.RaisePropertyChanged(nameof(NextAya));
        this.RaisePropertyChanged(nameof(NowPlaying));
        this.RaisePropertyChanged(nameof(Stopped));
        this.RaisePropertyChanged(nameof(RecitationComplete));
        this.RaisePropertyChanged(nameof(Basmalah));
        
        // Info window properties
        this.RaisePropertyChanged(nameof(OpenInfo));
        this.RaisePropertyChanged(nameof(InfoTitle));
        this.RaisePropertyChanged(nameof(AppSubtitle));
        this.RaisePropertyChanged(nameof(ResourcesCreditsTitle));
        this.RaisePropertyChanged(nameof(AudioImagesSource));
        this.RaisePropertyChanged(nameof(EveryAyahDescription));
        this.RaisePropertyChanged(nameof(QuranTextSource));
        this.RaisePropertyChanged(nameof(TanzilDescription));
        this.RaisePropertyChanged(nameof(DevelopmentToolsTitle));
        this.RaisePropertyChanged(nameof(TajweedReviewTitle));
        this.RaisePropertyChanged(nameof(UsulAiDescription));
        this.RaisePropertyChanged(nameof(CodingAssistantTitle));
        this.RaisePropertyChanged(nameof(WindsurfDescription));
        this.RaisePropertyChanged(nameof(AcknowledgmentText));
        this.RaisePropertyChanged(nameof(CloseButton));
        
        // Donate window properties
        this.RaisePropertyChanged(nameof(OpenDonate));
        this.RaisePropertyChanged(nameof(DonateTitle));
        this.RaisePropertyChanged(nameof(DonateSubtitle));
        this.RaisePropertyChanged(nameof(DonateViaPayPal));
        this.RaisePropertyChanged(nameof(PayPalDescription));
        this.RaisePropertyChanged(nameof(DonateViaBankTransfer));
        this.RaisePropertyChanged(nameof(AccountHolder));
        this.RaisePropertyChanged(nameof(PaymentReference));
        this.RaisePropertyChanged(nameof(CopyButton));
        this.RaisePropertyChanged(nameof(BankTransferNote));
        this.RaisePropertyChanged(nameof(AboutAssociation));
        this.RaisePropertyChanged(nameof(AssociationDescription));
        this.RaisePropertyChanged(nameof(BarakahMessage));
        this.RaisePropertyChanged(nameof(TaxDeductionNote));
        
        // Tajweed Rule window properties
        this.RaisePropertyChanged(nameof(TajweedRuleTitle));
        this.RaisePropertyChanged(nameof(RuleNameLabel));
        this.RaisePropertyChanged(nameof(ExplanationLabel));
        this.RaisePropertyChanged(nameof(RuleButton));
        this.RaisePropertyChanged(nameof(NoRuleSelected));
        this.RaisePropertyChanged(nameof(NoExplanationAvailable));
        this.RaisePropertyChanged(nameof(ShowTajweedRules));
        
        // Share properties
        this.RaisePropertyChanged(nameof(ShareButton));
        this.RaisePropertyChanged(nameof(ShareAya));
        this.RaisePropertyChanged(nameof(ShareAyaChooser));
        this.RaisePropertyChanged(nameof(ShareAppInfo));
        this.RaisePropertyChanged(nameof(ShareDonationInfo));
        this.RaisePropertyChanged(nameof(ShareAppInfoChooser));
        this.RaisePropertyChanged(nameof(ShareDonationChooser));
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
    
    // Recitation mode properties
    public string OpenRecitation => this["OpenRecitation"];
    public string RecitationMode => this["RecitationMode"];
    public string SelectReciter => this["SelectReciter"];
    public string SelectSurah => this["SelectSurah"];
    public string FromAya => this["FromAya"];
    public string ToAya => this["ToAya"];
    public string StartRecitation => this["StartRecitation"];
    public string StopRecitation => this["StopRecitation"];
    public string PreviousAya => this["PreviousAya"];
    public string NextAya => this["NextAya"];
    public string NowPlaying => this["NowPlaying"];
    public string Stopped => this["Stopped"];
    public string RecitationComplete => this["RecitationComplete"];
    public string Basmalah => this["Basmalah"];

    // Info window properties
    public string OpenInfo => this["OpenInfo"];
    public string InfoTitle => this["InfoTitle"];
    public string AppSubtitle => this["AppSubtitle"];
    public string ResourcesCreditsTitle => this["ResourcesCreditsTitle"];
    public string AudioImagesSource => this["AudioImagesSource"];
    public string EveryAyahDescription => this["EveryAyahDescription"];
    public string QuranTextSource => this["QuranTextSource"];
    public string TanzilDescription => this["TanzilDescription"];
    public string DevelopmentToolsTitle => this["DevelopmentToolsTitle"];
    public string TajweedReviewTitle => this["TajweedReviewTitle"];
    public string UsulAiDescription => this["UsulAiDescription"];
    public string CodingAssistantTitle => this["CodingAssistantTitle"];
    public string WindsurfDescription => this["WindsurfDescription"];
    public string AcknowledgmentText => this["AcknowledgmentText"];
    public string CloseButton => this["CloseButton"];
    public string ContactEmail => this["ContactEmail"];
    public string ContactEmailText => this["ContactEmailText"];

    // Donate window properties
    public string OpenDonate => this["OpenDonate"];
    public string DonateTitle => this["DonateTitle"];
    public string DonateSubtitle => this["DonateSubtitle"];
    public string DonateViaPayPal => this["DonateViaPayPal"];
    public string PayPalDescription => this["PayPalDescription"];
    public string DonateViaBankTransfer => this["DonateViaBankTransfer"];
    public string AccountHolder => this["AccountHolder"];
    public string PaymentReference => this["PaymentReference"];
    public string CopyButton => this["CopyButton"];
    public string BankTransferNote => this["BankTransferNote"];
    public string AboutAssociation => this["AboutAssociation"];
    public string AssociationDescription => this["AssociationDescription"];
    public string BarakahMessage => this["BarakahMessage"];
    public string TaxDeductionNote => this["TaxDeductionNote"];

    // Tajweed Rule window properties
    public string TajweedRuleTitle => this["TajweedRuleTitle"];
    public string RuleNameLabel => this["RuleNameLabel"];
    public string ExplanationLabel => this["ExplanationLabel"];
    public string RuleButton => this["RuleButton"];
    public string NoRuleSelected => this["NoRuleSelected"];
    public string NoExplanationAvailable => this["NoExplanationAvailable"];
    public string ShowTajweedRules => this["ShowTajweedRules"];
    
    // Share properties
    public string ShareButton => this["ShareButton"];
    public string ShareAya => this["ShareAya"];
    public string ShareAyaChooser => this["ShareAyaChooser"];
    public string ShareAppInfo => this["ShareAppInfo"];
    public string ShareDonationInfo => this["ShareDonationInfo"];
    public string ShareAppInfoChooser => this["ShareAppInfoChooser"];
    public string ShareDonationChooser => this["ShareDonationChooser"];

    public bool IsRightToLeft => _currentLanguage == "ar";

    public List<LanguageOption> AvailableLanguages => new()
    {
        new LanguageOption { Code = "ar", Name = "العربية", IsRightToLeft = true },
        new LanguageOption { Code = "en", Name = "English", IsRightToLeft = false },
        new LanguageOption { Code = "de", Name = "Deutsch", IsRightToLeft = false },
        new LanguageOption { Code = "es", Name = "Español", IsRightToLeft = false },
        new LanguageOption { Code = "tr", Name = "Türkçe", IsRightToLeft = false },
        new LanguageOption { Code = "fr", Name = "Français", IsRightToLeft = false },
        new LanguageOption { Code = "ja", Name = "日本語", IsRightToLeft = false }
    };
}

public class LanguageOption
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsRightToLeft { get; set; }
    
    public override string ToString() => Name;
}
