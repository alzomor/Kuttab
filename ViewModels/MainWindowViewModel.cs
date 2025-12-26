using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;
using Kuttab.Core.Models;
using Kuttab.Core.Services;
using Kuttab.Core.ViewModels;
using Kuttab.Core.Interfaces;
using Kuttab.Services;
using Avalonia.Media.Imaging;
using Avalonia.Controls;
using FlowDirection = Avalonia.Media.FlowDirection;

namespace Kuttab.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly QuranSearchService _searchService;
    private readonly IAudioService _audioService;
    private readonly IPictureService _pictureService;
    private readonly LocalizationService _localizationService;
    private readonly ScreenKeepAwakeService _screenKeepAwakeService;
    private ObservableCollection<string> _rules = new();
    private ObservableCollection<RuleGroupNode> _ruleGroups = new();
    private ObservableCollection<QuranAya> _searchResults = new();
    private string? _selectedRule;
    private object? _selectedRuleItem;
    private QuranAya? _selectedAya;
    private int _repeatCount = 1;
    private string _statusMessage = "Ready";
    private string _ruleDescription = string.Empty;
    private bool _isPlaying = false;
    private bool _isRepeating = false;
    private bool _isPlayingSequence = false;
    private bool _isSequencePaused = false;
    private int _lastPlayedAyaIndex = -1;
    private int _currentPlayingAyaIndex = -1;
    private bool _isPlayingBasmala = false;
    private QuranAya? _currentPlayingAya;
    private string? _currentPicturePath;
    private Bitmap? _currentPictureBitmap;
    private bool _useRemoteImages = false;
    private bool _useRemoteAudio = true; // Default to online audio for portability
    private bool _ignoreNextPlaybackCompleted = false;
    private SearchDomainType _searchDomainType = SearchDomainType.WholeQuran;
    private int _searchStartSurah = 1;
    private int _searchEndSurah = 114;
    private string _selectedReciterFolder = "Husary_128kbps";
    private int _fontSize = 18;
    private string _selectedQuranTextFile = "quran-uthmani-ver1.2.txt";

    // Map from display name (may be localized) to Arabic rule name used in rules.json
    private readonly Dictionary<string, string> _ruleDisplayToArabic = new();

    public MainWindowViewModel()
    {
        var fileService = new DesktopFileService();
        _searchService = new QuranSearchService(fileService);
        _audioService = new AudioService();
        _pictureService = new PictureService();
        _screenKeepAwakeService = new ScreenKeepAwakeService();
        _localizationService = new LocalizationService(fileService);
        
        // Initialize audio remote source setting
        _audioService.UseRemoteSource = _useRemoteAudio;
        
        // Subscribe to audio service events
        _audioService.PlaybackStateChanged += OnPlaybackStateChanged;
        _audioService.PlaybackError += OnPlaybackError;
        _audioService.SequencePlaybackEnded += OnSequencePlaybackEnded;
        
        // Subscribe to language change events
        _localizationService.LanguageChanged += OnLanguageChanged;
        
        SearchCommand = new AsyncCommand(SearchAsync);
        PlaySingleCommand = new SimpleCommand(PlaySingle);
        PlayRepeatCommand = new SimpleCommand(PlayRepeat);
        PlayAllCommand = new SimpleCommand(PlayAll);
        StopCommand = new SimpleCommand(StopPlayback);
        PauseSequenceCommand = new SimpleCommand(PauseSequence);
        ResumeSequenceCommand = new SimpleCommand(ResumeSequence);
        OpenSettingsCommand = new AsyncCommand(OpenSettings);
        OpenRecitationCommand = new SimpleCommand(OpenRecitation);
        OpenInfoCommand = new SimpleCommand(OpenInfo);
        OpenDonateCommand = new SimpleCommand(OpenDonate);
        
        _ = LoadDataAsync();
    }

    public ObservableCollection<string> Rules
    {
        get => _rules;
        set => this.RaiseAndSetIfChanged(ref _rules, value);
    }

    public ObservableCollection<RuleGroupNode> RuleGroups
    {
        get => _ruleGroups;
        set => this.RaiseAndSetIfChanged(ref _ruleGroups, value);
    }

    public ObservableCollection<QuranAya> SearchResults
    {
        get => _searchResults;
        set => this.RaiseAndSetIfChanged(ref _searchResults, value);
    }

    public string? SelectedRule
    {
        get => _selectedRule;
        set => this.RaiseAndSetIfChanged(ref _selectedRule, value);
    }

    // Selected item in the grouped TreeView. When a leaf rule item is selected,
    // map it back to the display key used by the ComboBox/lookup dictionary.
    public object? SelectedRuleItem
    {
        get => _selectedRuleItem;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedRuleItem, value);

            if (value is RuleItemNode ruleItem && !string.IsNullOrWhiteSpace(ruleItem.DisplayKey))
            {
                SelectedRule = ruleItem.DisplayKey;
            }
        }
    }

    public QuranAya? SelectedAya
    {
        get => _selectedAya;
        set 
        { 
            this.RaiseAndSetIfChanged(ref _selectedAya, value);
            OnSelectedAyaChanged();
        }
    }

    private void OnSelectedAyaChanged()
    {
        if (SelectedAya != null)
        {
            // Get the rule description if available
            RuleDescription = string.Empty;
            if (!string.IsNullOrEmpty(SelectedRule))
            {
                var rule = _searchService.GetRuleInfo(SelectedRule);
                if (rule != null && rule.Cases.Count > 0)
                {
                    RuleDescription = rule.Cases[0].Description;
                }
            }
            
            this.RaisePropertyChanged(nameof(HasRuleDescription));
            _ = UpdatePictureForSelectedAyaAsync();
        }
    }

    public int RepeatCount
    {
        get => _repeatCount;
        set => this.RaiseAndSetIfChanged(ref _repeatCount, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
    }

    public string RuleDescription
    {
        get => _ruleDescription;
        set => this.RaiseAndSetIfChanged(ref _ruleDescription, value);
    }

    public bool IsPlaying
    {
        get => _isPlaying;
        set => this.RaiseAndSetIfChanged(ref _isPlaying, value);
    }

    public bool IsRepeating
    {
        get => _isRepeating;
        set => this.RaiseAndSetIfChanged(ref _isRepeating, value);
    }

    public bool IsPlayingSequence
    {
        get => _isPlayingSequence;
        set 
        { 
            this.RaiseAndSetIfChanged(ref _isPlayingSequence, value);
            this.RaisePropertyChanged(nameof(CanPauseSequence));
            this.RaisePropertyChanged(nameof(IsSequenceActive));
            this.RaisePropertyChanged(nameof(CanStartSequence));
        }
    }

    public bool IsSequencePaused
    {
        get => _isSequencePaused;
        set 
        { 
            this.RaiseAndSetIfChanged(ref _isSequencePaused, value);
            this.RaisePropertyChanged(nameof(CanPauseSequence));
            this.RaisePropertyChanged(nameof(PauseResumeButtonText));
            this.RaisePropertyChanged(nameof(PauseResumeCommand));
            this.RaisePropertyChanged(nameof(PauseResumeButtonColor));
        }
    }

    public bool CanPauseSequence => _isPlayingSequence && !_isSequencePaused;

    public bool IsSequenceActive => _isPlayingSequence; // Button should be visible when sequence is active (playing or paused)

    public bool CanStartSequence => !_isPlayingSequence; // Play All button should only be enabled when no sequence is playing

    public QuranAya? CurrentPlayingAya
    {
        get => _currentPlayingAya;
        set => this.RaiseAndSetIfChanged(ref _currentPlayingAya, value);
    }

    public string PauseResumeButtonText => _isSequencePaused ? "Resume Sequence" : "Pause Sequence";

    public ICommand PauseResumeCommand => _isSequencePaused ? ResumeSequenceCommand : PauseSequenceCommand;

    public string PauseResumeButtonColor => _isSequencePaused ? "Green" : "Orange";

    public bool UseRemoteImages
    {
        get => _useRemoteImages;
        set
        {
            this.RaiseAndSetIfChanged(ref _useRemoteImages, value);
            _pictureService.UseRemoteSource = value;
            // Update the current picture when the source changes
            if (SelectedAya != null)
            {
                _ = UpdatePictureForSelectedAyaAsync();
            }
        }
    }

    public bool UseRemoteAudio
    {
        get => _useRemoteAudio;
        set
        {
            this.RaiseAndSetIfChanged(ref _useRemoteAudio, value);
            _audioService.UseRemoteSource = value;
        }
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

    public string? CurrentPicturePath
    {
        get => _currentPicturePath;
        set => this.RaiseAndSetIfChanged(ref _currentPicturePath, value);
    }

    public Bitmap? CurrentPictureBitmap
    {
        get => _currentPictureBitmap;
        set => this.RaiseAndSetIfChanged(ref _currentPictureBitmap, value);
    }

    public bool HasRuleDescription => !string.IsNullOrWhiteSpace(RuleDescription);

    public LocalizationService Localization => _localizationService;

    public List<LanguageOption> AvailableLanguages => _localizationService.AvailableLanguages;

    public LanguageOption SelectedLanguage
    {
        get => _localizationService.AvailableLanguages.Find(l => l.Code == _localizationService.CurrentLanguage) 
               ?? _localizationService.AvailableLanguages[0];
        set
        {
            if (value != null && _localizationService.CurrentLanguage != value.Code)
            {
                _localizationService.CurrentLanguage = value.Code;
                this.RaisePropertyChanged(nameof(SelectedLanguage));
            }
        }
    }

    public FlowDirection CurrentFlowDirection => _localizationService.IsRightToLeft 
        ? FlowDirection.RightToLeft 
        : FlowDirection.LeftToRight;

    public ICommand SearchCommand { get; }
    public ICommand PlaySingleCommand { get; }
    public ICommand PlayRepeatCommand { get; }
    public ICommand PlayAllCommand { get; }
    public ICommand StopCommand { get; }
    public ICommand PauseSequenceCommand { get; }
    public ICommand ResumeSequenceCommand { get; }
    public ICommand OpenSettingsCommand { get; }
    public ICommand OpenRecitationCommand { get; }
    public ICommand OpenInfoCommand { get; }
    public ICommand OpenDonateCommand { get; }

    private async Task LoadDataAsync()
    {
        try
        {
            StatusMessage = _localizationService.GetString("LoadingQuranText");
            await _searchService.LoadQuranTextAsync();
            
            StatusMessage = _localizationService.GetString("LoadingRules");
            await _searchService.LoadRulesAsync();
            
            UpdateRuleDisplayNames();
            var ruleNames = _searchService.GetRuleNames();
            StatusMessage = _localizationService.GetString("ReadyLoadedRules", ruleNames.Count);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            Rules = new ObservableCollection<string>();
        }
    }

    private void UpdateRuleDisplayNames()
    {
        var rules = _searchService.GetRules();
        var displayNames = new List<string>();
        _ruleDisplayToArabic.Clear();
        var languageCode = _localizationService.CurrentLanguage;

        // Build flat list for ComboBox and grouped structure for TreeView
        var groupMap = new Dictionary<string, RuleGroupNode>();
        var groupList = new List<RuleGroupNode>();

        foreach (var rule in rules)
        {
            var arabicName = rule.Name;
            var localizedRuleName = RuleNameTranslator.GetLocalizedName(arabicName, languageCode);
            var groupTitle = RuleGroupTranslator.GetGroupTitle(rule.Group, languageCode);

            string displayName;
            if (!string.IsNullOrWhiteSpace(groupTitle))
            {
                displayName = $"{groupTitle} – {localizedRuleName}";
            }
            else
            {
                displayName = localizedRuleName;
            }

            displayNames.Add(displayName);
            _ruleDisplayToArabic[displayName] = arabicName;

            // Populate grouped structure for TreeView
            var effectiveGroupTitle = string.IsNullOrWhiteSpace(groupTitle)
                ? localizedRuleName
                : groupTitle;

            if (!groupMap.TryGetValue(effectiveGroupTitle, out var groupNode))
            {
                groupNode = new RuleGroupNode { GroupTitle = effectiveGroupTitle };
                groupMap[effectiveGroupTitle] = groupNode;
                groupList.Add(groupNode);
            }

            groupNode.Rules.Add(new RuleItemNode
            {
                DisplayKey = displayName,
                RuleTitle = localizedRuleName
            });
        }

        Rules = new ObservableCollection<string>(displayNames);
        RuleGroups = new ObservableCollection<RuleGroupNode>(groupList);
    }

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        // Update status message when language changes
        this.RaisePropertyChanged(nameof(Localization));
        // Don't raise SelectedLanguage here - it's already raised in the setter
        this.RaisePropertyChanged(nameof(CurrentFlowDirection));

        // Update rule display names when language changes
        UpdateRuleDisplayNames();
        
        // If we have results, update the status message
        if (SearchResults.Count > 0)
        {
            StatusMessage = _localizationService.GetString("FoundMatches", SearchResults.Count);
        }
        else if (Rules.Count > 0)
        {
            StatusMessage = _localizationService.GetString("ReadyLoadedRules", Rules.Count);
        }
        else
        {
            StatusMessage = _localizationService.GetString("Ready");
        }
    }

    private async Task SearchAsync()
    {
        if (string.IsNullOrWhiteSpace(SelectedRule))
        {
            StatusMessage = _localizationService.GetString("PleaseSelectRule");
            return;
        }

        try
        {
            StatusMessage = _localizationService.GetString("Searching");
            // Determine Arabic rule name from selected display name
            var selectedDisplayName = SelectedRule ?? string.Empty;
            if (!_ruleDisplayToArabic.TryGetValue(selectedDisplayName, out var arabicRuleName))
            {
                arabicRuleName = selectedDisplayName; // Fallback: assume it's already Arabic
            }

            // Run search on background thread using Arabic rule name with domain filtering
            // Pass domain parameters to search service for efficient filtering BEFORE searching
            var results = await Task.Run(() => 
            {
                if (_searchDomainType == SearchDomainType.SingleSurah)
                {
                    return _searchService.SearchPattern(arabicRuleName, _searchStartSurah, _searchStartSurah);
                }
                else if (_searchDomainType == SearchDomainType.SurahRange)
                {
                    return _searchService.SearchPattern(arabicRuleName, _searchStartSurah, _searchEndSurah);
                }
                else
                {
                    return _searchService.SearchPattern(arabicRuleName);
                }
            });
            
            // Update UI on main thread
            SearchResults = new ObservableCollection<QuranAya>(results);
            
            // Reset selected Aya when new search results are loaded
            SelectedAya = null;
            
            // Scroll to top to show first result
            ScrollToTop();
            
            StatusMessage = _localizationService.GetString("FoundMatches", results.Count);
            
            // Update rule description with localized rule name based on current language
            var selectedRuleInfo = _searchService.GetRuleInfo(arabicRuleName);
            var arabicRuleNameForDescription = selectedRuleInfo?.Name ?? arabicRuleName;
            var languageCode = _localizationService.CurrentLanguage;
            var localizedRuleName = RuleNameTranslator.GetLocalizedName(arabicRuleNameForDescription, languageCode);

            // For Arabic, keep the original name; for other languages, show the translated name
            if (languageCode == "ar")
            {
                RuleDescription = $"Rule: {arabicRuleNameForDescription}";
            }
            else
            {
                RuleDescription = $"Rule: {localizedRuleName}";
            }
            this.RaisePropertyChanged(nameof(HasRuleDescription));
        }
        catch (Exception ex)
        {
            StatusMessage = _localizationService.GetString("SearchError", ex.Message);
        }
    }

    private async void PlaySingle()
    {
        if (SelectedAya == null)
        {
            StatusMessage = _localizationService.GetString("PleaseSelectAya");
            return;
        }

        if (!_audioService.AudioFileExists(SelectedAya.SurahNumber, SelectedAya.AyaNumber))
        {
            StatusMessage = _localizationService.GetString("AudioFileNotFound", SelectedAya.SurahNumber, SelectedAya.AyaNumber);
            return;
        }
        
        // Prevent screen from sleeping during playback
        _screenKeepAwakeService.PreventSleep();
        
        _audioService.SetRepeatMode(false);
        
        // Check if this is Aya 1 from any Sura (except Sura 1 and Sura 9)
        // If so, play Basmala (Sura 1, Aya 1) first
        // Skip Basmalah for Menshawi reciter (his recordings already include it)
        if (SelectedAya.AyaNumber == 1 && SelectedAya.SurahNumber != 1 && SelectedAya.SurahNumber != 9 && !ShouldSkipBasmalah(SelectedAya.SurahNumber))
        {
            if (_audioService.AudioFileExists(1, 1))
            {
                StatusMessage = _localizationService.GetString("PlayingBasmala", SelectedAya.SurahNumber, SelectedAya.AyaNumber);
                await _audioService.PlayAyaAsync(1, 1);
                
                // Wait for Basmala to finish
                while (_audioService.IsPlaying)
                {
                    await Task.Delay(100);
                }
            }
        }
        
        StatusMessage = _localizationService.GetString("PlayingAya", SelectedAya.SurahNumber, SelectedAya.AyaNumber);
        await _audioService.PlayAyaAsync(SelectedAya.SurahNumber, SelectedAya.AyaNumber);
    }

    private async void PlayRepeat()
    {
        if (SelectedAya == null)
        {
            StatusMessage = _localizationService.GetString("PleaseSelectAya");
            return;
        }

        if (!_audioService.AudioFileExists(SelectedAya.SurahNumber, SelectedAya.AyaNumber))
        {
            StatusMessage = _localizationService.GetString("AudioFileNotFound", SelectedAya.SurahNumber, SelectedAya.AyaNumber);
            return;
        }
        
        // Prevent screen from sleeping during playback
        _screenKeepAwakeService.PreventSleep();
        
        // Check if this is Aya 1 from any Sura (except Sura 1 and Sura 9)
        // If so, play Basmala (Sura 1, Aya 1) first (only once, not repeated)
        // Skip Basmalah for Menshawi reciter (his recordings already include it)
        if (SelectedAya.AyaNumber == 1 && SelectedAya.SurahNumber != 1 && SelectedAya.SurahNumber != 9 && !ShouldSkipBasmalah(SelectedAya.SurahNumber))
        {
            if (_audioService.AudioFileExists(1, 1))
            {
                StatusMessage = _localizationService.GetString("PlayingBasmala", SelectedAya.SurahNumber, SelectedAya.AyaNumber);
                _audioService.SetRepeatMode(false);
                await _audioService.PlayAyaAsync(1, 1);
                
                // Wait for Basmala to finish
                while (_audioService.IsPlaying)
                {
                    await Task.Delay(100);
                }
            }
        }
        
        StatusMessage = _localizationService.GetString("PlayingAyaOnRepeat", SelectedAya.SurahNumber, SelectedAya.AyaNumber);
        _audioService.SetRepeatMode(true);
        await _audioService.PlayAyaAsync(SelectedAya.SurahNumber, SelectedAya.AyaNumber);
    }

    private async void PlayAll()
    {
        if (SearchResults == null || SearchResults.Count == 0)
        {
            StatusMessage = _localizationService.GetString("NoSearchResults");
            return;
        }
        
        // Prevent screen from sleeping during playback
        _screenKeepAwakeService.PreventSleep();
        
        // Reset sequence state
        _isPlayingSequence = true;
        _isSequencePaused = false;
        _lastPlayedAyaIndex = -1;
        _currentPlayingAyaIndex = -1;
        CurrentPlayingAya = null;
        IsPlayingSequence = true;
        IsSequencePaused = false;
        
        StatusMessage = _localizationService.GetString("PlayingAllAyas", SearchResults.Count);
        _audioService.SetRepeatMode(false);
        
        await PlaySingleAyaInSequence(0);
    }
    
    private async Task PlaySingleAyaInSequence(int index)
    {
        // Check if we've reached the end
        if (index >= SearchResults.Count)
        {
            // Sequence completed
            _isPlayingSequence = false;
            _isSequencePaused = false;
            IsPlayingSequence = false;
            IsSequencePaused = false;
            _lastPlayedAyaIndex = -1;
            _currentPlayingAyaIndex = -1;
            StatusMessage = _localizationService.GetString("FinishedPlayingAll");
            
            // Allow screen to sleep again
            _screenKeepAwakeService.AllowSleep();
            
            // Clear highlighting
            if (CurrentPlayingAya != null)
            {
                CurrentPlayingAya.IsCurrentlyPlaying = false;
                CurrentPlayingAya = null;
            }
            return;
        }
        
        // Check if stop was requested
        if (!_isPlayingSequence)
        {
            StatusMessage = _localizationService.GetString("SequenceStopped");
            IsPlayingSequence = false;
            return;
        }
        
        // Check if pause was requested
        if (_isSequencePaused)
        {
            _lastPlayedAyaIndex = index - 1;
            StatusMessage = _localizationService.GetString("SequencePausedMessage");
            return;
        }
        
        var aya = SearchResults[index];
        if (!_audioService.AudioFileExists(aya.SurahNumber, aya.AyaNumber))
        {
            // Skip this Aya and move to next
            await PlaySingleAyaInSequence(index + 1);
            return;
        }
        
        _currentPlayingAyaIndex = index;
        
        // Clear previous highlighting
        if (CurrentPlayingAya != null)
            CurrentPlayingAya.IsCurrentlyPlaying = false;
        
        // Set current playing Aya and highlight it
        CurrentPlayingAya = aya;
        aya.IsCurrentlyPlaying = true;
        
        // Auto-select the currently playing Aya to make it scroll into view
        SelectedAya = aya;
        
        // Play Basmala before Aya 1 of any Surah (except Surah 1 and 9)
        // Skip Basmalah for Menshawi reciter (his recordings already include it)
        if (aya.AyaNumber == 1 && aya.SurahNumber != 1 && aya.SurahNumber != 9 && !ShouldSkipBasmalah(aya.SurahNumber))
        {
            if (_audioService.AudioFileExists(1, 1))
            {
                StatusMessage = _localizationService.GetString("PlayingBasmala", aya.SurahNumber, aya.AyaNumber);
                
                // Set flag to prevent sequence from advancing after Basmala
                _isPlayingBasmala = true;
                
                // Play Basmala
                _audioService.SetRepeatMode(false);
                await _audioService.PlayAyaAsync(1, 1);
                
                // Wait for Basmala to finish
                while (_audioService.IsPlaying && _isPlayingSequence && !_isSequencePaused)
                {
                    await Task.Delay(100);
                }
                
                // Clear the Basmala flag
                _isPlayingBasmala = false;
                
                // Check if stop/pause was requested during Basmala
                if (!_isPlayingSequence || _isSequencePaused)
                {
                    if (!_isPlayingSequence)
                    {
                        StatusMessage = _localizationService.GetString("SequenceStopped");
                        IsPlayingSequence = false;
                    }
                    else
                    {
                        _lastPlayedAyaIndex = index - 1;
                        StatusMessage = _localizationService.GetString("SequencePausedMessage");
                    }
                    return;
                }
            }
        }
        
        // Play the actual Aya
        StatusMessage = $"{_localizationService.GetString("PlayingAya", aya.SurahNumber, aya.AyaNumber)} ({index + 1}/{SearchResults.Count})";
        _audioService.SetRepeatMode(false);
        await _audioService.PlayAyaAsync(aya.SurahNumber, aya.AyaNumber);
        
        // Mark this Aya as completed (will be used when resuming)
        _lastPlayedAyaIndex = index;
        
        // Note: The OnSequencePlaybackEnded event will handle playing the next Aya
    }

    private async void StopPlayback()
    {
        _ignoreNextPlaybackCompleted = true;
        CancelSequence();
        await _audioService.StopAsync();
        
        // Allow screen to sleep again
        _screenKeepAwakeService.AllowSleep();
        
        // Update UI state on the UI thread
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            // Reset all playback states
            _isPlaying = false;
            _isRepeating = false;
            
            // Update properties with change notifications
            IsPlaying = false;
            IsRepeating = false;
            
            // Clear highlighting from previous playing Aya
            if (CurrentPlayingAya != null)
            {
                CurrentPlayingAya.IsCurrentlyPlaying = false;
                this.RaisePropertyChanged(nameof(CurrentPlayingAya));
            }
            CurrentPlayingAya = null;
            
            // Force UI updates
            this.RaisePropertyChanged(nameof(IsPlaying));
            this.RaisePropertyChanged(nameof(IsRepeating));
            
            StatusMessage = _localizationService.GetString("PlaybackStopped");
        });
        _ignoreNextPlaybackCompleted = false;
    }
    
    private async void PauseSequence()
    {
        if (!_isPlayingSequence || _isSequencePaused)
        {
            return;
        }
        
        // Save the current playing Aya as the last position to resume from
        if (_currentPlayingAyaIndex >= 0)
        {
            _lastPlayedAyaIndex = _currentPlayingAyaIndex - 1; // Resume from current Aya
        }
        
        _isSequencePaused = true;
        IsSequencePaused = true;
        await _audioService.StopAsync(); // Stop current audio
        StatusMessage = _localizationService.GetString("SequencePausedMessage");
    }
    
    private async void ResumeSequence()
    {
        if (!_isSequencePaused || !_isPlayingSequence)
        {
            StatusMessage = _localizationService.GetString("NoPausedSequence");
            return;
        }
        
        _isSequencePaused = false;
        IsSequencePaused = false;
        
        // Resume from the next Aya after the last completed one
        int resumeIndex = _lastPlayedAyaIndex + 1;
        
        if (resumeIndex < SearchResults.Count)
        {
            StatusMessage = _localizationService.GetString("ResumingSequence", resumeIndex + 1, SearchResults.Count);
            await PlaySingleAyaInSequence(resumeIndex);
        }
        else
        {
            // All ayas were completed
            _isPlayingSequence = false;
            IsPlayingSequence = false;
            _lastPlayedAyaIndex = -1;
            StatusMessage = _localizationService.GetString("SequenceCompleted");
        }
    }

    private void OnPlaybackStateChanged(object? sender, bool isPlaying)
    {
        // Update the properties on the UI thread
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            IsPlaying = isPlaying;
            IsRepeating = _audioService.IsRepeating;
            
            // Force UI update by raising property changed for IsPlaying
            this.RaisePropertyChanged(nameof(IsPlaying));
            
            // If playback stopped, ensure the UI reflects this immediately
            if (!isPlaying)
            {
                _isPlaying = false;
                _isRepeating = false;
                IsRepeating = false;
                this.RaisePropertyChanged(nameof(IsPlaying));
                this.RaisePropertyChanged(nameof(IsRepeating));
                
                // Allow screen to sleep when single playback ends (not in sequence mode)
                if (!_isPlayingSequence)
                {
                    _screenKeepAwakeService.AllowSleep();
                }
            }
        });
    }

    private async void OnSequencePlaybackEnded(object? sender, EventArgs e)
    {
        if (_ignoreNextPlaybackCompleted)
            return;

        // Run on UI thread
        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
        {
            // Don't advance if we're playing Basmala - wait for the actual Aya to finish
            if (_isPlayingBasmala)
            {
                return; // Don't clear the flag or advance - just wait
            }
            
            // Only advance if we're in sequence mode
            if (_isPlayingSequence && !_isSequencePaused && _currentPlayingAyaIndex >= 0)
            {
                int nextIndex = _currentPlayingAyaIndex + 1;
                await PlaySingleAyaInSequence(nextIndex);
            }
        });
    }

    private void OnPlaybackError(object? sender, string error)
    {
        // Update the UI on the UI thread
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            StatusMessage = _localizationService.GetString("AudioError", error);
            IsPlaying = false;
            
            // Ensure the UI is updated immediately
            this.RaisePropertyChanged(nameof(StatusMessage));
            this.RaisePropertyChanged(nameof(IsPlaying));
        });
    }

    private async Task UpdatePictureForSelectedAyaAsync()
    {
        if (SelectedAya == null)
        {
            CurrentPicturePath = null;
            CurrentPictureBitmap = null;
            Console.WriteLine("[ViewModel] SelectedAya is null, clearing picture path");
            return;
        }

        Console.WriteLine($"[ViewModel] Updating picture for Aya {SelectedAya.SurahNumber}:{SelectedAya.AyaNumber}");
        
        try
        {
            if (_pictureService.PictureExists(SelectedAya.SurahNumber, SelectedAya.AyaNumber))
            {
                var picturePath = _pictureService.GetPicturePath(SelectedAya.SurahNumber, SelectedAya.AyaNumber);
                CurrentPicturePath = picturePath;
                
                var bitmapObject = await _pictureService.LoadBitmapAsync(picturePath);
                if (bitmapObject != null && bitmapObject is Bitmap bitmap)
                {
                    CurrentPictureBitmap = bitmap;
                    Console.WriteLine($"[ViewModel] Picture loaded successfully: {picturePath}");
                }
                else
                {
                    CurrentPictureBitmap = null;
                    StatusMessage = _localizationService.GetString("ErrorCouldNotLoadImage");
                    Console.WriteLine($"[ViewModel] Failed to load bitmap for: {picturePath}");
                }
            }
            else
            {
                CurrentPicturePath = null;
                CurrentPictureBitmap = null;
                StatusMessage = _localizationService.GetString("PictureNotFound", SelectedAya.SurahNumber, SelectedAya.AyaNumber);
                Console.WriteLine($"[ViewModel] Picture not found for Aya {SelectedAya.SurahNumber}:{SelectedAya.AyaNumber}");
            }
        }
        catch (Exception ex)
        {
            CurrentPicturePath = null;
            CurrentPictureBitmap = null;
            StatusMessage = _localizationService.GetString("ErrorLoadingPicture", ex.Message);
            Console.WriteLine($"[ViewModel] Error in UpdatePictureForSelectedAyaAsync: {ex.Message}");
        }
    }
    
    // Keep the old method for backward compatibility
    private void UpdatePictureForSelectedAya()
    {
        _ = UpdatePictureForSelectedAyaAsync();
    }

    private void CancelSequence()
    {
        _isPlayingSequence = false;
        _isSequencePaused = false;
        _lastPlayedAyaIndex = -1;
        _currentPlayingAyaIndex = -1;
        CurrentPlayingAya = null;
        IsPlayingSequence = false;
        IsSequencePaused = false;
    }

    private async void OpenRecitation()
    {
        var recitationViewModel = new RecitationViewModel(_localizationService, _selectedReciterFolder);
        
        var recitationWindow = new Views.RecitationWindow
        {
            DataContext = recitationViewModel
        };
        
        await recitationWindow.ShowDialog(
            Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow
                : null);
    }

    private async Task OpenSettings()
    {
        var currentSearchDomain = new SearchDomainOption
        {
            Type = _searchDomainType,
            DisplayKey = _searchDomainType switch
            {
                SearchDomainType.WholeQuran => "WholeQuran",
                SearchDomainType.SingleSurah => "SingleSurah",
                SearchDomainType.SurahRange => "SurahRange",
                _ => "WholeQuran"
            }
        };

        var settingsViewModel = new SettingsViewModel(
            _localizationService,
            SelectedLanguage,
            UseRemoteAudio,
            UseRemoteImages,
            currentSearchDomain,
            _searchStartSurah,
            _searchEndSurah,
            _selectedReciterFolder,
            _fontSize,
            _selectedQuranTextFile);

        var settingsWindow = new Views.SettingsWindow
        {
            DataContext = settingsViewModel
        };

        var result = await settingsWindow.ShowDialog<SettingsSavedEventArgs?>(
            Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow
                : null);

        if (result != null)
        {
            // Apply settings
            SelectedLanguage = result.SelectedLanguage;
            UseRemoteAudio = result.UseRemoteAudio;
            UseRemoteImages = result.UseRemoteImages;
            _searchDomainType = result.SearchDomain.Type;
            _searchStartSurah = result.StartSurah;
            _searchEndSurah = result.EndSurah;
            _selectedReciterFolder = result.SelectedReciterFolder;
            FontSize = result.FontSize;
            
            // Update audio service with new reciter
            if (_audioService is Services.AudioService audioService)
            {
                audioService.SelectedReciter = _selectedReciterFolder;
            }

            // Clear search results when settings change
            SearchResults.Clear();
            _currentPlayingAyaIndex = -1;
            IsPlayingSequence = false;
            await _audioService?.StopAsync();
            StatusMessage = _localizationService.GetString("Ready");

            // Check if Quran text file changed and reload data if needed
            if (_selectedQuranTextFile != result.SelectedQuranTextFile)
            {
                _selectedQuranTextFile = result.SelectedQuranTextFile;
                _searchService.SetQuranTextFile(_selectedQuranTextFile);
                StatusMessage = _localizationService.GetString("QuranTextChanged");
            }

            // Update status message
            StatusMessage = _localizationService.GetString("SettingsSaved");
        }
    }

    private async void OpenInfo()
    {
        var infoViewModel = new InfoViewModel(_localizationService);

        var infoWindow = new Views.InfoWindow
        {
            DataContext = infoViewModel
        };

        await infoWindow.ShowDialog(
            Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow
                : null);
    }

    private async void OpenDonate()
    {
        var donateViewModel = new DonateViewModel(_localizationService);

        var donateWindow = new Views.DonateWindow
        {
            DataContext = donateViewModel
        };

        await donateWindow.ShowDialog(
            Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow
                : null);
    }
    
    /// <summary>
    /// Returns true if Basmalah should be skipped for the current reciter and surah.
    /// Some reciters already include Basmalah in their recordings.
    /// </summary>
    private bool ShouldSkipBasmalah(int surahNumber = 0)
    {
        // Menshawi and Mustafa Ismail always include Basmalah
        if (_selectedReciterFolder == "Menshawi_32kbps" ||
            _selectedReciterFolder == "Mustafa_Ismail_48kbps")
            return true;
        
        // Mahmoud Ali Al-Banna includes Basmalah except for Surah 108 (Al-Kawthar)
        if (_selectedReciterFolder == "mahmoud_ali_al_banna_32kbps")
            return surahNumber != 108;
        
        return false;
    }
    
    /// <summary>
    /// Scrolls the search results ListBox to the top
    /// </summary>
    private void ScrollToTop()
    {
        try
        {
            // Find the main window
            if (Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
            {
                var mainWindow = desktop.MainWindow;
                if (mainWindow != null)
                {
                    // Find the ListBox by name
                    var listBox = mainWindow.GetControl<Avalonia.Controls.ListBox>("ResultsListBox");
                    if (listBox != null)
                    {
                        // Scroll to the first item
                        if (listBox.ItemCount > 0)
                        {
                            listBox.ScrollIntoView(0);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error scrolling to top: {ex.Message}");
        }
    }
}

public class RuleGroupNode
{
    public string GroupTitle { get; set; } = string.Empty;
    public ObservableCollection<RuleItemNode> Rules { get; } = new();
}

public class RuleItemNode
{
    // Key used for mapping back to Arabic rule name (matches ComboBox display string)
    public string DisplayKey { get; set; } = string.Empty;
    // Text shown under each group in the TreeView
    public string RuleTitle { get; set; } = string.Empty;
}
