using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace TripService.Observability
{
    /// <summary>
    /// Zajednička OpenTelemetry konfiguracija (traces + metrics + logs -> OTel Collector -> Tempo/
    /// Prometheus/Loki, vidi docker-compose.observability.yml). Isti obrazac je dupliran po servisu
    /// jer projekat nema deljenu biblioteku između servisa (analogno Health/HealthCheckResponseWriter.cs).
    /// </summary>
    internal static class ObservabilityExtensions
    {
        public static WebApplicationBuilder AddTravelPlannerObservability(
            this WebApplicationBuilder builder, string serviceName)
        {
            var otlpEndpoint = builder.Configuration["OpenTelemetry:OtlpEndpoint"] ?? "http://localhost:4317";
            var otlpUri = new Uri(otlpEndpoint);

            var resourceBuilder = ResourceBuilder.CreateDefault()
                .AddService(serviceName: serviceName, serviceNamespace: "TravelPlanner")
                .AddAttributes(new[]
                {
                    new KeyValuePair<string, object>("deployment.environment", builder.Environment.EnvironmentName)
                });

            // Logovi i dalje idu i na Console (default provider ostaje) - OTel exporter se dodaje kao
            // dodatni provider, ne zamena, radi lokalnog debug-a bez otvaranja Grafane.
            builder.Logging.AddOpenTelemetry(options =>
            {
                options.SetResourceBuilder(resourceBuilder);
                options.IncludeScopes = true;
                options.IncludeFormattedMessage = true;
                options.AddOtlpExporter(otlp => otlp.Endpoint = otlpUri);
            });

            builder.Services.AddOpenTelemetry()
                .WithTracing(tracing => tracing
                    .SetResourceBuilder(resourceBuilder)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    // Namerno bez opcije za snimanje sirovog SQL teksta (u ovoj verziji paketa je to
                    // gated iza OTEL_DOTNET_EXPERIMENTAL_SQLCLIENT_SET_DBSTATEMENT_FOR_TEXT env var
                    // zbog rizika curenja podataka kroz SQL parametre) - RecordException je dovoljan i
                    // bezbedan default za demonstraciju grešaka/sporih upita.
                    .AddSqlClientInstrumentation(sql => sql.RecordException = true)
                    .AddOtlpExporter(otlp => otlp.Endpoint = otlpUri))
                .WithMetrics(metrics => metrics
                    .SetResourceBuilder(resourceBuilder)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddOtlpExporter(otlp => otlp.Endpoint = otlpUri));

            return builder;
        }
    }
}
