using Android.Content;
using AndroidX.Work;
using Kuttab.Android.Services;
using System;
using System.Threading.Tasks;

namespace Kuttab.Android.Workers;

public class NotificationWorker : Worker
{
    private const string TAG = "NotificationWorker";

    public NotificationWorker(Context context, WorkerParameters workerParams) : base(context, workerParams)
    {
    }

    public override Result DoWork()
    {
        try
        {
            Console.WriteLine($"{TAG}: Starting background notification check");
            
            // Initialize services
            var notificationService = new SimpleNotificationService(ApplicationContext);
            var contentService = new NotificationContentService(ApplicationContext, notificationService);
            
            // Check for notifications synchronously (WorkManager handles threading)
            var task = contentService.CheckForContentNotificationsAsync(forceCheck: false);
            task.Wait(); // Safe to wait in WorkManager background thread
            
            Console.WriteLine($"{TAG}: Background notification check completed successfully");
            return Result.InvokeSuccess();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{TAG}: Background notification check failed: {ex.Message}");
            Console.WriteLine($"{TAG}: Stack trace: {ex.StackTrace}");
            
            // Retry on failure
            return Result.InvokeRetry();
        }
    }
}
