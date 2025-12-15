using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;
using QuranSearch.Core.Models;
using QuranSearch.Core.Services;
using QuranSearch.Core.Interfaces;
using QuranSearch.Core.ViewModels;
using QuranSearchApp.Services;
using FlowDirection = Avalonia.Media.FlowDirection;

namespace QuranSearchApp.ViewModels;

public class RecitationViewModel : ViewModelBase
{
    private readonly IAudioService _audioService;
    private readonly LocalizationService _localizationService;
    private readonly IFileService _fileService;
    
    // Reciters list
    private readonly List<ReciterInfo> _recitersList = new()
    {
        new ReciterInfo("Abdul_Basit_Murattal_192kbps", "Abdul Basit (Murattal)"),
        new ReciterInfo("Ayman_Sowaid_64kbps", "Ayman Sowaid"),
        new ReciterInfo("Husary_128kbps", "Mahmoud Khalil Al-Husary"),
        new ReciterInfo("Husary_Muallim_128kbps", "Al-Husary (Muallim)"),
        new ReciterInfo("Menshawi_32kbps", "Mohamed Siddiq Al-Menshawi"),
        new ReciterInfo("Mohammad_al_Tablaway_128kbps", "Mohammad Al-Tablaway"),
        new ReciterInfo("Mustafa_Ismail_48kbps", "Mustafa Ismail"),
        new ReciterInfo("Muhammad_Ayyoub_128kbps", "Muhammad Ayyoub"),
        new ReciterInfo("mahmoud_ali_al_banna_32kbps", "Mahmoud Ali Al-Banna")
    };
    
    // State
    private int _selectedSurahIndex = 0;
    private int _fromAya = 1;
    private int _toAya = 7;
    private int _currentSurah = 1;
    private int _currentAya = 1;
    private bool _isPlaying = false;
    private bool _needsBasmalah = false;
    private bool _playingBasmalah = false;
    private string _selectedReciter = "Husary_128kbps";
    private string _statusMessage = "";
    private string _currentAyaInfo = "";
    private string _currentAyaText = "";
    private Dictionary<(int, int), string>? _quranText;
    
    // Repeat count state
    private int _repeatCount = 1;
    private int _currentRepeat = 1;
    private int _currentAyaRepeat = 1;
    private bool _repeatEachAya = false; // false = repeat whole range, true = repeat each aya
    private string _selectedRepeatMode = "";
    private bool _teacherModeEnabled = false;
    private DateTime _currentAyaStartTime = DateTime.UtcNow;
    
    // Surah At-Tawbah number (no Basmalah)
    private const int SURAH_TAWBAH = 9;
    // Surah Al-Fatiha number (Basmalah is part of it)
    private const int SURAH_FATIHA = 1;
    
    public RecitationViewModel()
    {
        _fileService = new DesktopFileService();
        _audioService = new AudioService();
        _localizationService = new LocalizationService(_fileService);
        
        // Subscribe to audio events
        _audioService.PlaybackStateChanged += OnPlaybackStateChanged;
        _audioService.SequencePlaybackEnded += OnAyaPlaybackEnded;
        _audioService.PlaybackError += (s, msg) => StatusMessage = msg;
        
        // Subscribe to language changes
        _localizationService.LanguageChanged += OnLanguageChanged;
        
        // Initialize commands
        StartCommand = new SimpleCommand(StartRecitation);
        StopCommand = new SimpleCommand(StopRecitation);
        
        // Initialize repeat options
        InitializeRepeatOptions();
        
        // Initialize collections
        InitializeSurahs();
        UpdateAyaNumbers();
        
        // Load Quran text
        _ = LoadQuranTextAsync();
        
        StatusMessage = _localizationService["Ready"];
    }
    
