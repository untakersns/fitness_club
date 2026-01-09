using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.JSInterop;
using fitness_club_front.Models;

namespace fitness_club_front.Services
{
    public class AuthService
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly IJSRuntime _js;
        private readonly TokenStore _tokenStore;
        private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };
        private const string AccessTokenKey = "fc_access_token";
        private const string RefreshTokenKey = "fc_refresh_token";

        public AuthService(IHttpClientFactory httpFactory, IJSRuntime js, TokenStore tokenStore)
        {
            _httpFactory = httpFactory;
            _js = js;
            _tokenStore = tokenStore;
        }

        public async Task<(bool Success, string? Error)> RegisterWithErrorAsync(RegisterRequest model, CancellationToken ct = default)
        {
            var client = _httpFactory.CreateClient("Api");
            var resp = await client.PostAsJsonAsync("api/Auth/register", model, ct);
            if (resp.IsSuccessStatusCode)
            {
                return (true, null);
            }

            string content = string.Empty;
            try
            {
                content = await resp.Content.ReadAsStringAsync();
            }
            catch
            {
            }

            string? extracted = null;
            try
            {
                using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(content) ? "{}" : content);
                var root = doc.RootElement;
                if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
                {
                    var parts = new List<string>();
                    foreach (var prop in errors.EnumerateObject())
                    {
                        if (prop.Value.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var el in prop.Value.EnumerateArray())
                            {
                                if (el.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(el.GetString()))
                                    parts.Add(el.GetString()!);
                            }
                        }
                    }
                    if (parts.Count > 0) extracted = string.Join("; ", parts);
                }
                else if (root.ValueKind == JsonValueKind.String)
                {
                    extracted = root.GetString();
                }
            }
            catch
            {
            }

            var errorText = !string.IsNullOrWhiteSpace(extracted) ? extracted : (!string.IsNullOrWhiteSpace(content) ? content : $"HTTP {(int)resp.StatusCode} {resp.ReasonPhrase}");
            return (false, errorText);
        }

        public async Task<bool> LoginAsync(LoginRequest model, CancellationToken ct = default)
        {
            var client = _httpFactory.CreateClient("Api");
            var resp = await client.PostAsJsonAsync("api/Auth/login", model, ct);
            if (!resp.IsSuccessStatusCode) return false;

            var token = await resp.Content.ReadFromJsonAsync<TokenResponse>(_jsonOptions, ct);
            if (token?.AccessToken == null) return false;

            await SaveTokensAsync(token);
            return true;
        }

        public async Task LogoutAsync(CancellationToken ct = default)
        {
            var client = _httpFactory.CreateClient("Api");
            var token = await GetAccessTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                await client.PostAsync("api/Auth/logout", null, ct);
            }

            await RemoveTokensAsync();
        }

        public async Task<UserProfileDto?> GetCurrentUserAsync(CancellationToken ct = default)
        {
            var client = _httpFactory.CreateClient("Api");
            var token = await GetAccessTokenAsync();
            if (string.IsNullOrWhiteSpace(token)) return null;

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            try
            {
                var resp = await client.GetAsync("api/Users/me", ct);
                if (!resp.IsSuccessStatusCode)
                {
                    // if unauthorized try refresh once
                    if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        var refreshed = await TryRefreshAsync(ct);
                        if (refreshed)
                        {
                            token = await GetAccessTokenAsync();
                            if (!string.IsNullOrWhiteSpace(token))
                            {
                                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                                resp = await client.GetAsync("api/Users/me", ct);
                            }
                        }
                    }

                    if (!resp.IsSuccessStatusCode) return null;
                }

                var user = await resp.Content.ReadFromJsonAsync<UserProfileDto>(_jsonOptions, ct);
                return user;
            }
            catch
            {
                return null;
            }
        }

        public async Task SaveTokensAsync(TokenResponse token)
        {
            if (token.AccessToken != null)
            {
                _tokenStore.AccessToken = token.AccessToken;
                await _js.InvokeVoidAsync("localStorage.setItem", AccessTokenKey, token.AccessToken);
            }
            if (token.RefreshToken != null)
            {
                _tokenStore.RefreshToken = token.RefreshToken;
                await _js.InvokeVoidAsync("localStorage.setItem", RefreshTokenKey, token.RefreshToken);
            }
        }

        public async Task<string?> GetAccessTokenAsync()
        {
            if (!string.IsNullOrWhiteSpace(_tokenStore.AccessToken))
                return _tokenStore.AccessToken;

            try
            {
                var token = await _js.InvokeAsync<string?>("localStorage.getItem", AccessTokenKey);
                if (!string.IsNullOrWhiteSpace(token))
                {
                    _tokenStore.AccessToken = token;
                    return token;
                }
            }
            catch
            {
                // ignore
            }

            return null;
        }

        public async Task<string?> GetRefreshTokenAsync()
        {
            if (!string.IsNullOrWhiteSpace(_tokenStore.RefreshToken))
                return _tokenStore.RefreshToken;

            try
            {
                var token = await _js.InvokeAsync<string?>("localStorage.getItem", RefreshTokenKey);
                if (!string.IsNullOrWhiteSpace(token))
                {
                    _tokenStore.RefreshToken = token;
                    return token;
                }
            }
            catch
            {
                // ignore
            }

            return null;
        }

        public async Task RemoveTokensAsync()
        {
            _tokenStore.AccessToken = null;
            _tokenStore.RefreshToken = null;
            try
            {
                await _js.InvokeVoidAsync("localStorage.removeItem", AccessTokenKey);
                await _js.InvokeVoidAsync("localStorage.removeItem", RefreshTokenKey);
            }
            catch
            {
                // ignore
            }
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            var token = await GetAccessTokenAsync();
            return !string.IsNullOrWhiteSpace(token);
        }

        private async Task<bool> TryRefreshAsync(CancellationToken ct = default)
        {
            var refresh = await GetRefreshTokenAsync();
            if (string.IsNullOrEmpty(refresh)) return false;

            var client = _httpFactory.CreateClient("Api");
            var resp = await client.PostAsJsonAsync("api/Auth/refresh", new { RefreshToken = refresh }, ct);
            if (!resp.IsSuccessStatusCode) return false;

            var token = await resp.Content.ReadFromJsonAsync<TokenResponse>(_jsonOptions, ct);
            if (token?.AccessToken == null) return false;

            await SaveTokensAsync(token);
            return true;
        }
    }
}