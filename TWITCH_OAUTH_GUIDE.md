# 🔐 TWITCH OAUTH AUTHENTICATION GUIDE

## ⚠️ IMPORTANT NOTICE

**Twitchapps TMI Token Generator has been DISCONTINUED** as of 2024.

This guide provides modern, secure alternatives for Twitch OAuth authentication.

---

## 🎯 RECOMMENDED SOLUTIONS

### **Option 1: Twitch Token Generator (For End Users)**
**Best for:** Non-developers who need a quick token

🔗 **URL:** https://twitchtokengenerator.com/

**Steps:**
1. Visit https://twitchtokengenerator.com/
2. Click "Bot Chat Token"
3. Authorize with your bot account
4. Copy the Access Token (starts with your username)
5. Paste into `config/appsettings.twitch.json` under `OAuthToken`

**Scopes Needed:**
- `chat:read` - Read chat messages
- `chat:edit` - Send chat messages
- `channel:moderate` - Moderate chat (optional)

---

### **Option 2: Official Twitch OAuth (For Developers)**
**Best for:** Developers building applications

🔗 **Documentation:** https://dev.twitch.tv/docs/authentication/

#### **Step 1: Create Twitch Application**

1. Go to https://dev.twitch.tv/console/apps
2. Click "Register Your Application"
3. Fill in details:
   - **Name:** YourBotName
   - **OAuth Redirect URLs:** `http://localhost:3000`
   - **Category:** Chat Bot
4. Click "Create"
5. Copy your **Client ID**
6. Generate a **Client Secret**

#### **Step 2: Get OAuth Token**

**Method A: Using Authorization Code Flow (Recommended)**

1. Build authorization URL:
```
https://id.twitch.tv/oauth2/authorize
  ?client_id=YOUR_CLIENT_ID
  &redirect_uri=http://localhost:3000
  &response_type=code
  &scope=chat:read+chat:edit+channel:moderate
```

2. Open URL in browser and authorize
3. Copy the `code` parameter from redirect URL
4. Exchange code for token:

```bash
curl -X POST 'https://id.twitch.tv/oauth2/token' \
  -H 'Content-Type: application/x-www-form-urlencoded' \
  -d 'client_id=YOUR_CLIENT_ID' \
  -d 'client_secret=YOUR_CLIENT_SECRET' \
  -d 'code=AUTHORIZATION_CODE' \
  -d 'grant_type=authorization_code' \
  -d 'redirect_uri=http://localhost:3000'
```

5. Response will contain `access_token` - this is your OAuth token

**Method B: Using Implicit Flow (Quick but less secure)**

Authorization URL:
```
https://id.twitch.tv/oauth2/authorize
  ?client_id=YOUR_CLIENT_ID
  &redirect_uri=http://localhost:3000
  &response_type=token
  &scope=chat:read+chat:edit
```

Token will be in URL fragment after authorization.

---

### **Option 3: CLI Token Generator (For Developers)**

Use the official Twitch CLI tool:

```bash
# Install Twitch CLI
# Windows (Chocolatey)
choco install twitch-cli

# Linux/Mac (Homebrew)
brew install twitchdev/twitch/twitch-cli

# Generate token
twitch token -u -s "chat:read chat:edit"
```

---

## 🔧 CONFIGURATION

### **Update appsettings.twitch.json:**

```json
{
  "Twitch": {
    "OAuthToken": "YOUR_ACCESS_TOKEN_HERE",
    "Channel": "your_channel_name",
    "ClientId": "YOUR_CLIENT_ID",
    "ClientSecret": "YOUR_CLIENT_SECRET",
    "ChatMessages": [
      "Great stream!",
      "Amazing content!"
    ],
    "EnableChat": true
  }
}
```

⚠️ **SECURITY WARNING:** 
- Never commit OAuth tokens to Git
- Store tokens in environment variables for production
- Use `.env` files with `.gitignore`

---

## 🔄 TOKEN REFRESH (For Long-Running Bots)

OAuth tokens expire. Implement automatic refresh:

### **Get Refresh Token:**

When getting initial token, add `offline_access` scope:
```
scope=chat:read+chat:edit+offline_access
```

### **Refresh Token:**

```bash
curl -X POST 'https://id.twitch.tv/oauth2/token' \
  -H 'Content-Type: application/x-www-form-urlencoded' \
  -d 'client_id=YOUR_CLIENT_ID' \
  -d 'client_secret=YOUR_CLIENT_SECRET' \
  -d 'refresh_token=YOUR_REFRESH_TOKEN' \
  -d 'grant_type=refresh_token'
```

