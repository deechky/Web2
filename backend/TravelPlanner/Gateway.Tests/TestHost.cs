using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Gateway.Tests
{
    /// <summary>
    /// Pokreće pravi Kestrel host preko <see cref="GatewayApp"/> (ista konfiguracija koju koristi i
    /// produkcija preko Service Fabric-a) na efemernom portu. YARP zahteva bar jednu rutu po klasteru
    /// da bi konfiguracija bila validna, pa test prosleđuje minimalan ali potpun ReverseProxy config
    /// koji pokazuje na adrese lažnih downstream servera (<see cref="FakeDownstreamServer"/>).
    /// </summary>
    public sealed class TestHost : IAsyncDisposable
    {
        public WebApplication App { get; }
        public HttpClient Client { get; }

        private TestHost(WebApplication app, HttpClient client)
        {
            App = app;
            Client = client;
        }

        public static async Task<TestHost> StartAsync(string authAddress, string tripAddress, string sharingAddress)
        {
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.UseUrls("http://127.0.0.1:0");

            var config = new Dictionary<string, string?>
            {
                ["ReverseProxy:Routes:auth-route:ClusterId"] = "auth-cluster",
                ["ReverseProxy:Routes:auth-route:Match:Path"] = "/api/auth/{**catch-all}",
                ["ReverseProxy:Routes:trips-route:ClusterId"] = "trip-cluster",
                ["ReverseProxy:Routes:trips-route:Match:Path"] = "/api/trips/{**catch-all}",
                ["ReverseProxy:Routes:shares-route:ClusterId"] = "sharing-cluster",
                ["ReverseProxy:Routes:shares-route:Match:Path"] = "/api/shares/{**catch-all}",
                ["ReverseProxy:Clusters:auth-cluster:Destinations:destination1:Address"] = authAddress,
                ["ReverseProxy:Clusters:trip-cluster:Destinations:destination1:Address"] = tripAddress,
                ["ReverseProxy:Clusters:sharing-cluster:Destinations:destination1:Address"] = sharingAddress,
            };
            builder.Configuration.AddInMemoryCollection(config);

            GatewayApp.ConfigureServices(builder);

            var app = builder.Build();
            GatewayApp.ConfigurePipeline(app);

            await app.StartAsync();

            var address = app.Services.GetRequiredService<IServer>()
                .Features.Get<IServerAddressesFeature>()!.Addresses.First();

            var client = new HttpClient { BaseAddress = new Uri(address) };
            return new TestHost(app, client);
        }

        public async ValueTask DisposeAsync()
        {
            Client.Dispose();
            await App.StopAsync();
            await App.DisposeAsync();
        }
    }
}
