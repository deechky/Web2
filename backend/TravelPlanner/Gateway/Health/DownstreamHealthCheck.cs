using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Gateway.Health
{
    /// <summary>
    /// Gateway nema sopstvenu bazu, pa njegov "ready" status ne znači "moja baza radi" već
    /// "servisi kojima prosleđujem saobraćaj su mrežno dostupni". Svaki downstream servis i dalje
    /// samostalno prijavljuje sopstveno zdravlje (uključujući svoju bazu) na svom /health/ready -
    /// ovaj check namerno ne agregira tu dubinu, samo dostupnost.
    /// </summary>
    internal sealed class DownstreamHealthCheck : IHealthCheck
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _serviceUrl;

        public DownstreamHealthCheck(IHttpClientFactory httpClientFactory, string serviceUrl)
        {
            _httpClientFactory = httpClientFactory;
            _serviceUrl = serviceUrl;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            var client = _httpClientFactory.CreateClient(nameof(DownstreamHealthCheck));

            try
            {
                var response = await client.GetAsync($"{_serviceUrl}/health/live", cancellationToken);
                return response.IsSuccessStatusCode
                    ? HealthCheckResult.Healthy($"{_serviceUrl} je dostupan.")
                    : HealthCheckResult.Unhealthy($"{_serviceUrl} je odgovorio sa statusom {(int)response.StatusCode}.");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy($"{_serviceUrl} nije dostupan.", ex);
            }
        }
    }
}
