using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace TripService.Tests
{
    /// <summary>
    /// Integracioni testovi - pravi Kestrel host, prava lokalna SQL Server baza (TripsDB). Pokrivaju
    /// tačno ono što je Milestone 1 (health checks) obećao: /health/live nezavisan od zavisnosti,
    /// /health/ready tačno odražava dostupnost baze (i pozitivno i negativno).
    /// </summary>
    public class HealthEndpointsTests
    {
        [Fact]
        public async Task HealthLive_ReturnsHealthyWithNoChecks()
        {
            await using var host = await TestHost.StartAsync();

            var response = await host.Client.GetAsync("/health/live");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal("Healthy", doc.RootElement.GetProperty("status").GetString());
            Assert.Empty(doc.RootElement.GetProperty("checks").EnumerateArray());
        }

        [Fact]
        public async Task HealthLive_DoesNotDependOnDatabase()
        {
            // Namerno neispravan connection string - /health/live i dalje mora biti Healthy jer ne
            // dodiruje zavisnosti (Predicate = _ => false u TripServiceApp.ConfigurePipeline).
            await using var host = await TestHost.StartAsync(new()
            {
                ["ConnectionStrings:TripsDB"] =
                    "Server=localhost,59999;Database=TripsDB;Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=2",
            });

            var response = await host.Client.GetAsync("/health/live");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task HealthReady_WhenDatabaseReachable_ReturnsHealthy()
        {
            await using var host = await TestHost.StartAsync();

            var response = await host.Client.GetAsync("/health/ready");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal("Healthy", doc.RootElement.GetProperty("status").GetString());

            var checks = doc.RootElement.GetProperty("checks").EnumerateArray().ToList();
            var dbCheck = Assert.Single(checks);
            Assert.Equal("sqlserver-tripsdb", dbCheck.GetProperty("name").GetString());
            Assert.Equal("Healthy", dbCheck.GetProperty("status").GetString());
        }

        [Fact]
        public async Task HealthReady_WhenDatabaseUnreachable_ReturnsUnhealthyWith503()
        {
            await using var host = await TestHost.StartAsync(new()
            {
                ["ConnectionStrings:TripsDB"] =
                    "Server=localhost,59999;Database=TripsDB;Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=2",
            });

            var response = await host.Client.GetAsync("/health/ready");

            Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal("Unhealthy", doc.RootElement.GetProperty("status").GetString());

            var checks = doc.RootElement.GetProperty("checks").EnumerateArray().ToList();
            var dbCheck = Assert.Single(checks);
            Assert.Equal("Unhealthy", dbCheck.GetProperty("status").GetString());
            Assert.False(string.IsNullOrEmpty(dbCheck.GetProperty("error").GetString()));
        }
    }
}
