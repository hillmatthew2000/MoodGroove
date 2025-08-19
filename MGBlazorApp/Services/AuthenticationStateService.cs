using Microsoft.JSInterop;

namespace MGBlazorApp.Services
{
    public class AuthenticationStateService
    {
        private readonly IJSRuntime _jsRuntime;
        private SpotifyService? _spotifyService;
        
        public event Action? OnAuthenticationStateChanged;
        
        public bool IsAuthenticated { get; private set; }
        public SpotifyUser? CurrentUser { get; private set; }
        
        public AuthenticationStateService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }
        
        // Set the SpotifyService after both services are created (dependency injection circular reference workaround)
        public void SetSpotifyService(SpotifyService spotifyService)
        {
            _spotifyService = spotifyService;
        }
        
        public async Task InitializeAsync()
        {
            if (_spotifyService == null) return;
            
            try
            {
                // Check if user is already authenticated
                var isAuth = await _spotifyService.IsAuthenticatedAsync();
                if (isAuth)
                {
                    // Try to get user profile to validate token
                    var user = await _spotifyService.GetUserProfileAsync();
                    if (user != null)
                    {
                        SetAuthenticatedUser(user);
                    }
                    else
                    {
                        // Token is invalid, clear authentication
                        await LogoutAsync();
                    }
                }
            }
            catch
            {
                // If there's any error, clear authentication
                await LogoutAsync();
            }
        }
        
        public void SetAuthenticatedUser(SpotifyUser user)
        {
            IsAuthenticated = true;
            CurrentUser = user;
            OnAuthenticationStateChanged?.Invoke();
        }
        
        public async Task LogoutAsync()
        {
            IsAuthenticated = false;
            CurrentUser = null;
            if (_spotifyService != null)
            {
                await _spotifyService.LogoutAsync();
            }
            OnAuthenticationStateChanged?.Invoke();
        }
        
        public async Task<bool> RefreshAuthenticationAsync()
        {
            if (_spotifyService == null) return false;
            
            try
            {
                if (!IsAuthenticated)
                    return false;
                
                var refreshed = await _spotifyService.RefreshTokenAsync();
                if (refreshed != null)
                {
                    // Re-fetch user profile with new token
                    var user = await _spotifyService.GetUserProfileAsync();
                    if (user != null)
                    {
                        SetAuthenticatedUser(user);
                        return true;
                    }
                }
                
                await LogoutAsync();
                return false;
            }
            catch
            {
                await LogoutAsync();
                return false;
            }
        }
    }
}