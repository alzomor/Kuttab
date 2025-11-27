using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using ReactiveUI;
using QuranSearch.Core.Services;
using QuranSearch.Core.ViewModels;

namespace QuranSearchApp.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    private readonly LocalizationService _localizationService;
    private LanguageOption _selectedLanguage;
    private SearchDomainOption _selectedSearchDomain;
    private bool _useRemoteAudio;
    private bool _useRemoteImages;
    private int _startSurah = 1;
    private int _endSurah = 114;

    public SettingsViewModel(LocalizationService localizationService, 
                            LanguageOption currentLanguage,
                            bool useRemoteAudio,
                            bool useRemoteImages,
                            SearchDomainOption? currentSearchDomain = null)
    {
        _localizationService = localizationService;
        _selectedLanguage = currentLanguage;
        _useRemoteAudio = useRemoteAudio;
        _useRemoteImages = useRemoteImages;
        _selectedSearchDomain = currentSearchDomain ?? SearchDomainOptions[0];
        
        SaveCommand = new SimpleCommand(OnSave);
        CancelCommand = new SimpleCommand(OnCancel);
        
        // Subscribe to language changes
        _localizationService.LanguageChanged += OnLanguageChanged;
    }

    public LocalizationService Localization => _localizationService;

    public List<LanguageOption> AvailableLanguages => _localizationService.AvailableLanguages;

    public LanguageOption SelectedLanguage
    {
        get => _selectedLanguage;
        set => this.RaiseAndSetIfChanged(ref _selectedLanguage, value);
    }

    public ObservableCollection<SearchDomainOption> SearchDomainOptions { get; } = new()
    {
        new SearchDomainOption { Type = SearchDomainType.WholeQuran, DisplayKey = "WholeQuran" },
        new SearchDomainOption { Type = SearchDomainType.SingleSurah, DisplayKey = "SingleSurah" },
        new SearchDomainOption { Type = SearchDomainType.SurahRange, DisplayKey = "SurahRange" }
    };

    public SearchDomainOption SelectedSearchDomain
    {
        get => _selectedSearchDomain;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedSearchDomain, value);
            this.RaisePropertyChanged(nameof(IsSingleSurahSelected));
            this.RaisePropertyChanged(nameof(IsSurahRangeSelected));
        }
    }

    public bool IsSingleSurahSelected => _selectedSearchDomain?.Type == SearchDomainType.SingleSurah;
    public bool IsSurahRangeSelected => _selectedSearchDomain?.Type == SearchDomainType.SurahRange;

    public int StartSurah
    {
        get => _startSurah;
        set
        {
            if (value >= 1 && value <= 114)
            {
                this.RaiseAndSetIfChanged(ref _startSurah, value);
                // Ensure end is not less than start
                if (_endSurah < value)
                {
                    EndSurah = value;
                }
            }
        }
    }

    public int EndSurah
    {
        get => _endSurah;
        set
        {
            if (value >= 1 && value <= 114)
            {
                this.RaiseAndSetIfChanged(ref _endSurah, value);
                // Ensure start is not greater than end
                if (_startSurah > value)
                {
                    StartSurah = value;
                }
            }
        }
    }

    public bool UseRemoteAudio
    {
        get => _useRemoteAudio;
        set => this.RaiseAndSetIfChanged(ref _useRemoteAudio, value);
    }

    public bool UseRemoteImages
    {
        get => _useRemoteImages;
        set => this.RaiseAndSetIfChanged(ref _useRemoteImages, value);
    }

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public event EventHandler<SettingsSavedEventArgs>? SettingsSaved;
    public event EventHandler? Cancelled;

    private void OnSave()
    {
        var args = new SettingsSavedEventArgs
        {
            SelectedLanguage = SelectedLanguage,
            SearchDomain = SelectedSearchDomain,
            StartSurah = StartSurah,
            EndSurah = EndSurah,
            UseRemoteAudio = UseRemoteAudio,
            UseRemoteImages = UseRemoteImages
        };
        
        SettingsSaved?.Invoke(this, args);
    }

    private void OnCancel()
    {
        Cancelled?.Invoke(this, EventArgs.Empty);
    }

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        this.RaisePropertyChanged(nameof(Localization));
    }
}

public class SearchDomainOption
{
    public SearchDomainType Type { get; set; }
    public string DisplayKey { get; set; } = string.Empty;
}

public enum SearchDomainType
{
    WholeQuran,
    SingleSurah,
    SurahRange
}

public class SettingsSavedEventArgs : EventArgs
{
    public LanguageOption SelectedLanguage { get; set; } = null!;
    public SearchDomainOption SearchDomain { get; set; } = null!;
    public int StartSurah { get; set; }
    public int EndSurah { get; set; }
    public bool UseRemoteAudio { get; set; }
    public bool UseRemoteImages { get; set; }
}
