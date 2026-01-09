using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace fitness_club_front.Services
{
    public class LoggingHandler : DelegatingHandler
    {
        private readonly ILogger<LoggingHandler> _logger;
        public LoggingHandler(ILogger<LoggingHandler> logger) => _logger = logger;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Api => {Method} {Uri}", request.Method, request.RequestUri);

            if (request.Headers.Authorization != null)
            {
                _logger.LogInformation("Api => Authorization header present: {Scheme} [***REDACTED***]", request.Headers.Authorization.Scheme);
            }
            else
            {
                _logger.LogWarning("Api => No Authorization header present");
            }

            var response = await base.SendAsync(request, cancellationToken);

            _logger.LogInformation("Api <= {StatusCode} for {Uri}", (int)response.StatusCode, request.RequestUri);
            return response;
        }
    }
}
