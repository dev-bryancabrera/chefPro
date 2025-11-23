using chefPro;
using chefPro.Models;
using System.Text.Json;

public class GoogleAuthService
{
    private const string ClientId = "924868466556-opk2ad8coo6op096a453nin3eradlq1a.apps.googleusercontent.com";
    private const string RedirectUri = "urn:ietf:wg:oauth:2.0:oob";

    public async Task<GoogleUserInfo> SignInAsync()
    {
        try
        {
            var authUrl = $"https://accounts.google.com/o/oauth2/v2/auth?" +
                $"client_id={ClientId}&" +
                $"redirect_uri={RedirectUri}&" +
                $"response_type=code&" +
                $"scope=openid%20profile%20email&" +
                $"prompt=select_account";

            // Abrir en navegador del sistema
            await Browser.OpenAsync(authUrl, BrowserLaunchMode.SystemPreferred);

            // Usuario copia el código manualmente
            var code = await Application.Current.MainPage.DisplayPromptAsync(
                "Código de Google",
                "Copia y pega el código que te dio Google:",
                "OK", "Cancelar");

            if (string.IsNullOrEmpty(code))
                return null;

            var token = await GetTokenAsync(code);
            var userInfo = await GetUserInfoAsync(token);

            return userInfo;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
            throw;
        }
    }

    private async Task<string> GetTokenAsync(string code)
    {
        using (var client = new HttpClient())
        {
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "code", code },
                { "client_id", ClientId },
                { "redirect_uri", RedirectUri },
                { "grant_type", "authorization_code" }
            });

            var response = await client.PostAsync("https://oauth2.googleapis.com/token", content);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Error: {json}");
            }

            var tokenData = System.Text.Json.JsonSerializer.Deserialize<TokenResponse>(json);
            return tokenData.access_token;
        }
    }

    private async Task<GoogleUserInfo> GetUserInfoAsync(string token)
    {
        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("https://www.googleapis.com/oauth2/v2/userinfo");
            var json = await response.Content.ReadAsStringAsync();

            return System.Text.Json.JsonSerializer.Deserialize<GoogleUserInfo>(json);
        }
    }

    private class TokenResponse
    {
        public string access_token { get; set; }
    }
}
