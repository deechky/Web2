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

namespace TripService.Tests
{
    /// <summary>
    /// Pokreće pravi Kestrel host preko <see cref="TripServiceApp"/> (ista konfiguracija koju koristi
    /// i produkcija preko Service Fabric-a) na efemernom portu, bez potrebe za SF klasterom. Podrazumevani
    /// connection string gađa pravu lokalnu TripsDB (isto kao appsettings.json) - testovi kojima treba
    /// nedostupna baza je override-uju preko <paramref name="configOverrides"/>.
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

        public static async Task<TestHost> StartAsync(Dictionary<string, string?>? configOverrides = null)
        {
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.UseUrls("http://127.0.0.1:0");

            var config = new Dictionary<string, string?>
            {
                ["ConnectionStrings:TripsDB"] =
                    "Server=localhost;Database=TripsDB;Trusted_Connection=True;TrustServerCertificate=True",
                ["Jwt:Key"] = "TravelPlanner-Super-Secret-Signing-Key-Change-Me",
                ["Jwt:Issuer"] = "TravelPlanner.AuthService",
                ["Jwt:Audience"] = "TravelPlanner",
            };
            if (configOverrides != null)
            {
                foreach (var kv in configOverrides)
                {
                    config[kv.Key] = kv.Value;
                }
            }
            builder.Configuration.AddInMemoryCollection(config);

            TripServiceApp.ConfigureServices(builder);

            var app = builder.Build();
            TripServiceApp.ConfigurePipeline(app);

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
