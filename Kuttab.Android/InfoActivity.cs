using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using Kuttab.Core.Services;
using Kuttab.Android.Utils;
using Kuttab.Android.Services;

namespace Kuttab.Android;

[Activity(Label = "@string/info_title", Theme = "@style/AppTheme")]
public class InfoActivity : AppCompatActivity
{
    private LocalizationService? _localizationService;
    
    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        
        // Handle edge-to-edge on Android 15+ (SDK 35)
        if (Window != null)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                Window.SetStatusBarColor(global::Android.Graphics.Color.ParseColor("#0D3F13"));
                Window.SetNavigationBarColor(global::Android.Graphics.Color.ParseColor("#1B5E20"));
            }
        }
        
        // Initialize localization service and apply language setting
        var fileService = new AndroidFileService(this);
        _localizationService = new LocalizationService(fileService);
        LoadSettings();
        ApplyLanguageSettings();
        
        SetContentView(Resource.Layout.activity_info);
        
        // Set version dynamically from package info
        var versionTextView = FindViewById<TextView>(Resource.Id.appVersionTextView);
        if (versionTextView != null)
        {
            try
            {
                var packageInfo = PackageManager?.GetPackageInfo(PackageName ?? "", 0);
                var versionName = packageInfo?.VersionName ?? "0.0";
                versionTextView.Text = $"v{versionName}";
            }
            catch
            {
                // Keep default from layout
            }
        }
        
        // Set up action bar
        if (SupportActionBar != null)
        {
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);
        }
    }
    
    private void LoadSettings()
    {
        var prefs = GetSharedPreferences("QuranSearchSettings", FileCreationMode.Private);
        
        // Load language with system language detection
        var language = LanguageHelper.GetLanguageFromPreferences(this);
        if (_localizationService != null)
        {
            _localizationService.CurrentLanguage = language;
        }
    }
    
    private void ApplyLanguageSettings()
    {
        if (_localizationService == null) return;
        
        try
        {
            var languageCode = _localizationService.CurrentLanguage;
            
            // Set locale for the activity
            var locale = new Java.Util.Locale(languageCode);
            Java.Util.Locale.Default = locale;
            
            var config = new Configuration();
            config.SetLocale(locale);
            
            // Apply configuration to resources
            BaseContext.Resources.UpdateConfiguration(config, BaseContext.Resources.DisplayMetrics);
            
            // Update layout direction
            var layoutDirection = languageCode == "ar" ? LayoutDirection.Rtl : LayoutDirection.Ltr;
            Window.DecorView.LayoutDirection = layoutDirection;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error applying language settings: {ex.Message}");
        }
    }
}
