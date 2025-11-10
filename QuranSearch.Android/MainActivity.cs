using Android.App;
using Android.OS;
using Android.Widget;
using Android.Graphics;
using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.RecyclerView.Widget;
using QuranSearch.Android.Adapters;
using QuranSearch.Android.Services;
using QuranSearch.Core.Services;
using QuranSearch.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuranSearch.Android;

[Activity(Label = "@string/app_name", MainLauncher = true, Theme = "@style/SplashTheme")]
public class MainActivity : AppCompatActivity
{
    private QuranSearchService? _searchService;
    private AndroidAudioService? _audioService;
    private AndroidPictureService? _pictureService;
    private LocalizationService? _localizationService;
    
    private Spinner? _languageSpinner;
    private Spinner? _ruleSpinner;
    private Button? _searchButton;
    private Button? _playButton;
    private Button? _playRepeatButton;
    private Button? _playAllButton;
    private Button? _stopButton;
    private CheckBox? _useRemoteAudioCheckBox;
    private TextView? _statusText;
    private ImageView? _ayaImage;
    private RecyclerView? _recyclerView;
    private AyaAdapter? _adapter;
    
    private List<QuranAya> _searchResults = new();
    private int _currentPlayingIndex = -1;
    private bool _isPlayingSequence = false;
    
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        // Switch from splash theme to app theme
        SetTheme(Resource.Style.AppTheme);
        
        base.OnCreate(savedInstanceState);
        SetContentView(Resource.Layout.activity_main);
        
