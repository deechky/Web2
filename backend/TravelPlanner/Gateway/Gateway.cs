using System;
using System.Collections.Generic;
using System.Fabric;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Gateway.Health;

namespace Gateway
{
    /// <summary>
    /// The FabricRuntime creates an instance of this class for each service type instance.
    /// </summary>
    internal sealed class Gateway : StatelessService
    {
        public Gateway(StatelessServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Optional override to create listeners (like tcp, http) for this service instance.
        /// </summary>
        /// <returns>The collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[]
            {
                new ServiceInstanceListener(serviceContext =>
                    new KestrelCommunicationListener(serviceContext, "ServiceEndpoint", (url, listener) =>
                    {
                        ServiceEventSource.Current.ServiceMessage(serviceContext, $"Starting Kestrel on {url}");

                        var builder = WebApplication.CreateBuilder();

                        builder.Services.AddSingleton<StatelessServiceContext>(serviceContext);

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

                        builder.WebHost
                                    .UseKestrel()
                                    .UseContentRoot(Directory.GetCurrentDirectory())
                                    .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.None)
                                    .UseUrls(url);

                        var app = builder.Build();

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

                        return app;

                    }))
            };
        }
    }
}
