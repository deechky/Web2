using System;
using Gateway.Health;
using Gateway.Observability;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;

namespace Gateway
{
    /// <summary>
    /// DI registracija i middleware pipeline izdvojeni iz <see cref="Gateway"/> (Service Fabric
    /// listener factory) u testabilne statičke metode - isti razlog/obrazac kao TripServiceApp.
    /// </summary>
    public static class GatewayApp
    {
        public static void ConfigureServices(WebApplicationBuilder builder)
        {
            builder.AddTravelPlannerObservability("gateway");

            builder.Services.AddReverseProxy()
                .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

            // Iste adrese koje ReverseProxy već koristi za rutiranje - readiness Gateway-a
            // znači "downstream servisi kojima prosleđujem saobraćaj su dostupni".
            var clustersSection = builder.Configuration.GetSection("ReverseProxy:Clusters");
            string? ClusterAddress(string clusterId) =>
                clustersSection[$"{clusterId}:Destinations:destination1:Address"]?.TrimEnd('/');

            builder.Services.AddHttpClient(nameof(DownstreamHealthCheck), client =>
            {
                client.Timeout = TimeSpan.FromSeconds(3);
            });

            var healthChecksBuilder = builder.Services.AddHealthChecks();
            foreach (var (clusterId, checkName) in new[]
            {
                ("auth-cluster", "auth-service"),
                ("trip-cluster", "trip-service"),
                ("sharing-cluster", "sharing-service"),
            })
            {
                var address = ClusterAddress(clusterId);
                if (address != null)
                {
                    healthChecksBuilder.AddTypeActivatedCheck<DownstreamHealthCheck>(
                        checkName,
                        failureStatus: null,
                        tags: new[] { "ready" },
                        args: new object[] { address });
                }
            }
        }

        public static void ConfigurePipeline(WebApplication app)
        {
            // /health/live - proces radi, ne proverava zavisnosti (za orkestraciju/restart odluke).
            app.MapHealthChecks("/health/live", new HealthCheckOptions
            {
                Predicate = _ => false,
                ResponseWriter = HealthCheckResponseWriter.WriteJson
            });

            // /health/ready - downstream servisi (Auth/Trip/Sharing) su mrežno dostupni.
            app.MapHealthChecks("/health/ready", new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("ready"),
                ResponseWriter = HealthCheckResponseWriter.WriteJson
            });

            // Namerno bez UseCors ovde - dodavanje CORS-a i na Gateway-u bi dupliralo Access-Control-Allow-Origin sa servisa iza njega.
            app.MapReverseProxy();
        }
    }
}
