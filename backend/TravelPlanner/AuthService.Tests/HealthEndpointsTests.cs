using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace AuthService.Tests
{
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
        public async Task HealthReady_WhenDatabaseReachable_ReturnsHealthy()
        {
            await using var host = await TestHost.StartAsync();

            var response = await host.Client.GetAsync("/health/ready");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal("Healthy", doc.RootElement.GetProperty("status").GetString());

            var checks = doc.RootElement.GetProperty("checks").EnumerateArray().ToList();
            var dbCheck = Assert.Single(checks);
            Assert.Equal("sqlserver-usersdb", dbCheck.GetProperty("name").GetString());
            Assert.Equal("Healthy", dbCheck.GetProperty("status").GetString());
        }

        [Fact]
        public async Task HealthReady_WhenDatabaseUnreachable_ReturnsUnhealthyWith503()
        {
            await using var host = await TestHost.StartAsync(new()
            {
                ["ConnectionStrings:UsersDB"] =
                    "Server=localhost,59999;Database=UsersDB;Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=2",
            });

            var response = await host.Client.GetAsync("/health/ready");

            Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal("Unhealthy", doc.RootElement.GetProperty("status").GetString());

            var checks = doc.RootElement.GetProperty("checks").EnumerateArray().ToList();
            var dbCheck = Assert.Single(checks);
            Assert.Equal("Unhealthy", dbCheck.GetProperty("status").GetString());
        }
    }
}