    public RecitationViewModel(LocalizationService localizationService, string selectedReciter)
    {
        _fileService = new DesktopFileService();
        _audioService = new AudioService();
        _localizationService = localizationService;
        _selectedReciter = selectedReciter;
        
        // Subscribe to audio events
        _audioService.PlaybackStateChanged += OnPlaybackStateChanged;
        _audioService.SequencePlaybackEnded += OnAyaPlaybackEnded;
        _audioService.PlaybackError += (s, msg) => StatusMessage = msg;
        
        // Subscribe to language changes
        _localizationService.LanguageChanged += OnLanguageChanged;
        
        // Initialize commands
        StartCommand = new SimpleCommand(StartRecitation);
        StopCommand = new SimpleCommand(StopRecitation);
        
        // Initialize repeat options
        InitializeRepeatOptions();
        
        // Initialize collections
        InitializeSurahs();
        UpdateAyaNumbers();
        
        // Set reciter
        if (_audioService is AudioService audioService)
        {
            audioService.SelectedReciter = selectedReciter;
        }
        
        // Load Quran text
        _ = LoadQuranTextAsync();
        
        StatusMessage = _localizationService["Ready"];
    }
    
    #region Properties
    
    public ObservableCollection<string> Reciters { get; } = new();
    public ObservableCollection<string> Surahs { get; } = new();
    public ObservableCollection<int> AyaNumbers { get; } = new();
    public ObservableCollection<int> RepeatCountOptions { get; } = new();
    public ObservableCollection<string> RepeatModeOptions { get; } = new();
    
    public int RepeatCount
    {
        get => _repeatCount;
        set
        {
            if (_repeatCount != value)
            {
                _repeatCount = value;
                this.RaisePropertyChanged();
            }
        }
    }
    
    public string SelectedRepeatMode
    {
        get => _selectedRepeatMode;
        set
        {
            if (_selectedRepeatMode != value)
            {
                _selectedRepeatMode = value;
                _repeatEachAya = (value == _localizationService["RepeatEachAya"]);
                this.RaisePropertyChanged();
            }
        }
    }
    
    public string SelectedReciter
    {
        get => _recitersList.Find(r => r.Key == _selectedReciter)?.Name ?? _selectedReciter;
        set
        {
            var reciter = _recitersList.Find(r => r.Name == value);
            if (reciter != null && _selectedReciter != reciter.Key)
            {
                _selectedReciter = reciter.Key;
                if (_audioService is AudioService audioService)
                {
                    audioService.SelectedReciter = reciter.Key;
                }
                this.RaisePropertyChanged();
            }
        }
    }
    
    public string SelectedSurah
    {
        get => Surahs.Count > _selectedSurahIndex ? Surahs[_selectedSurahIndex] : "";
        set
        {
            var index = Surahs.IndexOf(value);
            if (index >= 0 && index != _selectedSurahIndex)
            {
                _selectedSurahIndex = index;
                UpdateAyaNumbers();
                this.RaisePropertyChanged();
            }
        }
    }
    
    public int FromAya
    {
        get => _fromAya;
        set
        {
            if (_fromAya != value)
            {
                _fromAya = value;
                if (_toAya < _fromAya)
                {
                    ToAya = _fromAya;
                }
                this.RaisePropertyChanged();
            }
        }
    }
    
    public int ToAya
    {
        get => _toAya;
        set
        {
            if (_toAya != value)
            {
                _toAya = value;
                if (_fromAya > _toAya)
                {
                    FromAya = _toAya;
                }
                this.RaisePropertyChanged();
            }
        }
    }
    
    public bool IsPlaying
    {
        get => _isPlaying;
        set => this.RaiseAndSetIfChanged(ref _isPlaying, value);
    }
    
    public string StatusMessage
    {
        get => _statusMessage;
        set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
    }
    
    public string CurrentAyaInfo
    {
        get => _currentAyaInfo;
        set => this.RaiseAndSetIfChanged(ref _currentAyaInfo, value);
    }
    
    public string CurrentAyaText
    {
        get => _currentAyaText;
        set => this.RaiseAndSetIfChanged(ref _currentAyaText, value);
    }
    
