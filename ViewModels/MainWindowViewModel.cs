using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;
using QuranSearchApp.Models;
using QuranSearchApp.Services;
using Avalonia.Media.Imaging;

namespace QuranSearchApp.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly QuranSearchService _searchService;
    private readonly AudioService _audioService;
    private readonly PictureService _pictureService;
    private ObservableCollection<string> _rules = new();
    private ObservableCollection<QuranAya> _searchResults = new();
    private string? _selectedRule;
    private QuranAya? _selectedAya;
    private int _repeatCount = 1;
    private string _statusMessage = "Ready";
    private double _fontSize = 20;
    private string _ruleDescription = string.Empty;
    private bool _isPlaying = false;
    private bool _isRepeating = false;
    private string? _currentPicturePath;
    private Bitmap? _currentPictureBitmap;

    public MainWindowViewModel()
    {
        _searchService = new QuranSearchService();
        _audioService = new AudioService();
        _pictureService = new PictureService();
        
        // Subscribe to audio service events
        _audioService.PlaybackStateChanged += OnPlaybackStateChanged;
        _audioService.PlaybackError += OnPlaybackError;
        
        SearchCommand = new AsyncCommand(SearchAsync);
        PlaySingleCommand = new SimpleCommand(PlaySingle);
        PlayRepeatCommand = new SimpleCommand(PlayRepeat);
        PlayAllCommand = new SimpleCommand(PlayAll);
        StopCommand = new SimpleCommand(StopPlayback);
        
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
            UpdatePictureForSelectedAya();
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

    public double FontSize
    {
        get => _fontSize;
        set => this.RaiseAndSetIfChanged(ref _fontSize, value);
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

    public ICommand SearchCommand { get; }
    public ICommand PlaySingleCommand { get; }
    public ICommand PlayRepeatCommand { get; }
    public ICommand PlayAllCommand { get; }
    public ICommand StopCommand { get; }

    private async Task LoadDataAsync()
    {
        try
        {
            StatusMessage = "Loading Quran text...";
            await _searchService.LoadQuranTextAsync();
            
            StatusMessage = "Loading rules...";
            await _searchService.LoadRulesAsync();
            
            var ruleNames = _searchService.GetRuleNames();
            Rules = new ObservableCollection<string>(ruleNames);
            StatusMessage = $"Ready - Loaded {ruleNames.Count} rules";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            Rules = new ObservableCollection<string>();
        }
    }

    private async Task SearchAsync()
    {
        if (string.IsNullOrWhiteSpace(SelectedRule))
        {
            StatusMessage = "Please select a rule to search";
            return;
        }

        try
        {
            StatusMessage = "Searching...";
            
            // Run search on background thread
            var results = await Task.Run(() => _searchService.SearchPattern(SelectedRule));
            
            // Update UI on main thread
            SearchResults = new ObservableCollection<QuranAya>(results);
            StatusMessage = $"Found {results.Count} matches";
            
            // Update rule description
            var selectedRuleInfo = _searchService.GetRuleInfo(SelectedRule);
            RuleDescription = selectedRuleInfo != null ? $"Rule: {selectedRuleInfo.Name}" : $"Rule: {SelectedRule}";
            this.RaisePropertyChanged(nameof(HasRuleDescription));
        }
        catch (Exception ex)
        {
            StatusMessage = $"Search error: {ex.Message}";
        }
    }

    private async void PlaySingle()
    {
        if (SelectedAya == null)
        {
            StatusMessage = "Please select an Aya to play";
            return;
        }

        if (!_audioService.AudioFileExists(SelectedAya.SurahNumber, SelectedAya.AyaNumber))
        {
            StatusMessage = $"Audio file not found for Aya {SelectedAya.SurahNumber}:{SelectedAya.AyaNumber}";
            return;
        }
        
        StatusMessage = $"Playing Aya {SelectedAya.SurahNumber}:{SelectedAya.AyaNumber}";
        _audioService.SetRepeatMode(false);
        await _audioService.PlayAyaAsync(SelectedAya.SurahNumber, SelectedAya.AyaNumber);
    }

    private async void PlayRepeat()
    {
        if (SelectedAya == null)
        {
            StatusMessage = "Please select an Aya to repeat";
            return;
        }

        if (!_audioService.AudioFileExists(SelectedAya.SurahNumber, SelectedAya.AyaNumber))
        {
            StatusMessage = $"Audio file not found for Aya {SelectedAya.SurahNumber}:{SelectedAya.AyaNumber}";
            return;
        }
        
        StatusMessage = $"Playing Aya {SelectedAya.SurahNumber}:{SelectedAya.AyaNumber} on repeat";
        _audioService.SetRepeatMode(true);
        await _audioService.PlayAyaAsync(SelectedAya.SurahNumber, SelectedAya.AyaNumber);
    }

    private async void PlayAll()
    {
        if (SearchResults == null || SearchResults.Count == 0)
        {
            StatusMessage = "No search results to play";
            return;
        }
        
        StatusMessage = $"Playing all {SearchResults.Count} found ayas in sequence";
        _audioService.SetRepeatMode(false);
        
        // Play each Aya in sequence
        foreach (var aya in SearchResults)
        {
            if (_audioService.AudioFileExists(aya.SurahNumber, aya.AyaNumber))
            {
                StatusMessage = $"Playing Aya {aya.SurahNumber}:{aya.AyaNumber}";
                await _audioService.PlayAyaAsync(aya.SurahNumber, aya.AyaNumber);
                
                // Wait for current audio to finish before playing next
                while (_audioService.IsPlaying)
                {
                    await Task.Delay(100);
                }
            }
        }
        
        StatusMessage = "Finished playing all ayas";
    }

    private async void StopPlayback()
    {
        await _audioService.StopAsync();
        StatusMessage = "Playback stopped";
    }

    private void OnPlaybackStateChanged(object? sender, bool isPlaying)
    {
        IsPlaying = isPlaying;
        IsRepeating = _audioService.IsRepeating;
    }

    private void OnPlaybackError(object? sender, string error)
    {
        StatusMessage = $"Audio error: {error}";
        IsPlaying = false;
    }

    private void UpdatePictureForSelectedAya()
    {
        if (SelectedAya == null)
        {
            CurrentPicturePath = null;
            CurrentPictureBitmap = null;
            Console.WriteLine("[ViewModel] SelectedAya is null, clearing picture path");
            return;
        }

        Console.WriteLine($"[ViewModel] Updating picture for Aya {SelectedAya.SurahNumber}:{SelectedAya.AyaNumber}");
        
        if (_pictureService.PictureFileExists(SelectedAya.SurahNumber, SelectedAya.AyaNumber))
        {
            CurrentPicturePath = _pictureService.GetPictureFilePath(SelectedAya.SurahNumber, SelectedAya.AyaNumber);
            
            try
            {
                CurrentPictureBitmap = new Bitmap(CurrentPicturePath);
                Console.WriteLine($"[ViewModel] Picture loaded successfully: {CurrentPicturePath}");
            }
            catch (Exception ex)
            {
                CurrentPictureBitmap = null;
                StatusMessage = $"Error loading picture: {ex.Message}";
                Console.WriteLine($"[ViewModel] Error loading picture: {ex.Message}");
            }
        }
        else
        {
            CurrentPicturePath = null;
            CurrentPictureBitmap = null;
            StatusMessage = $"Picture not found for Aya {SelectedAya.SurahNumber}:{SelectedAya.AyaNumber}";
            Console.WriteLine($"[ViewModel] Picture not found for Aya {SelectedAya.SurahNumber}:{SelectedAya.AyaNumber}");
        }
    }
}
