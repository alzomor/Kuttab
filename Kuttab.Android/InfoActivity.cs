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
        
        // Share button
        var shareButton = FindViewById<Button>(Resource.Id.shareAppInfoButton);
        if (shareButton != null)
        {
            shareButton.Text = _localizationService?.ShareAppInfo ?? "\uD83D\uDCE4 Share App";
            shareButton.Click += (s, e) => ShareAppInfo();
        }
    }
    
    private void ShareAppInfo()
    {
        try
        {
            var loc = _localizationService;
            var versionName = "0.0";
            try
            {
                var packageInfo = PackageManager?.GetPackageInfo(PackageName ?? "", 0);
                versionName = packageInfo?.VersionName ?? "0.0";
            }
            catch { }
            
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"\uD83D\uDCD6 {loc?.InfoTitle ?? "Kuttab"}");
            sb.AppendLine($"v{versionName}");
            sb.AppendLine();
            sb.AppendLine(loc?.AppSubtitle ?? "Tajweed Pattern Search Application");
            sb.AppendLine();
            sb.AppendLine("\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501");
            sb.AppendLine();
            sb.AppendLine("\u2022 " + (loc?.ShowTajweedRules ?? "Tajweed rule highlighting"));
            sb.AppendLine("\u2022 " + (loc?.SelectTajweedRule ?? "Search Tajweed rules across the full Quran"));
            sb.AppendLine("\u2022 " + (loc?.OpenRecitation ?? "Recitation mode with audio"));
            sb.AppendLine("\u2022 " + (loc?.QuranicPageView ?? "Quranic page view"));
            sb.AppendLine("\u2022 " + (loc?.OnlineAudio ?? "Online audio recitation"));
            sb.AppendLine();
            sb.AppendLine("\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501\u2501");
            sb.AppendLine();
            sb.AppendLine($"\uD83D\uDCE7 {loc?.ContactEmail ?? "Contact:"} {loc?.ContactEmailText ?? "almanar.backup@gmail.com"}");
            sb.AppendLine();
            sb.AppendLine("\uD83D\uDCF1 https://play.google.com/store/apps/details?id=com.kuttab.app");
            
            var shareIntent = new Intent(Intent.ActionSend);
            shareIntent.SetType("text/plain");
            shareIntent.PutExtra(Intent.ExtraText, sb.ToString());
            shareIntent.PutExtra(Intent.ExtraSubject, loc?.InfoTitle ?? "Kuttab");
            
            var chooserTitle = loc?.ShareAppInfoChooser ?? "Share app via";
            var chooserIntent = Intent.CreateChooser(shareIntent, chooserTitle);
            if (chooserIntent != null)
                StartActivity(chooserIntent);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error sharing app info: {ex.Message}");
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
