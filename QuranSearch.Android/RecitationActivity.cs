using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using QuranSearch.Android.Services;
using QuranSearch.Core.Models;
using QuranSearch.Core.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuranSearch.Android;

[Activity(Label = "@string/recitation_mode", Theme = "@style/AppTheme")]
public class RecitationActivity : AppCompatActivity
{
    // Services
    private AndroidAudioService? _audioService;
    private AndroidPictureService? _pictureService;
    private QuranSearchService? _searchService;
    
    // UI Elements
    private Button? _backButton;
    private Button? _settingsButton;
    private Spinner? _reciterSpinner;
    private Spinner? _surahSpinner;
    private Spinner? _fromAyaSpinner;
    private Spinner? _toAyaSpinner;
    private Button? _startButton;
    private Button? _stopButton;
    private Button? _previousButton;
    private Button? _nextButton;
    private TextView? _currentAyaInfo;
    private TextView? _ayaTextView;
    private ImageView? _ayaImage;
    
    // State
    private int _selectedSurah = 1;
    private int _fromAya = 1;
    private int _toAya = 7;
    private int _currentSurah = 1;
    private int _currentAya = 1;
    private bool _isPlaying = false;
    private bool _needsBasmalah = false;
    private bool _playingBasmalah = false;
    private string _selectedReciter = "Husary_128kbps";
    private List<QuranAya> _quranData = new();
    
    // Reciters list (same as SettingsActivity)
    private readonly List<(string Key, string Name)> _reciters = new()
    {
        ("Abdul_Basit_Murattal_128kbps", "Abdul Basit (Murattal)"),
        ("Ayman_Sowaid_64kbps", "Ayman Sowaid"),
        ("Husary_128kbps", "Mahmoud Khalil Al-Husary"),
        ("Husary_Muallim_128kbps", "Al-Husary (Muallim)"),
        ("Menshawi_32kbps", "Mohamed Siddiq Al-Menshawi"),
        ("Mohammad_al_Tablaway_128kbps", "Mohammad Al-Tablaway"),
        ("Mustafa_Ismail_48kbps", "Mustafa Ismail"),
        ("Muhammad_Ayyoub_128kbps", "Muhammad Ayyoub"),
        ("Mahmoud_Ali_Al_Banna_32kbps", "Mahmoud Ali Al-Banna")
    };
    
    private const int SETTINGS_REQUEST_CODE = 1002;
    
    // Surah At-Tawbah number (no Basmalah)
    private const int SURAH_TAWBAH = 9;
    // Surah Al-Fatiha number (Basmalah is part of it)
    private const int SURAH_FATIHA = 1;
    
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        SetContentView(Resource.Layout.activity_recitation);
        