    public FlowDirection CurrentFlowDirection => _localizationService.IsRightToLeft 
        ? FlowDirection.RightToLeft 
        : FlowDirection.LeftToRight;
    
    public bool TeacherModeEnabled
    {
        get => _teacherModeEnabled;
        set => this.RaiseAndSetIfChanged(ref _teacherModeEnabled, value);
    }
    
    #endregion
    
    #region Localized Labels
    
    public string RecitationModeTitle => _localizationService["RecitationMode"];
    public string SelectReciterLabel => _localizationService["SelectReciter"];
    public string SelectSurahLabel => _localizationService["SelectSurah"];
    public string FromAyaLabel => _localizationService["FromAya"];
    public string ToAyaLabel => _localizationService["ToAya"];
    public string StartRecitationLabel => _localizationService["StartRecitation"];
    public string StopRecitationLabel => _localizationService["StopRecitation"];
    public string RepeatCountLabel => _localizationService["RepeatCount"];
    public string TimesLabel => _localizationService["Times"];
    public string RepeatModeLabel => _localizationService["RepeatMode"];
    public string TeacherModeLabel => _localizationService["TeacherMode"];
    public string NowPlayingLabel => _localizationService["NowPlaying"];
    public string PlayingLabel => _localizationService["Playing"];
    
    #endregion
    
    #region Commands
    
    public ICommand StartCommand { get; }
    public ICommand StopCommand { get; }
    
    #endregion
    
    #region Initialization
    
    private void InitializeSurahs()
    {
        Reciters.Clear();
        foreach (var reciter in _recitersList)
        {
            Reciters.Add(reciter.Name);
        }
        
        Surahs.Clear();
        for (int i = 1; i <= SurahInfo.TotalSurahs; i++)
        {
            Surahs.Add($"{i}. {SurahInfo.GetSurahName(i)}");
        }
    }
    
    private void UpdateAyaNumbers()
    {
        int surahNumber = _selectedSurahIndex + 1;
        int ayaCount = SurahInfo.GetAyaCount(surahNumber);
        
        AyaNumbers.Clear();
        for (int i = 1; i <= ayaCount; i++)
        {
            AyaNumbers.Add(i);
        }
        
        _fromAya = 1;
        _toAya = ayaCount;
        this.RaisePropertyChanged(nameof(FromAya));
        this.RaisePropertyChanged(nameof(ToAya));
    }
    
    private void InitializeRepeatOptions()
    {
        // Initialize repeat count options (1-100)
        RepeatCountOptions.Clear();
        for (int i = 1; i <= 100; i++)
        {
            RepeatCountOptions.Add(i);
        }
        _repeatCount = 1;
        
        // Initialize repeat mode options
        RepeatModeOptions.Clear();
        RepeatModeOptions.Add(_localizationService["RepeatWholeRange"]);
        RepeatModeOptions.Add(_localizationService["RepeatEachAya"]);
        _selectedRepeatMode = _localizationService["RepeatWholeRange"];
        _repeatEachAya = false;
    }
    
