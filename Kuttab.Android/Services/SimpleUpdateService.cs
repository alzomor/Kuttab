using Android.App;
using Android.Content;
using Android.OS;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Kuttab.Android.Services;

public class SimpleUpdateService
{
    private const string TAG = "SimpleUpdateService";
    private const string UPDATE_CHECK_URL = "https://api.github.com/repos/alzomor/Kuttab/releases/latest";
    private const string PREFS_NAME = "KuttabPrefs";
    private const string LAST_UPDATE_CHECK_KEY = "last_update_check";
    private const string IGNORED_VERSION_KEY = "ignored_update_version";
    
    private readonly Context _context;
    private readonly SimpleNotificationService _notificationService;

    public SimpleUpdateService(Context context, SimpleNotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task CheckForUpdatesAsync(bool forceCheck = false)
    {
        try
        {
            var prefs = _context.GetSharedPreferences(PREFS_NAME, FileCreationMode.Private);
            var lastCheck = prefs.GetLong(LAST_UPDATE_CHECK_KEY, 0);
            var currentTime = Java.Lang.JavaSystem.CurrentTimeMillis();

            // Only check once per day unless forced
            if (!forceCheck && (currentTime - lastCheck) < TimeSpan.FromHours(24).TotalMilliseconds)
            {
                return;
            }

            Console.WriteLine("Checking for app updates...");
            
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Kuttab-App");
            
            var response = await client.GetAsync(UPDATE_CHECK_URL);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var releaseInfo = JsonConvert.DeserializeObject<GitHubRelease>(json);
                
                var currentVersion = GetCurrentVersion();
                var latestVersion = ParseVersion(releaseInfo.TagName);
                
                Console.WriteLine($"Current version: {currentVersion}, Latest version: {latestVersion}");

                if (latestVersion > currentVersion)
                {
                    var ignoredVersion = prefs.GetString(IGNORED_VERSION_KEY, "");
                    if (ignoredVersion != releaseInfo.TagName)
                    {
                        _notificationService.ShowUpdateNotification(
                            releaseInfo.TagName, 
                            releaseInfo.HtmlUrl, 
                            releaseInfo.Body);
                    }
                }
                
                // Update last check time
                var editor = prefs.Edit();
                editor.PutLong(LAST_UPDATE_CHECK_KEY, currentTime);
                editor.Apply();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking for updates: {ex.Message}");
        }
    }

    private int GetCurrentVersion()
    {
        try
        {
            var packageManager = _context.PackageManager;
            var packageInfo = packageManager.GetPackageInfo(_context.PackageName, 0);
            return packageInfo.VersionCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting current version: {ex.Message}");
            return 0;
        }
    }

    private int ParseVersion(string versionTag)
    {
        try
        {
            // Remove 'v' prefix if present and parse
            var version = versionTag.StartsWith("v") ? versionTag.Substring(1) : versionTag;
            var parts = version.Split('.');
            
            if (parts.Length >= 2)
            {
                var major = int.Parse(parts[0]);
                var minor = int.Parse(parts[1]);
                return major * 100 + minor; // Simple version comparison
            }
            
            return int.Parse(version);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing version '{versionTag}': {ex.Message}");
            return 0;
        }
    }

    public void SchedulePeriodicUpdateCheck()
    {
        try
        {
            // For now, we'll check on app start
            // In a production app, you might want to use WorkManager or AlarmManager
            Console.WriteLine("Periodic update check will be done on app start");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error scheduling update check: {ex.Message}");
        }
    }
}

public class GitHubRelease
{
    public string TagName { get; set; }
    public string Name { get; set; }
    public string Body { get; set; }
    public string HtmlUrl { get; set; }
    public bool Prerelease { get; set; }
    public DateTime PublishedAt { get; set; }
}

[BroadcastReceiver]
public class UpdateDismissReceiver : BroadcastReceiver
{
    public override void OnReceive(Context context, Intent intent)
    {
        try
        {
            var version = intent.GetStringExtra("version");
            if (!string.IsNullOrEmpty(version))
            {
                var prefs = context.GetSharedPreferences("KuttabPrefs", FileCreationMode.Private);
                var editor = prefs.Edit();
                editor.PutString("ignored_update_version", version);
                editor.Apply();

                var notificationManager = (NotificationManager)context.GetSystemService(Context.NotificationService);
                notificationManager.Cancel(1);

                Console.WriteLine($"Dismissed update notification for version {version}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error dismissing update notification: {ex.Message}");
        }
    }
}
