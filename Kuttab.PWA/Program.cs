using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Kuttab.Core.Interfaces;
using Kuttab.Core.Services;
using Kuttab.PWA;
using Kuttab.PWA.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var httpClient = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
builder.Services.AddScoped(sp => httpClient);

// Create file service and pre-load data before building the host
var fileService = new WebFileService(httpClient);
var initializer = new AppInitializer(fileService, httpClient);
await initializer.InitializeAsync();

// Register services with the pre-loaded file service
builder.Services.AddScoped<IFileService>(sp => fileService);
builder.Services.AddScoped<WebFileService>(sp => fileService);
builder.Services.AddScoped<QuranSearchService>();
builder.Services.AddScoped<LocalizationService>();
builder.Services.AddScoped<LocalStorageService>();
builder.Services.AddScoped(sp => initializer);

await builder.Build().RunAsync();
