using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Web;
using System.Globalization;
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

            _logger.LogInformation("TrainingSessionService: Sending GET {Uri}", uri);

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

        // Полное формирование query string из всех полей TrainingSessionQuery
        private static string QueryStringFrom(TrainingSessionQuery q)
        {
            var col = HttpUtility.ParseQueryString(string.Empty);
            var culture = CultureInfo.InvariantCulture;

            if (!string.IsNullOrWhiteSpace(q.SearchTerm)) col["SearchTerm"] = q.SearchTerm;
            if (q.PageNumber != 0) col["PageNumber"] = q.PageNumber.ToString(culture);
            if (q.PageSize != 0) col["PageSize"] = q.PageSize.ToString(culture);
            if (q.TrainerId.HasValue) col["TrainerId"] = q.TrainerId.Value.ToString(culture);
            if (!string.IsNullOrWhiteSpace(q.Specialization)) col["Specialization"] = q.Specialization;

            if (q.StartTimeFrom.HasValue) col["StartTimeFrom"] = q.StartTimeFrom.Value.ToString("o", culture);
            if (q.StartTimeTo.HasValue) col["StartTimeTo"] = q.StartTimeTo.Value.ToString("o", culture);
            if (q.StartTimeHour.HasValue) col["StartTimeHour"] = q.StartTimeHour.Value.ToString(culture);

            if (q.MinPrice.HasValue) col["MinPrice"] = q.MinPrice.Value.ToString(culture);
            if (q.MaxPrice.HasValue) col["MaxPrice"] = q.MaxPrice.Value.ToString(culture);

            if (!string.IsNullOrWhiteSpace(q.SessionType)) col["SessionType"] = q.SessionType;
            if (!string.IsNullOrWhiteSpace(q.DifficultyLevel)) col["DifficultyLevel"] = q.DifficultyLevel;
            if (!string.IsNullOrWhiteSpace(q.FitnessGoal)) col["FitnessGoal"] = q.FitnessGoal;

            if (q.MinTrainerRating.HasValue) col["MinTrainerRating"] = q.MinTrainerRating.Value.ToString(culture);
            if (q.HasAvailableSpots.HasValue) col["HasAvailableSpots"] = q.HasAvailableSpots.Value ? "true" : "false";

            if (!string.IsNullOrWhiteSpace(q.Location)) col["Location"] = q.Location;
            if (q.IsActive.HasValue) col["IsActive"] = q.IsActive.Value ? "true" : "false";

            if (!string.IsNullOrWhiteSpace(q.SortBy)) col["SortBy"] = q.SortBy;
            if (!string.IsNullOrWhiteSpace(q.SortOrder)) col["SortOrder"] = q.SortOrder;

            return col.ToString();
        }
    }
}