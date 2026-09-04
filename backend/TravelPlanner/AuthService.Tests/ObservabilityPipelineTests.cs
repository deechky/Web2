using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace AuthService.Tests
{
    /// <summary>
    /// Vidi TripService.Tests/ObservabilityPipelineTests.cs za napomenu o obimu - stvarna isporuka do
    /// Collector-a/Tempo-a/Loki-ja/Prometheus-a je pokrivena docs/observability-scenarios.md, ne ovde.
    /// </summary>
    public class ObservabilityPipelineTests
    {
        [Fact]
        public async Task RequestToHealthLive_ProducesActivityForTheRequest()
        {
            var capturedNames = new List<string>();
            using var listener = new ActivityListener
            {
                ShouldListenTo = _ => true,
                Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
                ActivityStopped = activity => capturedNames.Add(activity.DisplayName),
            };
            ActivitySource.AddActivityListener(listener);

            await using var host = await TestHost.StartAsync();
            var response = await host.Client.GetAsync("/health/live");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains(capturedNames, name => name.Contains("health/live"));
        }
    }
}
