using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;

namespace MGBlazorApp.Services
{
    public class SpotifyService
    {
        private readonly HttpClient _httpClient;
        private readonly AppConfigurationService _configService;
    private readonly IJSRuntime _jsRuntime;
    private readonly NavigationManager _navigation;
        
        // PKCE values stored for the session
        private string? _codeVerifier;
        private string? _state;
        
        public SpotifyService(HttpClient httpClient, AppConfigurationService configService, IJSRuntime jsRuntime, NavigationManager navigation)
        {
            _httpClient = httpClient;
            _configService = configService;
            _jsRuntime = jsRuntime;
            _navigation = navigation;
        }
        
    private string BuildRedirectUri(string? configured)
        {
            // If configured value present use trimmed version, otherwise build from current base URI
            if (!string.IsNullOrWhiteSpace(configured))
            {
        var val = configured.Trim();
        // If user set https://localhost and config flag asks to downgrade for testing
        if (val.StartsWith("https://localhost", StringComparison.OrdinalIgnoreCase) && _lastLoadedConfig?.UseHttpRedirect == true)
        {
            val = val.Replace("https://localhost", "http://localhost");
        }
        return val;
            }
            var baseUri = _navigation.BaseUri; // ends with '/'
            if (!baseUri.EndsWith('/')) baseUri += "/";
            return baseUri + "spotify-callback"; // yields e.g. https://localhost:7267/spotify-callback
        }
    private SpotifyConfig? _lastLoadedConfig;

        // Public helper for diagnostics
        public async Task<(string ClientId, string RedirectUri, string Origin)> GetAuthConfigSnapshotAsync()
        {
            var config = await _configService.GetSpotifyConfigAsync();
            var redirect = BuildRedirectUri(config.RedirectUri);
            var origin = _navigation.BaseUri;
            return (config.ClientId, redirect, origin);
        }
        
        public async Task<string> GetAuthorizationUrlAsync()
        {
            var config = await _configService.GetSpotifyConfigAsync();
            _lastLoadedConfig = config;            
            var clientId = config.ClientId;
            var redirectUri = BuildRedirectUri(config.RedirectUri);
            var scope = "user-read-private user-read-email playlist-read-private playlist-modify-public playlist-modify-private";
            
            // Debug logging
            Console.WriteLine($"Debug: ClientId = '{clientId}'");
            Console.WriteLine($"Debug: RedirectUri = '{redirectUri}'");
            Console.WriteLine($"Debug: Full auth URL will be generated with these parameters");
            Console.WriteLine($"Debug: RedirectUri Length = {redirectUri?.Length}");
            if (!string.IsNullOrEmpty(redirectUri))
            {
                var escaped = Uri.EscapeDataString(redirectUri);
                Console.WriteLine($"Debug: Escaped RedirectUri = '{escaped}' Length={escaped.Length}");
                Console.WriteLine("Debug: RedirectUri Hex Bytes: " + string.Join(" ", Encoding.UTF8.GetBytes(redirectUri).Select(b => b.ToString("X2"))));
            }
            
            if (string.IsNullOrEmpty(clientId))
            {
                throw new InvalidOperationException("Error configure spotify client id");
            }
            
            if (string.IsNullOrEmpty(redirectUri))
            {
                throw new InvalidOperationException("Error configure spotify redirect uri");
            }
            
            // Generate PKCE values
            _codeVerifier = GenerateCodeVerifier();
            var codeChallenge = GenerateCodeChallenge(_codeVerifier);
            _state = Guid.NewGuid().ToString();
            
            // Store PKCE values in browser storage for retrieval during callback
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "spotify_code_verifier", _codeVerifier);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "spotify_state", _state);
            
            var authUrl = $"https://accounts.spotify.com/authorize?" +
                   $"client_id={clientId}&" +
                   $"response_type=code&" +
                   $"redirect_uri={Uri.EscapeDataString(redirectUri)}&" +
                   $"scope={Uri.EscapeDataString(scope)}&" +
                   $"state={_state}&" +
                   $"code_challenge_method=S256&" +
                   $"code_challenge={codeChallenge}";
            
            Console.WriteLine($"Debug: Generated auth URL: {authUrl}");
            
