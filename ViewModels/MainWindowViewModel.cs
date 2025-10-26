using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;
using QuranSearchApp.Models;
using QuranSearchApp.Services;
using Avalonia.Media.Imaging;
using FlowDirection = Avalonia.Media.FlowDirection;

namespace QuranSearchApp.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly QuranSearchService _searchService;
    private readonly AudioService _audioService;
    private readonly PictureService _pictureService;
    private readonly LocalizationService _localizationService;
    private ObservableCollection<string> _rules = new();
    private ObservableCollection<QuranAya> _searchResults = new();
    private string? _selectedRule;
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

    public MainWindowViewModel()
    {
        _searchService = new QuranSearchService();
        _audioService = new AudioService();
        _pictureService = new PictureService();
        _localizationService = LocalizationService.Instance;
        
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
        
        _ = LoadDataAsync();
    }

    public ObservableCollection<string> Rules
    {
        get => _rules;
        set => this.RaiseAndSetIfChanged(ref _rules, value);
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

    private async Task LoadDataAsync()
    {
        try
        {
            StatusMessage = _localizationService.GetString("LoadingQuranText");
            await _searchService.LoadQuranTextAsync();
            
            StatusMessage = _localizationService.GetString("LoadingRules");
            await _searchService.LoadRulesAsync();
            
            var ruleNames = _searchService.GetRuleNames();
            Rules = new ObservableCollection<string>(ruleNames);
            StatusMessage = _localizationService.GetString("ReadyLoadedRules", ruleNames.Count);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            Rules = new ObservableCollection<string>();
        }
    }

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        // Update status message when language changes
        this.RaisePropertyChanged(nameof(Localization));
        this.RaisePropertyChanged(nameof(SelectedLanguage));
        this.RaisePropertyChanged(nameof(CurrentFlowDirection));
        
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
            // Run search on background thread
            var results = await Task.Run(() => _searchService.SearchPattern(SelectedRule));
            
            // Update UI on main thread
            SearchResults = new ObservableCollection<QuranAya>(results);
            StatusMessage = _localizationService.GetString("FoundMatches", results.Count);
            
            // Update rule description
            var selectedRuleInfo = _searchService.GetRuleInfo(SelectedRule);
            RuleDescription = selectedRuleInfo != null ? $"Rule: {selectedRuleInfo.Name}" : $"Rule: {SelectedRule}";
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
        
        _audioService.SetRepeatMode(false);
        
        // Check if this is Aya 1 from any Sura (except Sura 1 and Sura 9)
        // If so, play Basmala (Sura 1, Aya 1) first
        if (SelectedAya.AyaNumber == 1 && SelectedAya.SurahNumber != 1 && SelectedAya.SurahNumber != 9)
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
        
        // Check if this is Aya 1 from any Sura (except Sura 1 and Sura 9)
        // If so, play Basmala (Sura 1, Aya 1) first (only once, not repeated)
        if (SelectedAya.AyaNumber == 1 && SelectedAya.SurahNumber != 1 && SelectedAya.SurahNumber != 9)
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
        if (aya.AyaNumber == 1 && aya.SurahNumber != 1 && aya.SurahNumber != 9)
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
            if (await _pictureService.PictureExistsAsync(SelectedAya.SurahNumber, SelectedAya.AyaNumber))
            {
                var picturePath = _pictureService.GetPicturePath(SelectedAya.SurahNumber, SelectedAya.AyaNumber);
                CurrentPicturePath = picturePath;
                
                var bitmap = await _pictureService.LoadBitmapAsync(picturePath);
                if (bitmap != null)
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
}
