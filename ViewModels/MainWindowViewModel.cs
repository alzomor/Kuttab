using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;
using QuranSearchApp.Models;
using QuranSearchApp.Services;

namespace QuranSearchApp.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly QuranSearchService _searchService;
    private ObservableCollection<string> _rules = new();
    private ObservableCollection<QuranAya> _searchResults = new();
    private string? _selectedRule;
    private QuranAya? _selectedAya;
    private int _repeatCount = 1;
    private string _statusMessage = "Ready";
    private double _fontSize = 20;
    private string _ruleDescription = string.Empty;

    public MainWindowViewModel()
    {
        _searchService = new QuranSearchService();
        
        SearchCommand = new AsyncCommand(SearchAsync);
        PlaySingleCommand = new SimpleCommand(PlaySingle);
        PlayRepeatCommand = new SimpleCommand(PlayRepeat);
        PlayAllCommand = new SimpleCommand(PlayAll);
        
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
        set => this.RaiseAndSetIfChanged(ref _selectedAya, value);
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

    public bool HasRuleDescription => !string.IsNullOrWhiteSpace(RuleDescription);

    public ICommand SearchCommand { get; }
    public ICommand PlaySingleCommand { get; }
    public ICommand PlayRepeatCommand { get; }
    public ICommand PlayAllCommand { get; }

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

    private void PlaySingle()
    {
        if (SelectedAya == null)
        {
            StatusMessage = "Please select an Aya to play";
            return;
        }
        
        StatusMessage = $"Playing Aya {SelectedAya.SurahNumber}:{SelectedAya.AyaNumber}";
        
        // TODO: Implement actual audio playback
    }

    private void PlayRepeat()
    {
        if (SelectedAya == null)
        {
            StatusMessage = "Please select an Aya to repeat";
            return;
        }
        
        StatusMessage = $"Playing Aya {SelectedAya.SurahNumber}:{SelectedAya.AyaNumber} {RepeatCount} times";
        
        // TODO: Implement repeat playback
    }

    private void PlayAll()
    {
        if (SearchResults == null || SearchResults.Count == 0)
        {
            StatusMessage = "No search results to play";
            return;
        }
        
        StatusMessage = $"Playing all {SearchResults.Count} found ayas in sequence";
        
        // TODO: Implement sequential playback of all found ayas
    }
}
