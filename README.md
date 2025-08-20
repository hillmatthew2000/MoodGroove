# 🎵 MoodGroove

A modern web application built with Blazor WebAssembly that integrates with Spotify to provide personalized music experiences based on your mood.

## 📋 Table of Contents

- [Features](#-features)
- [Tech Stack](#-tech-stack)
- [Prerequisites](#-prerequisites)
- [Installation](#-installation)
- [Spotify App Configuration](#-spotify-app-configuration)
- [Running the Application](#-running-the-application)
- [Project Structure](#-project-structure)
- [Authentication Flow](#-authentication-flow)
- [Security Features](#-security-features)
- [Configuration](#-configuration)
- [Troubleshooting](#-troubleshooting)
- [Contributing](#-contributing)
- [License](#-license)

## ✨ Features

- **Secure Spotify Authentication**: OAuth 2.0 with PKCE (Proof Key for Code Exchange) flow
- **User Profile Integration**: Access Spotify user profile information
- **Responsive Design**: Modern, mobile-friendly interface
- **Real-time State Management**: Seamless authentication state handling
- **Token Management**: Automatic token refresh and secure storage
- **Error Handling**: Comprehensive error handling with user-friendly messages

## 🛠 Tech Stack

- **Frontend**: Blazor WebAssembly (.NET 9.0)
- **Authentication**: Spotify Web API OAuth 2.0 with PKCE
- **State Management**: Custom authentication state service
- **HTTP Client**: Built-in .NET HttpClient
- **Security**: SHA256 hashing, secure token storage
- **Storage**: Browser sessionStorage and localStorage

## 📋 Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Spotify Developer Account](https://developer.spotify.com/)
- Modern web browser with JavaScript enabled
- HTTPS development certificate (automatically configured with .NET)

## 🚀 Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/hillmatthew2000/MoodGroove.git
   cd MoodGroove/MoodGroove/MGBlazorApp
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Build the project**
   ```bash
   dotnet build
   ```

## 🎶 Spotify App Configuration

### 1. Create a Spotify App

1. Go to [Spotify Developer Dashboard](https://developer.spotify.com/dashboard)
2. Click **"Create App"**
3. Fill in the app details:
   - **App name**: MoodGroove (or your preferred name)
   - **App description**: A mood-based music application
   - **Website**: http://localhost (for development)
   - **Redirect URI**: `https://localhost:7267/spotify-callback`
4. Check the **Web API** checkbox
5. Click **"Save"**

### 2. Configure App Settings

1. In your Spotify app dashboard, click **"Settings"**
2. Add the redirect URI: `https://localhost:7267/spotify-callback`
3. Copy your **Client ID** (you'll need this for configuration)
4. **Important**: Do NOT use the Client Secret in this client-side application

### 3. Development Mode Configuration

If your app is in Development Mode:
1. Go to **"Users and Access"** in your Spotify app settings
2. Click **"Add New User"**
3. Enter your Spotify email address
4. Set role to **"User"**
5. Save the settings

## ⚙️ Configuration

Update the `wwwroot/appsettings.json` file with your Spotify app credentials:

```json
{
  "Spotify": {
    "ClientId": "your_spotify_client_id_here",
    "RedirectUri": "https://localhost:7267/spotify-callback"
  }
}
```

### Configuration Files

- **`appsettings.json`**: Contains Spotify client configuration
- **`Properties/launchSettings.json`**: Development server settings

## 🏃‍♂️ Running the Application

### Development Mode

1. **Trust the development certificate** (first time only):
   ```bash
   dotnet dev-certs https --trust
   ```

2. **Run the application**:
   ```bash
   dotnet run --launch-profile https
   ```

3. **Open your browser** and navigate to:
   ```
   https://localhost:7267
   ```

### Production Build

```bash
dotnet publish -c Release -o ./publish
```

## 📁 Project Structure

```
MGBlazorApp/
├── 📁 Pages/
│   ├── Index.razor              # Home page
│   ├── MoodSelection.razor      # Mood selection interface
│   └── Spotifycallback.razor    # OAuth callback handler
├── 📁 Services/
│   ├── SpotifyService.cs        # Spotify API integration
│   ├── AuthenticationStateService.cs  # Auth state management
│   └── AppConfigurationService.cs     # Configuration management
├── 📁 Shared/
│   ├── LoginDisplay.razor       # Authentication UI component
│   └── NavMenu.razor           # Navigation component
├── 📁 Layout/
│   └── MainLayout.razor        # Main application layout
├── 📁 Models/
│   └── Mood.cs                 # Mood data models
├── 📁 wwwroot/
│   ├── appsettings.json        # Application configuration
│   ├── index.html              # Main HTML page
│   └── css/                    # Stylesheets
├── App.razor                   # Root component
├── Program.cs                  # Application entry point
└── MGBlazorApp.csproj         # Project file
```

## 🔐 Authentication Flow

### 1. Authorization Request
- User clicks "Connect Spotify"
- App generates PKCE code verifier and challenge
- User is redirected to Spotify authorization page

### 2. Authorization Callback
- Spotify redirects back with authorization code
- App validates state parameter for security
- Code is exchanged for access token using PKCE

### 3. Token Management
- Access tokens stored in sessionStorage (temporary)
- Refresh tokens stored securely
- Automatic token refresh when expired

### 4. User Profile
- Fetch Spotify user profile information
- Update authentication state
- Redirect to main application

## 🔒 Security Features

### PKCE (Proof Key for Code Exchange)
- **Code Verifier**: Cryptographically random string (32 bytes, base64url encoded)
- **Code Challenge**: SHA256 hash of the code verifier
- **State Parameter**: CSRF protection with UUID

### Token Security
- **Access Tokens**: Stored in sessionStorage (cleared on browser close)
- **Refresh Tokens**: Stored in sessionStorage with automatic cleanup
- **No Client Secret**: Client-side app uses public client flow

### Storage Strategy
- **localStorage**: PKCE values (temporary, cleared after use)
- **sessionStorage**: Tokens (cleared when browser tab closes)
- **No persistent storage**: Enhanced security for web applications

## 🔧 Configuration Options

### AppConfigurationService
- Loads configuration from `appsettings.json`
- Fallback configuration for development
- Error handling with graceful degradation

### SpotifyService Configuration
```csharp
public class SpotifyConfig
{
    public string ClientId { get; set; } = "";
    public string RedirectUri { get; set; } = "";
}
```

### Spotify API Scopes
The application requests these Spotify scopes:
- `user-read-private`: Access user profile information
- `user-read-email`: Access user email address
- `playlist-read-private`: Read user's private playlists
- `playlist-modify-public`: Modify user's public playlists
- `playlist-modify-private`: Modify user's private playlists

## 🐛 Troubleshooting

### Common Issues

#### "INVALID_CLIENT: Insecure redirect URI"
- **Cause**: Redirect URI mismatch between app and Spotify configuration
- **Solution**: Ensure redirect URI in Spotify app settings exactly matches: `https://localhost:7267/spotify-callback`

#### "Access to this app is limited"
- **Cause**: Spotify app is in Development Mode and user is not added
- **Solution**: Add your Spotify email to "Users and Access" in app settings

#### Blank Page on Load
- **Cause**: Missing `appsettings.json` or configuration errors
- **Solution**: Verify `appsettings.json` exists in `wwwroot/` folder with correct format

#### Certificate Errors
- **Cause**: HTTPS development certificate not trusted
- **Solution**: Run `dotnet dev-certs https --trust`

### Debug Information

The application includes comprehensive logging:
- Browser console logs for authentication flow
- Server-side logging for API calls
- Error messages with detailed information

### Development Tips

1. **Check Browser Console**: Use F12 Developer Tools to see detailed logs
2. **Verify HTTPS**: Ensure you're accessing `https://localhost:7267` (not HTTP)
3. **Clear Storage**: Clear browser storage if authentication state is corrupted
4. **Check Network Tab**: Monitor API calls to Spotify in browser dev tools

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Development Guidelines

- Follow C# coding standards
- Add comprehensive error handling
- Include unit tests for new features
- Update documentation for new functionality
- Ensure security best practices

## 📄 License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## 🔗 Links

- [Spotify Web API Documentation](https://developer.spotify.com/documentation/web-api/)
- [Blazor WebAssembly Documentation](https://docs.microsoft.com/en-us/aspnet/core/blazor/webassembly)
- [OAuth 2.0 PKCE Specification](https://tools.ietf.org/html/rfc7636)

## 🙏 Acknowledgments

- Spotify for providing the Web API
- Microsoft for Blazor WebAssembly framework
- The open-source community for inspiration and support

---

**MoodGroove** - Connect your mood to your music 🎵✨