    private async Task LoadQuranTextAsync()
    {
        try
        {
            var content = await Task.Run(() => _fileService.ReadAllText("quran-uthmani.txt"));
            var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            
            _quranText = new Dictionary<(int, int), string>();
            foreach (var line in lines)
            {
                var parts = line.Split('|');
                if (parts.Length >= 3)
                {
                    if (int.TryParse(parts[0], out int surah) && int.TryParse(parts[1], out int aya))
                    {
                        _quranText[(surah, aya)] = parts[2];
                    }
                }
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading Quran text: {ex.Message}";
        }
    }
    
    #endregion
    
    #region Playback Control
    
    private void StartRecitation()
    {
        _currentSurah = _selectedSurahIndex + 1;
        _currentAya = _fromAya;
        _currentRepeat = 1;
        _currentAyaRepeat = 1;
        _currentAyaStartTime = DateTime.UtcNow;
        IsPlaying = true;
        
        // Check if we need Basmalah for the first Aya
        _needsBasmalah = ShouldPlayBasmalah(_currentSurah, _currentAya);
        
        PlayCurrentAya();
    }
    
    private async void StopRecitation()
    {
        IsPlaying = false;
        _playingBasmalah = false;
        _currentRepeat = 1;
        _currentAyaRepeat = 1;
        await _audioService.StopAsync();
        
        CurrentAyaInfo = _localizationService["Stopped"];
        StatusMessage = _localizationService["Stopped"];
    }
    
    private bool ShouldPlayBasmalah(int surah, int aya)
    {
        // Basmalah is already included in the audio files for each Surah's first Aya
        // No need to play it separately
        return false;
    }
    
    private async void PlayCurrentAya()
    {
        if (_audioService == null) return;
        
        try
        {
            // If we need to play Basmalah first
            if (_needsBasmalah && !_playingBasmalah)
            {
                _playingBasmalah = true;
                UpdateCurrentAyaDisplay(true);
                
                // Play Basmalah (Surah 1, Aya 1)
                await _audioService.PlayAyaAsync(1, 1);
                return;
            }
            
            _playingBasmalah = false;
            _needsBasmalah = false;
            
            UpdateCurrentAyaDisplay(false);
            _currentAyaStartTime = DateTime.UtcNow;
            await _audioService.PlayAyaAsync(_currentSurah, _currentAya);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Playback error: {ex.Message}";
        }
    }
    
    private void UpdateCurrentAyaDisplay(bool isBasmalah)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            if (isBasmalah)
            {
                CurrentAyaInfo = _localizationService["PlayingBasmala"];
                CurrentAyaText = _localizationService["Basmalah"];
            }
            else
            {
                string surahName = SurahInfo.GetSurahName(_currentSurah);
                var surahLabel = _localizationService["Surah"];
                var ayaLabel = _localizationService["Aya"];
                
                // Add repeat info if repeating more than once
                string repeatInfo = "";
                if (_repeatCount > 1)
                {
                    if (_repeatEachAya)
                    {
                        // Show aya repeat info: "Aya 5 - Repeat 2 of 3"
                        repeatInfo = $" - {string.Format(_localizationService["AyaRepeat"], _currentAya, _currentAyaRepeat, _repeatCount)}";
                        CurrentAyaInfo = $"{surahLabel} {surahName}{repeatInfo}";
                    }
                    else
                    {
                        // Show range repeat info: "Repeat 2 of 3"
                        repeatInfo = $" - {string.Format(_localizationService["CurrentRepeat"], _currentRepeat, _repeatCount)}";
                        CurrentAyaInfo = $"{surahLabel} {surahName} - {ayaLabel} {_currentAya}{repeatInfo}";
                    }
                }
                else
                {
                    CurrentAyaInfo = $"{surahLabel} {surahName} - {ayaLabel} {_currentAya}";
                }
                
                // Get and display the Aya text
                CurrentAyaText = GetAyaText(_currentSurah, _currentAya);
            }
        });
    }
    
    private string GetAyaText(int surah, int aya)
    {
        if (_quranText != null && _quranText.TryGetValue((surah, aya), out var text))
        {
            return text;
        }
        
        return $"سورة {SurahInfo.GetSurahName(surah)} - آية {aya}";
    }
    
    private void MoveToNextAya()
    {
        if (_repeatEachAya)
        {
            // Repeat Each Aya mode
            if (_currentAyaRepeat < _repeatCount)
            {
                // Repeat current aya
                _currentAyaRepeat++;
            }
            else
            {
                // Move to next aya
                _currentAyaRepeat = 1;
                if (_currentAya < _toAya)
                {
                    _currentAya++;
                    _needsBasmalah = ShouldPlayBasmalah(_currentSurah, _currentAya);
                }
                else
                {
                    // All ayas complete
                    IsPlaying = false;
                    var completeText = _localizationService["RecitationComplete"];
                    CurrentAyaInfo = completeText;
                    StatusMessage = completeText;
                }
            }
        }
        else
        {
            // Repeat Whole Range mode
            if (_currentAya < _toAya)
            {
                _currentAya++;
                _needsBasmalah = ShouldPlayBasmalah(_currentSurah, _currentAya);
            }
            else
            {
                // Finished one repeat cycle
                if (_currentRepeat < _repeatCount)
                {
                    // Start next repeat cycle
                    _currentRepeat++;
                    _currentAya = _fromAya;
                    _needsBasmalah = ShouldPlayBasmalah(_currentSurah, _currentAya);
                }
                else
                {
                    // All repeats complete
                    IsPlaying = false;
                    var completeText = _localizationService["RecitationComplete"];
                    CurrentAyaInfo = completeText;
                    StatusMessage = completeText;
                }
            }
        }
    }
    
    #endregion
    
    #region Event Handlers
    
    private void OnPlaybackStateChanged(object? sender, bool isPlaying)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            // Update UI state if needed
        });
    }
    
    private void OnAyaPlaybackEnded(object? sender, EventArgs e)
    {
        HandleAyaPlaybackEndedAsync();
    }
    
    private async void HandleAyaPlaybackEndedAsync()
    {
        if (!IsPlaying) return;
        
        // If we just finished playing Basmalah, now play the actual Aya
        if (_playingBasmalah)
        {
            _playingBasmalah = false;
            _needsBasmalah = false; // Clear the flag so we don't play Basmalah again
            PlayCurrentAya();
            return;
        }
        
        if (_teacherModeEnabled)
        {
            var ayaDuration = DateTime.UtcNow - _currentAyaStartTime;
            if (ayaDuration < TimeSpan.FromSeconds(1))
            {
                ayaDuration = TimeSpan.FromSeconds(1);
            }
            var pauseDuration = ayaDuration + TimeSpan.FromSeconds(1);
            try
            {
                await Task.Delay(pauseDuration);
            }
            catch
            {
                // Ignore delay cancellation
            }
            if (!IsPlaying)
                return;
        }
        
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            if (!IsPlaying) return;
            MoveToNextAya();
            
            if (IsPlaying)
            {
                PlayCurrentAya();
            }
        });
    }
    
    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        // Update all localized properties
        this.RaisePropertyChanged(nameof(RecitationModeTitle));
        this.RaisePropertyChanged(nameof(SelectReciterLabel));
        this.RaisePropertyChanged(nameof(SelectSurahLabel));
        this.RaisePropertyChanged(nameof(FromAyaLabel));
        this.RaisePropertyChanged(nameof(ToAyaLabel));
        this.RaisePropertyChanged(nameof(StartRecitationLabel));
        this.RaisePropertyChanged(nameof(StopRecitationLabel));
        this.RaisePropertyChanged(nameof(RepeatCountLabel));
        this.RaisePropertyChanged(nameof(TimesLabel));
        this.RaisePropertyChanged(nameof(RepeatModeLabel));
        this.RaisePropertyChanged(nameof(TeacherModeLabel));
        this.RaisePropertyChanged(nameof(NowPlayingLabel));
        
        // Update repeat mode options with new language
        var currentMode = _repeatEachAya;
        RepeatModeOptions.Clear();
        RepeatModeOptions.Add(_localizationService["RepeatWholeRange"]);
        RepeatModeOptions.Add(_localizationService["RepeatEachAya"]);
        _selectedRepeatMode = currentMode ? _localizationService["RepeatEachAya"] : _localizationService["RepeatWholeRange"];
        this.RaisePropertyChanged(nameof(SelectedRepeatMode));
        this.RaisePropertyChanged(nameof(PlayingLabel));
        this.RaisePropertyChanged(nameof(CurrentFlowDirection));
    }
    
    #endregion
}

public class ReciterInfo
{
    public string Key { get; }
    public string Name { get; }
    
    public ReciterInfo(string key, string name)
    {
        Key = key;
        Name = name;
    }
}
