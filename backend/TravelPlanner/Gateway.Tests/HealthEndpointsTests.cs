using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Gateway.Tests
{
    public class HealthEndpointsTests
    {
        [Fact]
        public async Task HealthLive_ReturnsHealthyWithNoChecks()
        {
            await using var auth = await FakeDownstreamServer.StartAsync();
            await using var trip = await FakeDownstreamServer.StartAsync();
            await using var sharing = await FakeDownstreamServer.StartAsync();
            await using var host = await TestHost.StartAsync(auth.Address, trip.Address, sharing.Address);

            var response = await host.Client.GetAsync("/health/live");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal("Healthy", doc.RootElement.GetProperty("status").GetString());
            Assert.Empty(doc.RootElement.GetProperty("checks").EnumerateArray());
        }

        [Fact]
        public async Task HealthReady_WhenAllDownstreamServicesReachable_ReturnsHealthy()
        {
            await using var auth = await FakeDownstreamServer.StartAsync();
            await using var trip = await FakeDownstreamServer.StartAsync();
            await using var sharing = await FakeDownstreamServer.StartAsync();
            await using var host = await TestHost.StartAsync(auth.Address, trip.Address, sharing.Address);

            var response = await host.Client.GetAsync("/health/ready");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal("Healthy", doc.RootElement.GetProperty("status").GetString());

            var checks = doc.RootElement.GetProperty("checks").EnumerateArray().ToList();
            Assert.Equal(3, checks.Count);
            Assert.All(checks, c => Assert.Equal("Healthy", c.GetProperty("status").GetString()));
            Assert.Contains(checks, c => c.GetProperty("name").GetString() == "auth-service");
            Assert.Contains(checks, c => c.GetProperty("name").GetString() == "trip-service");
            Assert.Contains(checks, c => c.GetProperty("name").GetString() == "sharing-service");
        }

        [Fact]
        public async Task HealthReady_WhenOneDownstreamServiceUnreachable_ReturnsUnhealthyWith503()
        {
            await using var auth = await FakeDownstreamServer.StartAsync();
            await using var sharing = await FakeDownstreamServer.StartAsync();

            // trip-service je "nedostupan" - server je pokrenut pa odmah ugašen, port sigurno ne
            // prihvata konekcije (portabilnija garancija nego oslanjanje na fiksni "sigurno zatvoren"
            // port kroz različita okruženja).
            var deadServer = await FakeDownstreamServer.StartAsync();
            var deadAddress = deadServer.Address;
            await deadServer.DisposeAsync();

            await using var host = await TestHost.StartAsync(auth.Address, deadAddress, sharing.Address);

            var response = await host.Client.GetAsync("/health/ready");

            Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal("Unhealthy", doc.RootElement.GetProperty("status").GetString());

            var checks = doc.RootElement.GetProperty("checks").EnumerateArray().ToList();
            var tripCheck = checks.Single(c => c.GetProperty("name").GetString() == "trip-service");
            Assert.Equal("Unhealthy", tripCheck.GetProperty("status").GetString());
            // auth i sharing su i dalje zdravi - jedan pao downstream ne sme da obori ostale check-ove.
            Assert.Equal("Healthy", checks.Single(c => c.GetProperty("name").GetString() == "auth-service").GetProperty("status").GetString());
            Assert.Equal("Healthy", checks.Single(c => c.GetProperty("name").GetString() == "sharing-service").GetProperty("status").GetString());
        }
    }
}
