using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.JSInterop;
using fitness_club_front.Models;

namespace fitness_club_front.Services
{
    public class ApiAuthHandler : DelegatingHandler
    {
        private readonly IJSRuntime _js;
        private readonly IConfiguration _config;
        private readonly TokenStore _tokenStore;
        private const string AccessTokenKey = "fc_access_token";
        private const string RefreshTokenKey = "fc_refresh_token";

        public ApiAuthHandler(IJSRuntime js, IConfiguration config, TokenStore tokenStore)
        {
            _js = js;
            _config = config;
            _tokenStore = tokenStore;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                string? token = _tokenStore?.AccessToken;
                if (string.IsNullOrWhiteSpace(token))
                {
                    try
                    {
                        token = await _js.InvokeAsync<string?>("localStorage.getItem", AccessTokenKey);
                        if (!string.IsNullOrWhiteSpace(token))
                            _tokenStore.AccessToken = token;
                    }
                    catch
                    {
                        // ignore JS interop errors (prerendering etc.)
                    }
                }

                if (!string.IsNullOrWhiteSpace(token))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
            }
            catch
            {
                // ignore problems setting header
            }

            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                // try refresh
                var refreshed = await TryRefreshAsync(cancellationToken);
                if (refreshed)
                {
                    try
                    {
                        string? newToken = _tokenStore?.AccessToken;
                        if (string.IsNullOrWhiteSpace(newToken))
                        {
                            try
                            {
                                newToken = await _js.InvokeAsync<string?>("localStorage.getItem", AccessTokenKey);
                                if (!string.IsNullOrWhiteSpace(newToken))
                                    _tokenStore.AccessToken = newToken;
                            }
                            catch
                            {
                                // ignore
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(newToken))
                        {
                            // clone request
                            var newRequest = await CloneHttpRequestMessageAsync(request);
                            newRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken);

                            response.Dispose();
                            return await base.SendAsync(newRequest, cancellationToken);
                        }
                    }
                    catch
                    {
                        // ignore
                    }
                }
            }

            return response;
        }

        private async Task<bool> TryRefreshAsync(CancellationToken ct)
        {
            string? refresh = _tokenStore?.RefreshToken;
            if (string.IsNullOrEmpty(refresh))
            {
                try
                {
                    refresh = await _js.InvokeAsync<string?>("localStorage.getItem", RefreshTokenKey);
                    if (!string.IsNullOrWhiteSpace(refresh))
                        _tokenStore.RefreshToken = refresh;
                }
                catch
                {
                    return false;
                }
            }

            if (string.IsNullOrEmpty(refresh)) return false;

            var apiBase = _config["ApiBaseUrl"] ?? "http://localhost:5285/";
            using var http = new HttpClient { BaseAddress = new Uri(apiBase) };

            try
            {
                var resp = await http.PostAsJsonAsync("api/Auth/refresh", new { RefreshToken = refresh }, ct);
                if (!resp.IsSuccessStatusCode) return false;

                var doc = await resp.Content.ReadFromJsonAsync<fitness_club_front.Models.TokenResponse>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, ct);
                if (doc?.AccessToken == null) return false;

                _tokenStore.AccessToken = doc.AccessToken;
                if (!string.IsNullOrEmpty(doc.RefreshToken))
                    _tokenStore.RefreshToken = doc.RefreshToken;

                try
                {
                    await _js.InvokeVoidAsync("localStorage.setItem", AccessTokenKey, doc.AccessToken);
                    if (!string.IsNullOrEmpty(doc.RefreshToken))
                        await _js.InvokeVoidAsync("localStorage.setItem", RefreshTokenKey, doc.RefreshToken);
                }
                catch
                {
                    // ignore
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private static async Task<HttpRequestMessage> CloneHttpRequestMessageAsync(HttpRequestMessage req)
        {
            var clone = new HttpRequestMessage(req.Method, req.RequestUri);
            // copy request content
            if (req.Content != null)
            {
                var ms = new MemoryStream();
                await req.Content.CopyToAsync(ms);
                ms.Position = 0;
                clone.Content = new StreamContent(ms);
                if (req.Content.Headers != null)
                {
                    foreach (var h in req.Content.Headers)
                        clone.Content.Headers.TryAddWithoutValidation(h.Key, h.Value);
                }
            }

            foreach (var header in req.Headers)
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

            clone.Version = req.Version;
            return clone;
        }
    }
}
