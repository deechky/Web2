using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Gateway.Tests
{
    /// <summary>
    /// Minimalan pravi HTTP server koji odgovara na /health/live - simulira jedan od tri downstream
    /// servisa (Auth/Trip/Sharing) koje Gateway-ov /health/ready proverava. Gateway ne dodiruje bazu
    /// direktno, pa mu je "downstream nedostupan" pravi analog "baza nedostupna" iz ostala tri servisa.
    /// </summary>
    public sealed class FakeDownstreamServer : IAsyncDisposable
    {
        private readonly WebApplication _app;
        public string Address { get; }

        private FakeDownstreamServer(WebApplication app, string address)
        {
            _app = app;
            Address = address;
        }

        public static async Task<FakeDownstreamServer> StartAsync(bool healthy = true)
        {
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.UseUrls("http://127.0.0.1:0");
            builder.Logging.ClearProviders();

            var app = builder.Build();
            app.MapGet("/health/live", () => healthy ? Results.Ok() : Results.StatusCode(503));

            await app.StartAsync();

            var address = app.Services.GetRequiredService<IServer>()
                .Features.Get<IServerAddressesFeature>()!.Addresses.First();

            return new FakeDownstreamServer(app, address);
        }

        public async ValueTask DisposeAsync()
        {
            await _app.StopAsync();
            await _app.DisposeAsync();
        }
    }
}
