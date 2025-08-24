using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using QuranSearch.Web;
using QuranSearch.Core.Services;
using QuranSearch.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Register services
builder.Services.AddSingleton<QuranSearchService>();
builder.Services.AddSingleton<IAudioService, WebAudioService>();

await builder.Build().RunAsync();
