using Android.App;
using Android.Content;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kuttab.Android.Services;

public class NotificationContentService
{
    private const string TAG = "NotificationContentService";
    private const string PREFS_NAME = "KuttabPrefs";
    private const string LAST_CONTENT_CHECK_KEY = "last_content_check";
    private const string CONTENT_URL = "https://raw.githubusercontent.com/alzomor/Kuttab/main/notifications.json";
    
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
            var prefs = _context.GetSharedPreferences(PREFS_NAME, FileCreationMode.Private);
            var lastCheck = prefs.GetLong(LAST_CONTENT_CHECK_KEY, 0);
            var currentTime = Java.Lang.JavaSystem.CurrentTimeMillis();

            // Check every 6 hours unless forced
            if (!forceCheck && (currentTime - lastCheck) < TimeSpan.FromHours(6).TotalMilliseconds)
            {
                return;
            }

            Console.WriteLine("Checking for content notifications...");

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Kuttab-App");
            
            var response = await client.GetAsync(CONTENT_URL);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var notifications = Newtonsoft.Json.JsonConvert.DeserializeObject<List<NotificationContent>>(json);
                
                if (notifications != null)
                {
                    foreach (var notification in notifications)
                    {
                        if (ShouldShowNotification(notification, prefs))
                        {
                            ShowContentNotification(notification);
                            MarkNotificationAsShown(notification.Id, prefs);
                        }
                    }
                }
                
                // Update last check time
                var editor = prefs.Edit();
                editor.PutLong(LAST_CONTENT_CHECK_KEY, currentTime);
                editor.Apply();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking content notifications: {ex.Message}");
        }
    }

    private bool ShouldShowNotification(NotificationContent notification, ISharedPreferences prefs)
    {
        // Check if notification is active
        if (!notification.IsActive)
            return false;

        // Check if already shown
        var shownKey = $"notification_shown_{notification.Id}";
        if (prefs.GetBoolean(shownKey, false))
            return false;

        // Check target audience
        if (!MatchesTargetAudience(notification.TargetAudience))
            return false;

        // Check schedule
        if (notification.Schedule != null)
        {
            var now = DateTime.UtcNow;
            var start = notification.Schedule.StartTime ?? DateTime.MinValue;
            var end = notification.Schedule.EndTime ?? DateTime.MaxValue;
            
            if (now < start || now > end)
                return false;
        }

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
        // For now, we'll check on app start
        // In future, could use WorkManager for background checks
        Console.WriteLine("Content notifications will be checked on app start");
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
