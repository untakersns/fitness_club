using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Web;
using fitness_club_front.Models;
using Microsoft.Extensions.Logging;

namespace fitness_club_front.Services
{
    public class TrainingSessionService
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly HttpClient _client;
        private readonly ILogger<TrainingSessionService> _logger;
        private readonly JsonSerializerOptions _jsOptions = new() { PropertyNameCaseInsensitive = true };

        public TrainingSessionService(IHttpClientFactory httpFactory, ILogger<TrainingSessionService> logger)
        {
            _httpFactory = httpFactory;
            _client = _httpFactory.CreateClient("Api");
            _logger = logger;
        }

        public async Task<PageResult<TrainingSessionDto>?> GetSessionsAsync(TrainingSessionQuery? query = null, CancellationToken ct = default)
        {
            var uri = "api/TrainingSessions";
            if (query != null)
            {
                var qs = QueryStringFrom(query);
                if (!string.IsNullOrEmpty(qs)) uri += "?" + qs;
            }

            try
            {
                using var resp = await _client.GetAsync(uri, ct);
                var content = await resp.Content.ReadAsStringAsync(ct);

                if (!resp.IsSuccessStatusCode)
                {
                    _logger.LogWarning("API returned {StatusCode} for {Uri}. Response: {Content}", resp.StatusCode, uri, content);
                    return null;
                }

                try
                {
                    var result = JsonSerializer.Deserialize<PageResult<TrainingSessionDto>>(content, _jsOptions);
                    if (result == null)
                    {
                        _logger.LogWarning("Deserialized result is null for {Uri}. Raw: {Content}", uri, content);
                    }
                    return result;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to deserialize API response from {Uri}. Raw: {Content}", uri, content);
                    return null;
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request to API failed for {Uri}", uri);
                return null;
            }
        }

        public async Task<TrainingSessionDto?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            try
            {
                return await _client.GetFromJsonAsync<TrainingSessionDto>($"api/TrainingSessions/{id}", _jsOptions, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetByIdAsync failed for id {Id}", id);
                return null;
            }
        }

        private static string QueryStringFrom(TrainingSessionQuery q)
        {
            var col = HttpUtility.ParseQueryString(string.Empty);
            if (!string.IsNullOrWhiteSpace(q.SearchTerm)) col["SearchTerm"] = q.SearchTerm;
            if (q.PageNumber != 0) col["PageNumber"] = q.PageNumber.ToString();
            if (q.PageSize != 0) col["PageSize"] = q.PageSize.ToString();
            if (q.TrainerId.HasValue) col["TrainerId"] = q.TrainerId.Value.ToString();
            if (!string.IsNullOrWhiteSpace(q.Specialization)) col["Specialization"] = q.Specialization;
            if (!string.IsNullOrWhiteSpace(q.SessionType)) col["SessionType"] = q.SessionType;
            if (!string.IsNullOrWhiteSpace(q.SortBy)) col["SortBy"] = q.SortBy;
            if (!string.IsNullOrWhiteSpace(q.SortOrder)) col["SortOrder"] = q.SortOrder;
            return col.ToString();
        }
    }
}