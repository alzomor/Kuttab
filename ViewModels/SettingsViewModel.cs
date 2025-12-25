using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using ReactiveUI;
using QuranSearch.Core.Services;
using QuranSearch.Core.ViewModels;
using QuranSearch.Core.Models;

namespace QuranSearchApp.ViewModels;

public class SurahItem
{
    public int Number { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DisplayText => $"{Number}. {Name}";
}

public class ReciterOption
{
    public string FolderName { get; set; } = string.Empty;
    public string DisplayKey { get; set; } = string.Empty;
}

public class QuranTextOption
{
    public string FileName { get; set; } = string.Empty;
    public string DisplayKey { get; set; } = string.Empty;
}

public class SettingsViewModel : ViewModelBase
{
    private readonly LocalizationService _localizationService;
    private LanguageOption _selectedLanguage;
    private SearchDomainOption _selectedSearchDomain;
    private bool _useRemoteAudio;
    private bool _useRemoteImages;
    private int _startSurah = 1;
    private int _endSurah = 114;
    private SurahItem? _selectedStartSurah;
    private SurahItem? _selectedEndSurah;
    private ReciterOption _selectedReciter;
    private int _fontSize = 18;
    private QuranTextOption _selectedQuranText;

    public SettingsViewModel(LocalizationService localizationService, 
                            LanguageOption currentLanguage,
                            bool useRemoteAudio,
                            bool useRemoteImages,
                            SearchDomainOption? currentSearchDomain = null,
                            int startSurah = 1,
                            int endSurah = 114,
                            string? selectedReciterFolder = null,
                            int fontSize = 18,
                            string? selectedQuranTextFile = null)
    {
        _localizationService = localizationService;
        _selectedLanguage = currentLanguage;
        _useRemoteAudio = useRemoteAudio;
        _useRemoteImages = useRemoteImages;
        _selectedSearchDomain = currentSearchDomain ?? SearchDomainOptions[0];
        _startSurah = startSurah;
        _endSurah = endSurah;
        _fontSize = fontSize;
        _selectedReciter = ReciterOptions.FirstOrDefault(r => r.FolderName == selectedReciterFolder) 
                          ?? ReciterOptions.First(r => r.FolderName == "Husary_128kbps");
        _selectedQuranText = QuranTextOptions.FirstOrDefault(q => q.FileName == selectedQuranTextFile)
                          ?? QuranTextOptions.First(q => q.FileName == "quran-uthmani-ver1.2.txt");
        
        // Initialize surah list
        InitializeSurahList();
        
        // Set selected surahs based on initial values
        _selectedStartSurah = SurahList.FirstOrDefault(s => s.Number == startSurah);
        _selectedEndSurah = SurahList.FirstOrDefault(s => s.Number == endSurah);
        
        SaveCommand = new SimpleCommand(OnSave);
        CancelCommand = new SimpleCommand(OnCancel);
        
        // Subscribe to language changes
        _localizationService.LanguageChanged += OnLanguageChanged;
    }
    
    private void InitializeSurahList()
    {
        for (int i = 1; i <= 114; i++)
        {
            SurahList.Add(new SurahItem
            {
                Number = i,
                Name = SurahInfo.GetSurahName(i)
            });
        }
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

    public List<ReciterOption> ReciterOptions { get; } = new()
    {
        new ReciterOption { FolderName = "Abdul_Basit_Murattal_192kbps", DisplayKey = "ReciterAbdulBasit" },
        new ReciterOption { FolderName = "Ayman_Sowaid_64kbps", DisplayKey = "ReciterAymanSowaid" },
        new ReciterOption { FolderName = "Husary_128kbps", DisplayKey = "ReciterHusary" },
        new ReciterOption { FolderName = "Husary_Muallim_128kbps", DisplayKey = "ReciterHusaryMuallim" },
        new ReciterOption { FolderName = "Menshawi_32kbps", DisplayKey = "ReciterMenshawi" },
        new ReciterOption { FolderName = "Mohammad_al_Tablaway_128kbps", DisplayKey = "ReciterTablaway" },
        new ReciterOption { FolderName = "Mustafa_Ismail_48kbps", DisplayKey = "ReciterMustafaIsmail" },
        new ReciterOption { FolderName = "Muhammad_Ayyoub_128kbps", DisplayKey = "ReciterAyyoub" },
        new ReciterOption { FolderName = "mahmoud_ali_al_banna_32kbps", DisplayKey = "ReciterBanna" }
    };

    public ReciterOption SelectedReciter
    {
        get => _selectedReciter;
        set => this.RaiseAndSetIfChanged(ref _selectedReciter, value);
    }

    public List<QuranTextOption> QuranTextOptions { get; } = new()
    {
        new QuranTextOption { FileName = "quran-uthmani-ver1.2.txt", DisplayKey = "QuranTextUthmaniClean" },
        new QuranTextOption { FileName = "quran-uthmani.txt", DisplayKey = "QuranTextUthmaniOriginal" }
    };

    public QuranTextOption SelectedQuranText
    {
        get => _selectedQuranText;
        set => this.RaiseAndSetIfChanged(ref _selectedQuranText, value);
    }

    public ObservableCollection<SurahItem> SurahList { get; } = new();

    public SurahItem? SelectedStartSurah
    {
        get => _selectedStartSurah;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedStartSurah, value);
            if (value != null)
            {
                _startSurah = value.Number;
                // Ensure end is not less than start
                if (_selectedEndSurah != null && _selectedEndSurah.Number < value.Number)
                {
                    SelectedEndSurah = value;
                }
            }
        }
    }

    public SurahItem? SelectedEndSurah
    {
        get => _selectedEndSurah;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedEndSurah, value);
            if (value != null)
            {
                _endSurah = value.Number;
                // Ensure start is not greater than end
                if (_selectedStartSurah != null && _selectedStartSurah.Number > value.Number)
                {
                    SelectedStartSurah = value;
                }
            }
        }
    }

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
        set
        {
            this.RaiseAndSetIfChanged(ref _useRemoteAudio, value);
            this.RaisePropertyChanged(nameof(IsReciterSelectionVisible));
        }
    }

    public bool IsReciterSelectionVisible => _useRemoteAudio;

    public bool UseRemoteImages
    {
        get => _useRemoteImages;
        set => this.RaiseAndSetIfChanged(ref _useRemoteImages, value);
    }

    public int FontSize
    {
        get => _fontSize;
        set
        {
            if (value >= 12 && value <= 32)
            {
                this.RaiseAndSetIfChanged(ref _fontSize, value);
            }
        }
    }

    public List<int> FontSizeOptions { get; } = new() { 12, 14, 16, 18, 20, 22, 24, 26, 28, 30, 32 };

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
            UseRemoteImages = UseRemoteImages,
            SelectedReciterFolder = SelectedReciter?.FolderName ?? "Husary_128kbps",
            FontSize = FontSize,
            SelectedQuranTextFile = SelectedQuranText?.FileName ?? "quran-uthmani-ver1.2.txt"
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
    public string SelectedReciterFolder { get; set; } = "Husary_128kbps";
    public int FontSize { get; set; } = 18;
    public string SelectedQuranTextFile { get; set; } = "quran-uthmani-ver1.2.txt";
}