---

## 🛡️ SECURITY BEST PRACTICES

### **1. Environment Variables**

Create `.env` file:
```env
TWITCH_CLIENT_ID=your_client_id
TWITCH_CLIENT_SECRET=your_client_secret
TWITCH_OAUTH_TOKEN=your_oauth_token
TWITCH_REFRESH_TOKEN=your_refresh_token
```

Add to `.gitignore`:
```
.env
appsettings.*.json
```

### **2. Use Docker Secrets**

For production deployment:
```bash
docker run -d \
  --name botcore-twitch \
  -e TWITCH_OAUTH_TOKEN=$(cat /run/secrets/twitch_token) \
  -e TWITCH_CLIENT_ID=$(cat /run/secrets/twitch_client_id) \
  botcore-twitch:latest
```

### **3. Token Validation**

Validate token before use:
```bash
curl -H "Authorization: OAuth YOUR_TOKEN" \
  https://id.twitch.tv/oauth2/validate
```

---

## 🧪 TESTING YOUR TOKEN

### **Quick Test:**

```bash
# Test token validity
curl -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Client-Id: YOUR_CLIENT_ID" \
  https://api.twitch.tv/helix/users

# Expected: JSON response with user data
```

### **Chat Connection Test:**

```bash
# Connect to Twitch IRC
# Server: irc.chat.twitch.tv
# Port: 6667 (or 6697 for SSL)
# Password: oauth:YOUR_TOKEN
# Nickname: your_bot_username
```

---

## 📚 INTEGRATION WITH BOT

### **C# Implementation Example:**

```csharp
using System;
using System.Net.Http;
using System.Threading.Tasks;

public class TwitchOAuthService
{
    private readonly HttpClient _httpClient;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private string _accessToken;
    private string _refreshToken;
    
    public async Task<string> GetAccessTokenAsync()
    {
        if (await IsTokenValidAsync())
            return _accessToken;
        
        return await RefreshTokenAsync();
    }
    
    private async Task<bool> IsTokenValidAsync()
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, 
                "https://id.twitch.tv/oauth2/validate");
            request.Headers.Add("Authorization", $"OAuth {_accessToken}");
            
            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
    
    private async Task<string> RefreshTokenAsync()
    {
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("client_id", _clientId),
            new KeyValuePair<string, string>("client_secret", _clientSecret),
            new KeyValuePair<string, string>("refresh_token", _refreshToken),
            new KeyValuePair<string, string>("grant_type", "refresh_token")
        });
        
        var response = await _httpClient.PostAsync(
            "https://id.twitch.tv/oauth2/token", content);
        
        var json = await response.Content.ReadAsStringAsync();
        // Parse JSON and extract new access_token
        
        return _accessToken;
    }
}
```

---

## 🔗 USEFUL RESOURCES

| Resource | URL |
|----------|-----|
| **Twitch Dev Console** | https://dev.twitch.tv/console |
| **OAuth Documentation** | https://dev.twitch.tv/docs/authentication |
| **Twitch CLI** | https://github.com/twitchdev/twitch-cli |
| **Token Generator (End Users)** | https://twitchtokengenerator.com |
| **API Reference** | https://dev.twitch.tv/docs/api |
| **Scopes Reference** | https://dev.twitch.tv/docs/authentication/scopes |

---

## ❓ TROUBLESHOOTING

### **"Invalid OAuth token" Error**

1. Validate token: `curl -H "Authorization: OAuth TOKEN" https://id.twitch.tv/oauth2/validate`
2. Check token hasn't expired
3. Verify required scopes are included
4. Ensure token matches the bot account

### **"Missing required scope" Error**

Regenerate token with correct scopes:
- Minimum: `chat:read chat:edit`
- Recommended: `chat:read chat:edit channel:moderate`

### **Connection Refused**

1. Check firewall settings
2. Verify IRC server: `irc.chat.twitch.tv:6667`
3. Test with telnet: `telnet irc.chat.twitch.tv 6667`

---

## 🎯 QUICK START SUMMARY

1. ✅ Visit https://twitchtokengenerator.com/
2. ✅ Click "Bot Chat Token"
3. ✅ Authorize with bot account
4. ✅ Copy access token
5. ✅ Update `config/appsettings.twitch.json`
6. ✅ Deploy bot: `DEPLOY_ALL.bat`

---

**OAuth authentication configured! Bot ready for Twitch chat integration.** 🚀