            return authUrl;
        }
        
        public async Task<SpotifyTokenResponse?> ExchangeCodeForTokenAsync(string code, string state)
        {
            try
            {
                // Retrieve stored PKCE values
                var storedCodeVerifier = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "spotify_code_verifier");
                var storedState = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "spotify_state");
                
                // Validate state parameter
                if (state != storedState)
                {
                    throw new UnauthorizedAccessException("Invalid state parameter");
                }
                
                if (string.IsNullOrEmpty(storedCodeVerifier))
                {
                    throw new InvalidOperationException("Code verifier not found");
                }
                
                var config = await _configService.GetSpotifyConfigAsync();
                _lastLoadedConfig = config;
                var clientId = config.ClientId;
                var redirectUri = BuildRedirectUri(config.RedirectUri);
                
                if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(redirectUri))
                {
                    throw new InvalidOperationException("Spotify configuration is missing");
                }
                
                var requestBody = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "authorization_code"),
                    new KeyValuePair<string, string>("code", code),
                    new KeyValuePair<string, string>("redirect_uri", redirectUri),
                    new KeyValuePair<string, string>("client_id", clientId),
                    new KeyValuePair<string, string>("code_verifier", storedCodeVerifier)
                });
                
                var response = await _httpClient.PostAsync("https://accounts.spotify.com/api/token", requestBody);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Token exchange failed: {response.StatusCode} - {errorContent}");
                }
                
                var content = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonSerializer.Deserialize<SpotifyTokenResponse>(content, new JsonSerializerOptions 
                { 
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower 
                });
                
                // Clean up stored PKCE values
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "spotify_code_verifier");
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "spotify_state");
                
                // Store access token securely in session storage
                if (tokenResponse?.AccessToken != null)
                {
                    await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", "spotify_access_token", tokenResponse.AccessToken);
                    if (tokenResponse.RefreshToken != null)
                    {
                        await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", "spotify_refresh_token", tokenResponse.RefreshToken);
                    }
                }
                
                return tokenResponse;
            }
            catch (Exception ex)
            {
                // Clean up on error
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "spotify_code_verifier");
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "spotify_state");
                throw new Exception($"Failed to exchange authorization code for token: {ex.Message}", ex);
            }
        }
        
        public async Task<SpotifyUser?> GetUserProfileAsync(string? accessToken = null)
        {
            try
            {
                // Use provided token or get from storage
                var token = accessToken ?? await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", "spotify_access_token");
                
                if (string.IsNullOrEmpty(token))
                {
                    throw new UnauthorizedAccessException("No access token available");
                }
                
                using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.spotify.com/v1/me");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                
                var response = await _httpClient.SendAsync(request);
                
                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        // Token might be expired, try to refresh
                        var refreshed = await RefreshTokenAsync();
                        if (refreshed != null)
                        {
                            return await GetUserProfileAsync(refreshed.AccessToken);
                        }
                    }
                    
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Failed to get user profile: {response.StatusCode} - {errorContent}");
                }
                
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<SpotifyUser>(content, new JsonSerializerOptions 
                { 
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower 
                });
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get user profile: {ex.Message}", ex);
            }
        }
        
        public async Task<SpotifyTokenResponse?> RefreshTokenAsync()
        {
            try
            {
                var refreshToken = await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", "spotify_refresh_token");
                
                if (string.IsNullOrEmpty(refreshToken))
                {
                    return null;
                }
                
                var config = await _configService.GetSpotifyConfigAsync();
                var clientId = config.ClientId;
                
                if (string.IsNullOrEmpty(clientId))
                {
                    throw new InvalidOperationException("Spotify ClientId is not configured");
                }
                
                var requestBody = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "refresh_token"),
                    new KeyValuePair<string, string>("refresh_token", refreshToken),
                    new KeyValuePair<string, string>("client_id", clientId)
                });
                
                var response = await _httpClient.PostAsync("https://accounts.spotify.com/api/token", requestBody);
                
                if (!response.IsSuccessStatusCode)
                {
                    // If refresh fails, clear tokens
                    await LogoutAsync();
                    return null;
                }
                
                var content = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonSerializer.Deserialize<SpotifyTokenResponse>(content, new JsonSerializerOptions 
                { 
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower 
                });
                
                // Update stored tokens
                if (tokenResponse?.AccessToken != null)
                {
                    await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", "spotify_access_token", tokenResponse.AccessToken);
                    if (tokenResponse.RefreshToken != null)
                    {
                        await _jsRuntime.InvokeVoidAsync("sessionStorage.setItem", "spotify_refresh_token", tokenResponse.RefreshToken);
                    }
                }
                
                return tokenResponse;
            }
            catch
            {
                await LogoutAsync();
                return null;
            }
        }
        
        public async Task LogoutAsync()
        {
            // Clear all stored tokens
            await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", "spotify_access_token");
            await _jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", "spotify_refresh_token");
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "spotify_code_verifier");
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "spotify_state");
        }
        
        public async Task<bool> IsAuthenticatedAsync()
        {
            var token = await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", "spotify_access_token");
            return !string.IsNullOrEmpty(token);
        }
        
        // PKCE Helper Methods
        private static string GenerateCodeVerifier()
        {
            var bytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
        }
        
        private static string GenerateCodeChallenge(string codeVerifier)
        {
            var bytes = Encoding.UTF8.GetBytes(codeVerifier);
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hashedBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
        }
    }
    
    public class SpotifyTokenResponse
    {
        public string? AccessToken { get; set; }
        public string? TokenType { get; set; }
        public int ExpiresIn { get; set; }
        public string? RefreshToken { get; set; }
        public string? Scope { get; set; }
    }
    
    public class SpotifyUser
    {
        public string? Id { get; set; }
        public string? DisplayName { get; set; }
        public string? Email { get; set; }
        public SpotifyImage[]? Images { get; set; }
        public int Followers { get; set; }
        public string? Country { get; set; }
        public string? Product { get; set; }
    }
    
    public class SpotifyImage
    {
        public string? Url { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
    }
}