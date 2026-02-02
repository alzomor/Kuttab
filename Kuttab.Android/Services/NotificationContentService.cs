using Android.App;
using Android.Content;
using AndroidX.Work;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Java.Util.Concurrent;

namespace Kuttab.Android.Services;

public class NotificationContentService
{
    private const string TAG = "NotificationContentService";
    private const string PREFS_NAME = "KuttabPrefs";
    private const string LAST_CONTENT_CHECK_KEY = "last_content_check";
    private const string CONTENT_URL = "https://raw.githubusercontent.com/alzomor/Kuttab/AndroidMigration/notifications.json";
    
    private readonly Context _context;
    private readonly SimpleNotificationService _notificationService;

    public NotificationContentService(Context context, SimpleNotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task CheckForContentNotificationsAsync(bool forceCheck = false)
    {
        try
        {
            Console.WriteLine($"{TAG}: Starting notification check (forceCheck: {forceCheck})");
            
            var prefs = _context.GetSharedPreferences(PREFS_NAME, FileCreationMode.Private);
            var lastCheck = prefs.GetLong(LAST_CONTENT_CHECK_KEY, 0);
            var currentTime = Java.Lang.JavaSystem.CurrentTimeMillis();
            var timeSinceLastCheck = TimeSpan.FromMilliseconds(currentTime - lastCheck);

            Console.WriteLine($"{TAG}: Last check: {new DateTime(1970, 1, 1).AddMilliseconds(lastCheck):yyyy-MM-dd HH:mm:ss} UTC");
            Console.WriteLine($"{TAG}: Current time: {new DateTime(1970, 1, 1).AddMilliseconds(currentTime):yyyy-MM-dd HH:mm:ss} UTC");
            Console.WriteLine($"{TAG}: Time since last check: {timeSinceLastCheck.TotalHours:F2} hours");

            // Check every 6 hours unless forced
            if (!forceCheck && timeSinceLastCheck.TotalHours < 6)
            {
                Console.WriteLine($"{TAG}: Skipping check - only {timeSinceLastCheck.TotalHours:F2} hours since last check");
                return;
            }

            Console.WriteLine($"{TAG}: Fetching notifications from: {CONTENT_URL}");

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Kuttab-App");
            client.Timeout = TimeSpan.FromSeconds(30);
            
            var response = await client.GetAsync(CONTENT_URL);
            Console.WriteLine($"{TAG}: HTTP Response: {response.StatusCode}");
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"{TAG}: Received JSON length: {json.Length} characters");
                
                var notifications = Newtonsoft.Json.JsonConvert.DeserializeObject<List<NotificationContent>>(json);
                
                if (notifications != null)
                {
                    Console.WriteLine($"{TAG}: Parsed {notifications.Count} notifications");
                    
                    int shownCount = 0;
                    foreach (var notification in notifications)
                    {
                        Console.WriteLine($"{TAG}: Checking notification '{notification.Id}' - '{notification.Title}'");
                        
                        if (ShouldShowNotification(notification, prefs))
                        {
                            Console.WriteLine($"{TAG}: Showing notification '{notification.Id}'");
                            ShowContentNotification(notification);
                            MarkNotificationAsShown(notification.Id, prefs);
                            shownCount++;
                        }
                        else
                        {
                            Console.WriteLine($"{TAG}: Skipping notification '{notification.Id}' - conditions not met");
                        }
                    }
                    
                    Console.WriteLine($"{TAG}: Showed {shownCount} out of {notifications.Count} notifications");
                }
                else
                {
                    Console.WriteLine($"{TAG}: Failed to parse notifications JSON");
                }
                
                // Update last check time
                var editor = prefs.Edit();
                editor.PutLong(LAST_CONTENT_CHECK_KEY, currentTime);
                editor.Apply();
                Console.WriteLine($"{TAG}: Updated last check time");
            }
            else
            {
                Console.WriteLine($"{TAG}: HTTP request failed: {response.StatusCode} - {response.ReasonPhrase}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{TAG}: Error checking content notifications: {ex.Message}");
            Console.WriteLine($"{TAG}: Stack trace: {ex.StackTrace}");
        }
    }

