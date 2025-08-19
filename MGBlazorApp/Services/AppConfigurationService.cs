using System.Text.Json;

namespace MGBlazorApp.Services
{
    public class AppConfigurationService
    {
        private readonly HttpClient _httpClient;
        private SpotifyConfig? _spotifyConfig;
        
        public AppConfigurationService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        
        public async Task<SpotifyConfig> GetSpotifyConfigAsync()
        {
            if (_spotifyConfig == null)
            {
                try
                {
                    var response = await _httpClient.GetStringAsync("appsettings.json");
                    var config = JsonSerializer.Deserialize<AppConfig>(response, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    
                    _spotifyConfig = config?.Spotify ?? new SpotifyConfig();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading configuration: {ex.Message}");
                    // Fallback configuration for development
                    _spotifyConfig = new SpotifyConfig
                    {
                        ClientId = "4a8fb4093f1349b1a23ea4728013b6c3",
                        RedirectUri = "https://localhost:7267/spotify-callback"
                    };
                }
            }
            
            return _spotifyConfig;
        }
    }
    
    public class AppConfig
    {
        public SpotifyConfig? Spotify { get; set; }
    }
    
    public class SpotifyConfig
    {
        public string ClientId { get; set; } = "";
        public string RedirectUri { get; set; } = "";
    }
}
