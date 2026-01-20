using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using System;
using System.Threading.Tasks;

namespace Kuttab.Android.Services;

public class SimpleNotificationService
{
    private const string TAG = "SimpleNotificationService";
    private const string CHANNEL_ID_GENERAL = "kuttab_general";
    private const string CHANNEL_ID_UPDATES = "kuttab_updates";
    private const string CHANNEL_ID_FEATURES = "kuttab_features";
    
    private readonly Context _context;
    private readonly NotificationManager _notificationManager;

    public SimpleNotificationService(Context context)
    {
        _context = context;
        _notificationManager = (NotificationManager)context.GetSystemService(Context.NotificationService);
        CreateNotificationChannels();
    }

    private void CreateNotificationChannels()
    {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            // General notifications
            var generalChannel = new NotificationChannel(
                CHANNEL_ID_GENERAL,
                "General Notifications",
                NotificationImportance.Default)
            {
                Description = "General app notifications and information",
                LockscreenVisibility = NotificationVisibility.Public
            };

            // Update notifications
            var updateChannel = new NotificationChannel(
                CHANNEL_ID_UPDATES,
                "App Updates",
                NotificationImportance.High)
            {
                Description = "Notifications about app updates and new versions",
                LockscreenVisibility = NotificationVisibility.Public
            };

            // Feature notifications
            var featureChannel = new NotificationChannel(
                CHANNEL_ID_FEATURES,
                "New Features",
                NotificationImportance.Default)
            {
                Description = "Notifications about new features and improvements",
                LockscreenVisibility = NotificationVisibility.Public
            };

            _notificationManager.CreateNotificationChannel(generalChannel);
            _notificationManager.CreateNotificationChannel(updateChannel);
            _notificationManager.CreateNotificationChannel(featureChannel);
        }
    }

    public bool AreNotificationsEnabled()
    {
        try
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu) // Android 13+
            {
                return _notificationManager.AreNotificationsEnabled();
            }
            
            // For older versions, notifications are enabled by default
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking notification status: {ex.Message}");
            return false;
        }
    }

    public void RequestNotificationPermission(Activity activity, int requestCode = 1000)
    {
        try
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu) // Android 13+
            {
                if (!AreNotificationsEnabled())
                {
                    ActivityCompat.RequestPermissions(
                        activity,
                        new[] { global::Android.Manifest.Permission.PostNotifications },
                        requestCode);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error requesting notification permission: {ex.Message}");
        }
    }

    public void ShowUpdateNotification(string version, string downloadUrl, string releaseNotes)
    {
        try
        {
            if (!AreNotificationsEnabled())
                return;

            var intent = new Intent(_context, typeof(MainActivity));
            intent.PutExtra("update_url", downloadUrl);
            intent.PutExtra("update_version", version);
            intent.AddFlags(ActivityFlags.ClearTop);

            var pendingIntent = PendingIntent.GetActivity(
                _context,
                1,
                intent,
                PendingIntentFlags.UpdateCurrent);

            var notificationBuilder = new NotificationCompat.Builder(_context, CHANNEL_ID_UPDATES)
                .SetSmallIcon(Resource.Drawable.ic_notification)
                .SetContentTitle("Kuttab Update Available")
                .SetContentText($"Version {version} is now available!")
                .SetAutoCancel(true)
                .SetContentIntent(pendingIntent)
                .SetPriority(NotificationCompat.PriorityHigh)
                .SetStyle(new NotificationCompat.BigTextStyle()
                    .BigText($"{releaseNotes}\n\nTap to download the latest version."))
                .AddAction(Resource.Drawable.ic_download, "Download", CreateDownloadIntent(downloadUrl))
                .AddAction(Resource.Drawable.ic_dismiss, "Dismiss", CreateDismissIntent(version));

            _notificationManager.Notify(1, notificationBuilder.Build());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error showing update notification: {ex.Message}");
        }
    }

    public void ShowFeatureNotification(string title, string message, string featureUrl = null)
    {
        try
        {
            if (!AreNotificationsEnabled())
                return;

            var intent = new Intent(_context, typeof(MainActivity));
            if (!string.IsNullOrEmpty(featureUrl))
            {
                intent.PutExtra("feature_url", featureUrl);
            }
            intent.AddFlags(ActivityFlags.ClearTop);

            var pendingIntent = PendingIntent.GetActivity(
                _context,
                0,
                intent,
                PendingIntentFlags.UpdateCurrent);

            var notificationBuilder = new NotificationCompat.Builder(_context, CHANNEL_ID_FEATURES)
                .SetSmallIcon(Resource.Drawable.ic_notification)
                .SetContentTitle(title)
                .SetContentText(message)
                .SetAutoCancel(true)
                .SetContentIntent(pendingIntent)
                .SetPriority(NotificationCompat.PriorityDefault)
                .SetStyle(new NotificationCompat.BigTextStyle().BigText(message));

            _notificationManager.Notify(2, notificationBuilder.Build());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error showing feature notification: {ex.Message}");
        }
    }

    public void ShowGeneralNotification(string title, string message)
    {
        try
        {
            if (!AreNotificationsEnabled())
                return;

            var intent = new Intent(_context, typeof(MainActivity));
            intent.AddFlags(ActivityFlags.ClearTop);

            var pendingIntent = PendingIntent.GetActivity(
                _context,
                0,
                intent,
                PendingIntentFlags.UpdateCurrent);

            var notificationBuilder = new NotificationCompat.Builder(_context, CHANNEL_ID_GENERAL)
                .SetSmallIcon(Resource.Drawable.ic_notification)
                .SetContentTitle(title)
                .SetContentText(message)
                .SetAutoCancel(true)
                .SetContentIntent(pendingIntent)
                .SetPriority(NotificationCompat.PriorityDefault);

            _notificationManager.Notify(3, notificationBuilder.Build());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error showing general notification: {ex.Message}");
        }
    }

    private PendingIntent CreateDownloadIntent(string downloadUrl)
    {
        var intent = new Intent(Intent.ActionView);
        intent.SetData(global::Android.Net.Uri.Parse(downloadUrl));
        return PendingIntent.GetActivity(_context, 2, intent, PendingIntentFlags.UpdateCurrent);
    }

    private PendingIntent CreateDismissIntent(string version)
    {
        var intent = new Intent(_context, typeof(UpdateDismissReceiver));
        intent.SetAction("DISMISS_UPDATE");
        intent.PutExtra("version", version);
        return PendingIntent.GetBroadcast(_context, 3, intent, PendingIntentFlags.UpdateCurrent);
    }

    public void CancelNotification(int notificationId)
    {
        try
        {
            _notificationManager.Cancel(notificationId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error canceling notification: {ex.Message}");
        }
    }

    public void CancelAllNotifications()
    {
        try
        {
            _notificationManager.CancelAll();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error canceling all notifications: {ex.Message}");
        }
    }
}