    private bool ShouldShowNotification(NotificationContent notification, ISharedPreferences prefs)
    {
        Console.WriteLine($"{TAG}: Evaluating notification '{notification.Id}':");
        
        // Check if notification is active
        if (!notification.IsActive)
        {
            Console.WriteLine($"{TAG}: - Skipped: isActive = false");
            return false;
        }
        Console.WriteLine($"{TAG}: - isActive = true ✓");

        // Check if already shown
        var shownKey = $"notification_shown_{notification.Id}";
        var alreadyShown = prefs.GetBoolean(shownKey, false);
        if (alreadyShown)
        {
            Console.WriteLine($"{TAG}: - Skipped: already shown");
            return false;
        }
        Console.WriteLine($"{TAG}: - not shown before ✓");

        // Check target audience
        if (!MatchesTargetAudience(notification.TargetAudience))
        {
            Console.WriteLine($"{TAG}: - Skipped: target audience mismatch");
            return false;
        }
        Console.WriteLine($"{TAG}: - target audience matches ✓");

        // Check schedule
        if (notification.Schedule != null)
        {
            var now = DateTime.UtcNow;
            var start = notification.Schedule.StartTime ?? DateTime.MinValue;
            var end = notification.Schedule.EndTime ?? DateTime.MaxValue;
            
            Console.WriteLine($"{TAG}: - Schedule check:");
            Console.WriteLine($"{TAG}: - - Current time: {now:yyyy-MM-dd HH:mm:ss} UTC");
            Console.WriteLine($"{TAG}: - - Start time: {start:yyyy-MM-dd HH:mm:ss} UTC");
            Console.WriteLine($"{TAG}: - - End time: {end:yyyy-MM-dd HH:mm:ss} UTC");
            
            if (now < start)
            {
                Console.WriteLine($"{TAG}: - Skipped: not yet started");
                return false;
            }
            
            if (now > end)
            {
                Console.WriteLine($"{TAG}: - Skipped: expired");
                return false;
            }
            
            Console.WriteLine($"{TAG}: - schedule valid ✓");
        }
        else
        {
            Console.WriteLine($"{TAG}: - no schedule restrictions ✓");
        }

        Console.WriteLine($"{TAG}: - All conditions met - will show notification");
        return true;
    }

    private bool MatchesTargetAudience(List<string> targetAudience)
    {
        // For now, show to all users
        // In future, you could check user preferences, language, etc.
        return targetAudience.Contains("all") || targetAudience.Count == 0;
    }

    private void ShowContentNotification(NotificationContent notification)
    {
        try
        {
            switch (notification.Type.ToLower())
            {
                case "feature":
                    _notificationService.ShowFeatureNotification(
                        notification.Title, 
                        notification.Message, 
                        notification.ActionUrl);
                    break;
                case "announcement":
                    _notificationService.ShowGeneralNotification(
                        notification.Title, 
                        notification.Message);
                    break;
                case "educational":
                    _notificationService.ShowFeatureNotification(
                        notification.Title, 
                        notification.Message, 
                        notification.ActionUrl);
                    break;
                default:
                    _notificationService.ShowGeneralNotification(
                        notification.Title, 
                        notification.Message);
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error showing content notification: {ex.Message}");
        }
    }

    private void MarkNotificationAsShown(string notificationId, ISharedPreferences prefs)
    {
        var editor = prefs.Edit();
        editor.PutBoolean($"notification_shown_{notificationId}", true);
        editor.Apply();
    }

    public void SchedulePeriodicContentCheck()
    {
        try
        {
            Console.WriteLine($"{TAG}: Setting up periodic background notification checks");
            
            // Create constraints for the work
            var constraints = new Constraints.Builder()
                .SetRequiredNetworkType(NetworkType.Connected)
                .SetRequiresBatteryNotLow(true)
                .Build();

            // Create periodic work request (minimum interval is 15 minutes)
            var workRequest = new PeriodicWorkRequest.Builder(Java.Lang.Class.FromType(typeof(Workers.NotificationWorker)), 6, TimeUnit.Hours)
                .SetConstraints(constraints)
                .SetBackoffCriteria(BackoffPolicy.Linear, 30, TimeUnit.Minutes)
                .AddTag("notification_check")
                .Build();

            // Enqueue the work (replace any existing work with same unique name)
            WorkManager.GetInstance(_context).EnqueueUniquePeriodicWork(
                "periodic_notification_check",
                ExistingPeriodicWorkPolicy.Replace,
                workRequest);

            Console.WriteLine($"{TAG}: Periodic notification check scheduled successfully (every 6 hours)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{TAG}: Failed to schedule periodic notification check: {ex.Message}");
        }
    }
}

public class NotificationContent
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = "general"; // feature, announcement, educational
    public string ActionUrl { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public List<string> TargetAudience { get; set; } = new List<string>();
    public NotificationSchedule Schedule { get; set; } = new NotificationSchedule();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class NotificationSchedule
{
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public List<string> DaysOfWeek { get; set; } = new List<string>(); // monday, tuesday, etc.
}