        InitializeServices();
        InitializeViews();
        LoadSettings();
        SetupSpinners();
        LoadQuranData();
    }
    
    private void InitializeServices()
    {
        try
        {
            var fileService = new AndroidFileService(this);
            _audioService = new AndroidAudioService(this);
            _pictureService = new AndroidPictureService(this);
            _searchService = new QuranSearchService(fileService);
            
            if (_audioService != null)
            {
                _audioService.PlaybackStateChanged += OnPlaybackStateChanged;
                _audioService.SequencePlaybackEnded += OnAyaPlaybackEnded;
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
        _backButton = FindViewById<Button>(Resource.Id.backButton);
        _settingsButton = FindViewById<Button>(Resource.Id.settingsButton);
        _reciterSpinner = FindViewById<Spinner>(Resource.Id.reciterSpinner);
        _surahSpinner = FindViewById<Spinner>(Resource.Id.surahSpinner);
        _fromAyaSpinner = FindViewById<Spinner>(Resource.Id.fromAyaSpinner);
        _toAyaSpinner = FindViewById<Spinner>(Resource.Id.toAyaSpinner);
        _startButton = FindViewById<Button>(Resource.Id.startButton);
        _stopButton = FindViewById<Button>(Resource.Id.stopButton);
        _previousButton = FindViewById<Button>(Resource.Id.previousButton);
        _nextButton = FindViewById<Button>(Resource.Id.nextButton);
        _currentAyaInfo = FindViewById<TextView>(Resource.Id.currentAyaInfo);
        _ayaTextView = FindViewById<TextView>(Resource.Id.ayaTextView);
        _ayaImage = FindViewById<ImageView>(Resource.Id.ayaImage);
        
        // Setup click handlers
        if (_backButton != null)
            _backButton.Click += (s, e) => Finish();
        if (_settingsButton != null)
            _settingsButton.Click += OnSettingsClick;
        if (_startButton != null)
            _startButton.Click += OnStartClick;
        if (_stopButton != null)
            _stopButton.Click += OnStopClick;
        if (_previousButton != null)
            _previousButton.Click += OnPreviousClick;
        if (_nextButton != null)
            _nextButton.Click += OnNextClick;
    }
    
    private void LoadSettings()
    {
        var prefs = GetSharedPreferences("QuranSearchSettings", FileCreationMode.Private);
        
        var useRemoteAudio = prefs?.GetBoolean("UseRemoteAudio", true) ?? true;
        var useRemoteImages = prefs?.GetBoolean("UseRemoteImages", false) ?? false;
        _selectedReciter = prefs?.GetString("SelectedReciter", "Husary_128kbps") ?? "Husary_128kbps";
        
        if (_audioService != null)
        {
            _audioService.UseRemoteSource = useRemoteAudio;
            _audioService.SelectedReciter = _selectedReciter;
        }
        if (_pictureService != null)
        {
            _pictureService.UseRemoteSource = useRemoteImages;
        }
    }
    
    private void SetupSpinners()
    {
        // Setup Reciter Spinner
        var reciterNames = new List<string>();
        int selectedReciterIndex = 0;
        for (int i = 0; i < _reciters.Count; i++)
        {
            reciterNames.Add(_reciters[i].Name);
            if (_reciters[i].Key == _selectedReciter)
                selectedReciterIndex = i;
        }
        
        var reciterAdapter = new ArrayAdapter<string>(this, 
            global::Android.Resource.Layout.SimpleSpinnerItem, reciterNames);
        reciterAdapter.SetDropDownViewResource(global::Android.Resource.Layout.SimpleSpinnerDropDownItem);
        if (_reciterSpinner != null)
        {
            _reciterSpinner.Adapter = reciterAdapter;
            _reciterSpinner.SetSelection(selectedReciterIndex);
            _reciterSpinner.ItemSelected += OnReciterSelected;
        }
        
        // Setup Surah Spinner
        var surahNames = new List<string>();
        for (int i = 1; i <= SurahInfo.TotalSurahs; i++)
        {
            surahNames.Add($"{i}. {SurahInfo.GetSurahName(i)}");
        }
        
        var surahAdapter = new ArrayAdapter<string>(this,
            global::Android.Resource.Layout.SimpleSpinnerItem, surahNames);
        surahAdapter.SetDropDownViewResource(global::Android.Resource.Layout.SimpleSpinnerDropDownItem);
        if (_surahSpinner != null)
        {
            _surahSpinner.Adapter = surahAdapter;
            _surahSpinner.ItemSelected += OnSurahSelected;
        }
        
        // Initial Aya spinners setup
        UpdateAyaSpinners();
    }
    
    private void UpdateAyaSpinners()
    {
        int ayaCount = SurahInfo.GetAyaCount(_selectedSurah);
        var ayaNumbers = new List<string>();
        for (int i = 1; i <= ayaCount; i++)
        {
            ayaNumbers.Add(i.ToString());
        }
        
        var ayaAdapter = new ArrayAdapter<string>(this,
            global::Android.Resource.Layout.SimpleSpinnerItem, ayaNumbers);
        ayaAdapter.SetDropDownViewResource(global::Android.Resource.Layout.SimpleSpinnerDropDownItem);
        
        if (_fromAyaSpinner != null)
        {
            _fromAyaSpinner.Adapter = ayaAdapter;
            _fromAyaSpinner.SetSelection(0); // First aya
            _fromAyaSpinner.ItemSelected += OnFromAyaSelected;
        }
        
        // Clone adapter for toAya spinner
        var toAyaAdapter = new ArrayAdapter<string>(this,
            global::Android.Resource.Layout.SimpleSpinnerItem, ayaNumbers);
        toAyaAdapter.SetDropDownViewResource(global::Android.Resource.Layout.SimpleSpinnerDropDownItem);
        
        if (_toAyaSpinner != null)
        {
            _toAyaSpinner.Adapter = toAyaAdapter;
            _toAyaSpinner.SetSelection(ayaCount - 1); // Last aya
            _toAyaSpinner.ItemSelected += OnToAyaSelected;
        }
        
        _fromAya = 1;
        _toAya = ayaCount;
    }
    
    private async void LoadQuranData()
    {
        try
        {
            if (_searchService != null)
            {
                await _searchService.LoadQuranTextAsync();
            }
        }
        catch (Exception ex)
        {
            ShowError($"Failed to load Quran data: {ex.Message}");
        }
    }
    
    private void OnReciterSelected(object? sender, AdapterView.ItemSelectedEventArgs e)
    {
        if (e.Position >= 0 && e.Position < _reciters.Count)
        {
            _selectedReciter = _reciters[e.Position].Key;
            if (_audioService != null)
            {
                _audioService.SelectedReciter = _selectedReciter;
            }
            
            // Save to preferences
            var prefs = GetSharedPreferences("QuranSearchSettings", FileCreationMode.Private);
            var editor = prefs?.Edit();
            editor?.PutString("SelectedReciter", _selectedReciter);
            editor?.Apply();
        }
    }
    
    private void OnSurahSelected(object? sender, AdapterView.ItemSelectedEventArgs e)
    {
        _selectedSurah = e.Position + 1; // 1-based
        UpdateAyaSpinners();
    }
    
    private void OnFromAyaSelected(object? sender, AdapterView.ItemSelectedEventArgs e)
    {
        _fromAya = e.Position + 1; // 1-based
        
        // Ensure toAya is not less than fromAya
        if (_toAya < _fromAya)
        {
            _toAya = _fromAya;
            _toAyaSpinner?.SetSelection(_toAya - 1);
        }
    }
    
    private void OnToAyaSelected(object? sender, AdapterView.ItemSelectedEventArgs e)
    {
        _toAya = e.Position + 1; // 1-based
        
        // Ensure fromAya is not greater than toAya
        if (_fromAya > _toAya)
        {
            _fromAya = _toAya;
            _fromAyaSpinner?.SetSelection(_fromAya - 1);
        }
    }
    
    private void OnStartClick(object? sender, EventArgs e)
    {
        StartRecitation();
    }
    
    private void OnStopClick(object? sender, EventArgs e)
    {
        StopRecitation();
    }
    
    private void OnPreviousClick(object? sender, EventArgs e)
    {
        PlayPreviousAya();
    }
    
    private void OnNextClick(object? sender, EventArgs e)
    {
        PlayNextAya();
    }
    
    private void StartRecitation()
    {
        _currentSurah = _selectedSurah;
        _currentAya = _fromAya;
        _isPlaying = true;
        
        // Check if we need Basmalah for the first Aya
        _needsBasmalah = ShouldPlayBasmalah(_currentSurah, _currentAya, true);
        
        UpdateButtonStates();
        PlayCurrentAya();
    }
    
    private void StopRecitation()
    {
        _isPlaying = false;
        _playingBasmalah = false;
        _audioService?.StopAsync();
        UpdateButtonStates();
        
        if (_currentAyaInfo != null)
            _currentAyaInfo.Text = GetString(Resource.String.stopped);
    }
    
    private void PlayPreviousAya()
    {
        if (_currentAya > _fromAya)
        {
            _currentAya--;
        }
        else if (_currentSurah > _selectedSurah)
        {
            // Go to previous surah's last aya in range
            _currentSurah--;
            _currentAya = SurahInfo.GetAyaCount(_currentSurah);
        }
        
        _needsBasmalah = false; // Don't play Basmalah when navigating manually
        PlayCurrentAya();
    }
    
    private void PlayNextAya()
    {
        MoveToNextAya();
        if (_isPlaying)
        {
            _needsBasmalah = false; // Don't play Basmalah when navigating manually
            PlayCurrentAya();
        }
    }
    
    private bool ShouldPlayBasmalah(int surah, int aya, bool isStartOfRecitation)
    {
        // Don't play Basmalah for Surah At-Tawbah (9)
        if (surah == SURAH_TAWBAH)
            return false;
        
        // Don't play Basmalah for Al-Fatiha (it's part of the Surah)
        if (surah == SURAH_FATIHA)
            return false;
        
        // Play Basmalah only for first Aya of a Surah
        if (aya != 1)
            return false;
        
        return true;
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
            await _audioService.PlayAyaAsync(_currentSurah, _currentAya);
            
            // Show the Aya image
            ShowAyaImage(_currentSurah, _currentAya);
        }
        catch (Exception ex)
        {
            ShowError($"Playback error: {ex.Message}");
        }
    }
    
    private void UpdateCurrentAyaDisplay(bool isBasmalah)
    {
        RunOnUiThread(() =>
        {
            if (isBasmalah)
            {
                if (_currentAyaInfo != null)
                    _currentAyaInfo.Text = GetString(Resource.String.playing_basmalah);
                if (_ayaTextView != null)
                    _ayaTextView.Text = GetString(Resource.String.basmalah);
            }
            else
            {
                string surahName = SurahInfo.GetSurahName(_currentSurah);
                if (_currentAyaInfo != null)
                    _currentAyaInfo.Text = string.Format(GetString(Resource.String.surah_aya_format), 
                        surahName, _currentAya);
                
                // Get and display the Aya text
                var ayaText = GetAyaText(_currentSurah, _currentAya);
                if (_ayaTextView != null)
                    _ayaTextView.Text = ayaText;
            }
        });
    }
    
    private string GetAyaText(int surah, int aya)
    {
        // Try to get from loaded Quran data
        // For now, return a placeholder - the actual text would come from the search service
        try
        {
            var fileService = new AndroidFileService(this);
            var content = fileService.ReadAllText("quran-uthmani.txt");
            var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var line in lines)
            {
                var parts = line.Split('|');
                if (parts.Length >= 3)
                {
                    if (int.TryParse(parts[0], out int s) && int.TryParse(parts[1], out int a))
                    {
                        if (s == surah && a == aya)
                            return parts[2];
                    }
                }
            }
        }
        catch { }
        
        return $"سورة {SurahInfo.GetSurahName(surah)} - آية {aya}";
    }
    
    private async void ShowAyaImage(int surah, int aya)
    {
        try
        {
            if (_pictureService == null || _ayaImage == null)
                return;
            
            var ok = await _pictureService.EnsurePictureAvailableAsync(surah, aya);
            var path = _pictureService.GetPicturePath(surah, aya);
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
                }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Image load failed: {ex.Message}");
        }
    }
    
    private void OnPlaybackStateChanged(object? sender, bool isPlaying)
    {
        RunOnUiThread(() =>
        {
            UpdateButtonStates();
        });
    }
    
    private void OnAyaPlaybackEnded(object? sender, EventArgs e)
    {
        RunOnUiThread(() =>
        {
            if (!_isPlaying) return;
            
            // If we just finished playing Basmalah, now play the actual Aya
            if (_playingBasmalah)
            {
                _playingBasmalah = false;
                PlayCurrentAya();
                return;
            }
            
            // Move to next Aya
            MoveToNextAya();
            
            if (_isPlaying)
            {
                PlayCurrentAya();
            }
        });
    }
    
    private void MoveToNextAya()
    {
        int maxAyaInCurrentSurah = (_currentSurah == _selectedSurah) ? _toAya : SurahInfo.GetAyaCount(_currentSurah);
        
        if (_currentAya < maxAyaInCurrentSurah)
        {
            _currentAya++;
        }
        else
        {
            // Check if we should continue to next Surah
            // For now, we only play within the selected Surah range
            if (_currentSurah == _selectedSurah && _currentAya >= _toAya)
            {
                // Recitation complete
                _isPlaying = false;
                UpdateButtonStates();
                
                if (_currentAyaInfo != null)
                    _currentAyaInfo.Text = GetString(Resource.String.recitation_complete);
                
                Toast.MakeText(this, GetString(Resource.String.recitation_complete), ToastLength.Short)?.Show();
                return;
            }
            
            // Move to next Surah
            _currentSurah++;
            _currentAya = 1;
            
            // Check if we need Basmalah for the new Surah
            _needsBasmalah = ShouldPlayBasmalah(_currentSurah, _currentAya, false);
        }
    }
    
    private void UpdateButtonStates()
    {
        bool isAudioPlaying = _audioService?.IsPlaying ?? false;
        
        if (_startButton != null)
            _startButton.Enabled = !_isPlaying;
        if (_stopButton != null)
            _stopButton.Enabled = _isPlaying;
        if (_previousButton != null)
            _previousButton.Enabled = _isPlaying;
        if (_nextButton != null)
            _nextButton.Enabled = _isPlaying;
    }
    
    private void OnSettingsClick(object? sender, EventArgs e)
    {
        var intent = new Intent(this, typeof(SettingsActivity));
        StartActivityForResult(intent, SETTINGS_REQUEST_CODE);
    }
    
    protected override void OnActivityResult(int requestCode, Result resultCode, Intent? data)
    {
        base.OnActivityResult(requestCode, resultCode, data);
        
        if (requestCode == SETTINGS_REQUEST_CODE && resultCode == Result.Ok)
        {
            LoadSettings();
            
            // Update reciter spinner selection
            for (int i = 0; i < _reciters.Count; i++)
            {
                if (_reciters[i].Key == _selectedReciter)
                {
                    _reciterSpinner?.SetSelection(i);
                    break;
                }
            }
            
            Toast.MakeText(this, "Settings applied", ToastLength.Short)?.Show();
        }
    }
    
    private void ShowError(string message)
    {
        RunOnUiThread(() =>
        {
            Toast.MakeText(this, message, ToastLength.Long)?.Show();
        });
    }
    
    protected override void OnDestroy()
    {
        _audioService?.Dispose();
        base.OnDestroy();
    }
}
