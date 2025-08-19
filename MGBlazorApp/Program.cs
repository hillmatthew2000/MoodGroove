using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MGBlazorApp;
using MGBlazorApp.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<AppConfigurationService>();
builder.Services.AddScoped<SpotifyService>();
builder.Services.AddSingleton<AuthenticationStateService>();

try
{
    var app = builder.Build();

    // Initialize the authentication state service with the spotify service
    var authService = app.Services.GetRequiredService<AuthenticationStateService>();
    var spotifyService = app.Services.GetRequiredService<SpotifyService>();
    authService.SetSpotifyService(spotifyService);

    await app.RunAsync();
}
catch (Exception ex)
{
    Console.WriteLine($"Application startup error: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
    throw;
}