        InitializeServices();
        InitializeViews();
        SetupRecyclerView();
        LoadDataAsync();
    }
    
    private void InitializeServices()
    {
        try
        {
            var fileService = new AndroidFileService(this);
            _searchService = new QuranSearchService(fileService);
            _audioService = new AndroidAudioService(this);
            _pictureService = new AndroidPictureService(this);
            _localizationService = new LocalizationService(fileService);
            
            // Setup audio event handlers
            if (_audioService != null)
            {
                _audioService.PlaybackStateChanged += OnPlaybackStateChanged;
                _audioService.SequencePlaybackEnded += OnSequencePlaybackEnded;
                _audioService.PlaybackError += (s, msg) => ShowError(msg);
            }
        }
        catch (Exception ex)
        {
            ShowError($"Failed to initialize services: {ex.Message}");
        }
    }
    
    private void InitializeViews()
    {
        _languageSpinner = FindViewById<Spinner>(Resource.Id.languageSpinner);
        _ruleSpinner = FindViewById<Spinner>(Resource.Id.ruleSpinner);
        _searchButton = FindViewById<Button>(Resource.Id.searchButton);
        _playButton = FindViewById<Button>(Resource.Id.playButton);
        _playRepeatButton = FindViewById<Button>(Resource.Id.playRepeatButton);
        _playAllButton = FindViewById<Button>(Resource.Id.playAllButton);
        _stopButton = FindViewById<Button>(Resource.Id.stopButton);
        _useRemoteAudioCheckBox = FindViewById<CheckBox>(Resource.Id.useRemoteAudioCheckBox);
        _statusText = FindViewById<TextView>(Resource.Id.statusText);
        _ayaImage = FindViewById<ImageView>(Resource.Id.ayaImage);
        _recyclerView = FindViewById<RecyclerView>(Resource.Id.recyclerView);
        
        // Setup button click handlers
        if (_searchButton != null)
            _searchButton.Click += OnSearchClick;
        if (_playButton != null)
            _playButton.Click += OnPlayClick;
        if (_playRepeatButton != null)
            _playRepeatButton.Click += OnPlayRepeatClick;
        if (_playAllButton != null)
            _playAllButton.Click += OnPlayAllClick;
        if (_stopButton != null)
            _stopButton.Click += OnStopClick;
        if (_useRemoteAudioCheckBox != null)
            _useRemoteAudioCheckBox.CheckedChange += OnUseRemoteAudioChanged;
        
        // Setup language spinner
        if (_languageSpinner != null)
        {
            var languages = new[] { "العربية", "English", "Deutsch" };
            var languageAdapter = new ArrayAdapter<string>(this, 
                global::Android.Resource.Layout.SimpleSpinnerItem, languages);
            languageAdapter.SetDropDownViewResource(global::Android.Resource.Layout.SimpleSpinnerDropDownItem);
            _languageSpinner.Adapter = languageAdapter;
            _languageSpinner.ItemSelected += OnLanguageSelected;
        }
    }
    
    private void SetupRecyclerView()
    {
        if (_recyclerView != null)
        {
            _recyclerView.SetLayoutManager(new LinearLayoutManager(this));
            _adapter = new AyaAdapter();
            _adapter.ItemClick += OnAyaItemClick;
            _recyclerView.SetAdapter(_adapter);
        }
    }
    
    private async void LoadDataAsync()
    {
        try
        {
            UpdateStatus(GetString(Resource.String.loading));
            
            if (_searchService != null)
            {
                await _searchService.LoadQuranTextAsync();
                await _searchService.LoadRulesAsync();
                
                // Populate rules spinner
                var rules = _searchService.GetRuleNames();
                if (_ruleSpinner != null && rules.Count > 0)
                {
                    var ruleAdapter = new ArrayAdapter<string>(this, 
                        global::Android.Resource.Layout.SimpleSpinnerItem, rules);
                    ruleAdapter.SetDropDownViewResource(global::Android.Resource.Layout.SimpleSpinnerDropDownItem);
                    _ruleSpinner.Adapter = ruleAdapter;
                }
                
                UpdateStatus(GetString(Resource.String.ready));
            }
        }
        catch (Exception ex)
        {
            ShowError($"{GetString(Resource.String.error_loading)}: {ex.Message}");
        }
    }
    
    private async void OnSearchClick(object? sender, EventArgs e)
    {
        try
        {
            if (_searchService == null || _ruleSpinner == null) return;
            
            var selectedRule = _ruleSpinner.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selectedRule)) return;
            
            UpdateStatus(GetString(Resource.String.searching));
            
            _searchResults = await Task.Run(() => _searchService.SearchByRuleName(selectedRule));
            
            _adapter?.UpdateData(_searchResults);
            
            var matchCount = _searchResults.Count;
            // Use Android formatting (resource uses %d)
            UpdateStatus(GetString(Resource.String.found_matches, matchCount));
            
            // Enable audio buttons if there are results
            UpdateAudioButtonsState(matchCount > 0);
        }
        catch (Exception ex)
        {
            ShowError($"Search failed: {ex.Message}");
        }
    }
    
    private void OnAyaItemClick(object? sender, int position)
    {
        try
        {
            UpdateStatus($"Clicked on item at position: {position}");
            _currentPlayingIndex = position;
            if (position >= 0 && position < _searchResults.Count)
            {
                ShowAyaImage(_searchResults[position]);
            }
            PlayCurrentAya(false);
        }
        catch (Exception ex)
        {
            UpdateStatus($"Click error: {ex.Message}");
        }
    }
    
    private void OnPlayClick(object? sender, EventArgs e)
    {
        if (_currentPlayingIndex >= 0 && _currentPlayingIndex < _searchResults.Count)
        {
            PlayCurrentAya(false);
        }
        else if (_searchResults.Count > 0)
        {
            _currentPlayingIndex = 0;
            PlayCurrentAya(false);
        }
    }
    
    private void OnPlayRepeatClick(object? sender, EventArgs e)
    {
        if (_currentPlayingIndex >= 0 && _currentPlayingIndex < _searchResults.Count)
        {
            PlayCurrentAya(true);
        }
        else if (_searchResults.Count > 0)
        {
            _currentPlayingIndex = 0;
            PlayCurrentAya(true);
        }
    }
    
    private void OnPlayAllClick(object? sender, EventArgs e)
    {
        if (_searchResults.Count > 0)
        {
            _isPlayingSequence = true;
            _currentPlayingIndex = 0;
            PlayCurrentAya(false);
        }
    }
    
    private void OnStopClick(object? sender, EventArgs e)
    {
        _audioService?.StopPlayback();
        _isPlayingSequence = false;
        UpdateStatus(GetString(Resource.String.stopped));
    }
    
    private void OnUseRemoteAudioChanged(object? sender, CompoundButton.CheckedChangeEventArgs e)
    {
        if (_audioService != null)
        {
            _audioService.UseRemoteSource = e.IsChecked;
        }
    }
    
    private void OnLanguageSelected(object? sender, AdapterView.ItemSelectedEventArgs e)
    {
        var languages = new[] { "ar", "en", "de" };
        if (e.Position >= 0 && e.Position < languages.Length && _localizationService != null)
        {
            _localizationService.CurrentLanguage = languages[e.Position];
            UpdateUIStringsForCurrentLanguage();
        }
    }
    
    private void UpdateUIStringsForCurrentLanguage()
    {
        if (_localizationService == null) return;
        
        try
        {
            // Update button texts using LocalizationService
            if (_searchButton != null)
                _searchButton.Text = _localizationService.GetString("Search");
            if (_playButton != null)
                _playButton.Text = _localizationService.GetString("Play");
            if (_playRepeatButton != null)
                _playRepeatButton.Text = _localizationService.GetString("PlayRepeat");
            if (_playAllButton != null)
                _playAllButton.Text = _localizationService.GetString("PlayAll");
            if (_stopButton != null)
                _stopButton.Text = _localizationService.GetString("Stop");
            if (_useRemoteAudioCheckBox != null)
                _useRemoteAudioCheckBox.Text = _localizationService.GetString("UseRemoteAudio");
            
            // Update status
            UpdateStatus(_localizationService.GetString("Ready"));
            
            // Reload rules spinner with localized names if available
            if (_searchService != null && _ruleSpinner != null)
            {
                var rules = _searchService.GetRuleNames();
                if (rules.Count > 0)
                {
                    var ruleAdapter = new ArrayAdapter<string>(this,
                        global::Android.Resource.Layout.SimpleSpinnerItem, rules);
                    ruleAdapter.SetDropDownViewResource(global::Android.Resource.Layout.SimpleSpinnerDropDownItem);
                    _ruleSpinner.Adapter = ruleAdapter;
                }
            }
        }
        catch (Exception ex)
        {
            ShowError($"Error updating UI strings: {ex.Message}");
        }
    }
    
    private async Task PlayCurrentAya(bool repeat)
    {
        try
        {
            UpdateStatus($"PlayCurrentAya called: index={_currentPlayingIndex}, repeat={repeat}, results.Count={_searchResults.Count}");
            
            if (_audioService == null || _currentPlayingIndex < 0 || _currentPlayingIndex >= _searchResults.Count)
            {
                UpdateStatus($"Cannot play: audioService={_audioService != null}, index={_currentPlayingIndex}, count={_searchResults.Count}");
                return;
            }
            
            var aya = _searchResults[_currentPlayingIndex];
            _audioService.SetRepeatMode(repeat);
            
            UpdateStatus($"Playing: {aya.SurahNumber}:{aya.AyaNumber}");
            await _audioService.PlayAyaAsync(aya.SurahNumber, aya.AyaNumber);
            UpdateStatus($"{GetString(Resource.String.playing)} {aya.SurahNumber}:{aya.AyaNumber}");
        }
        catch (Exception ex)
        {
            ShowError($"Playback failed: {ex.Message}");
        }
    }
    
    private void OnPlaybackStateChanged(object? sender, bool isPlaying)
    {
        RunOnUiThread(() =>
        {
            if (_audioService != null && !_audioService.IsPlaying && _isPlayingSequence)
            {
                // Play next in sequence
                _currentPlayingIndex++;
                if (_currentPlayingIndex < _searchResults.Count)
                {
                    PlayCurrentAya(false);
                }
                else
                {
                    _isPlayingSequence = false;
                    UpdateStatus(GetString(Resource.String.ready));
                }
            }
            
            UpdateAudioButtonsState(true);
        });
    }
    
    private async void ShowAyaImage(QuranAya aya)
    {
        try
        {
            if (_pictureService == null || _ayaImage == null)
                return;
            // Ensure image exists locally (download if allowed and online)
            var ok = await _pictureService.EnsurePictureAvailableAsync(aya.SurahNumber, aya.AyaNumber);
            var path = _pictureService.GetPicturePath(aya.SurahNumber, aya.AyaNumber);
            var bmp = await _pictureService.LoadBitmapAsync(path) as Bitmap;
            RunOnUiThread(() =>
            {
                if (bmp != null)
                {
                    _ayaImage.SetImageBitmap(bmp);
                    _ayaImage.Visibility = ViewStates.Visible;
                }
                else
                {
                    _ayaImage.Visibility = ViewStates.Gone;
                    if (!ok)
                        UpdateStatus("Image not available (offline or cannot download).");
                }
            });
        }
        catch (Exception ex)
        {
            ShowError($"Image load failed: {ex.Message}");
        }
    }
    
    private void OnSequencePlaybackEnded(object? sender, EventArgs e)
    {
        RunOnUiThread(() =>
        {
            if (_isPlayingSequence)
            {
                _currentPlayingIndex++;
                if (_currentPlayingIndex < _searchResults.Count)
                {
                    PlayCurrentAya(false);
                }
                else
                {
                    _isPlayingSequence = false;
                    UpdateStatus(GetString(Resource.String.ready));
                }
            }
        });
    }
    
    private string GetRemoteAudioUrl(int surah, int aya)
    {
        var paddedSurah = surah.ToString("D3");
        var paddedAya = aya.ToString("D3");
        return $"https://everyayah.com/data/Husary_128kbps/{paddedSurah}{paddedAya}.mp3";
    }
    
    private string GetLocalAudioPath(int surah, int aya)
    {
        var paddedSurah = surah.ToString("D3");
        var paddedAya = aya.ToString("D3");
        return $"AL Husary/000_versebyverse/{paddedSurah}{paddedAya}.mp3";
    }
    
    private void UpdateStatus(string message)
    {
        RunOnUiThread(() =>
        {
            if (_statusText != null)
            {
                _statusText.Text = message;
            }
        });
    }
    
    private void ShowError(string message)
    {
        RunOnUiThread(() =>
        {
            Toast.MakeText(this, message, ToastLength.Long)?.Show();
            UpdateStatus(message);
        });
    }
    
    private void UpdateAudioButtonsState(bool hasResults)
    {
        var isPlaying = _audioService?.IsPlaying ?? false;
        
        if (_playButton != null)
            _playButton.Enabled = hasResults && !isPlaying;
        if (_playRepeatButton != null)
            _playRepeatButton.Enabled = hasResults && !isPlaying;
        if (_playAllButton != null)
            _playAllButton.Enabled = hasResults && !isPlaying;
        if (_stopButton != null)
            _stopButton.Enabled = isPlaying;
    }
    
    protected override void OnDestroy()
    {
        _audioService?.Dispose();
        base.OnDestroy();
    }
}