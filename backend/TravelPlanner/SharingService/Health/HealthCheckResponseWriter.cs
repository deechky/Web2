using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace SharingService.Health
{
    /// <summary>
    /// Formatira rezultat health check-a kao JSON, umesto podrazumevanog praznog 200/503 odgovora.
    /// Koristi se i za /health/live i za /health/ready.
    /// </summary>
    internal static class HealthCheckResponseWriter
    {
        private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

        public static Task WriteJson(HttpContext context, HealthReport report)
        {
            context.Response.ContentType = "application/json";

            var payload = new
            {
                status = report.Status.ToString(),
                totalDurationMs = report.TotalDuration.TotalMilliseconds,
                checks = report.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    durationMs = e.Value.Duration.TotalMilliseconds,
                    description = e.Value.Description,
                    error = e.Value.Exception?.Message
                })
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(payload, JsonOptions));
        }
    }
}
