using Microsoft.Extensions.Logging;
using QuranSearch.Core.Services;
using QuranSearch.MAUI.Services;

namespace QuranSearch.MAUI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddLogging(logging =>
        {
            logging.AddDebug();
        });
#endif

        // Register services
        builder.Services.AddSingleton<QuranSearchService>();
        builder.Services.AddSingleton<IAudioService, PlatformAudioService>();

        return builder.Build();
    }
}
