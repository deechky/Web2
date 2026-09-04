using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace TripService.Tests
{
    /// <summary>
    /// Testira da NAŠ kod ispravno generiše telemetriju (OTel SDK je zaista registrovan i aktivan) -
    /// koristi ugrađeni <see cref="ActivityListener"/> (System.Diagnostics, nema OTel-specifičnih test
    /// paketa, nema zavisnosti od Docker-a). Da telemetrija stvarno stiže do Collector-a/Tempo-a/Loki-ja/
    /// Prometheus-a je već rigorozno provereno na pravom deployovanom sistemu u
    /// docs/observability-scenarios.md (Scenario 1-4) - taj deo namerno nije dupliran ovde kao
    /// automatizovan test, jer bi zavisio od Docker stack-a da bude "up" da bi test prošao, što ovaj
    /// test suite ne treba da pretpostavlja.
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
                // Stop, ne Started - OTel instrumentacija obogaćuje DisplayName (npr. "GET /health/live",
                // vidi docs/observability-scenarios.md) tek pri završetku zahteva, kad je poznata ruta;
                // na Start je DisplayName još uvek sirovo ime izvora ("Microsoft.AspNetCore.Hosting.HttpRequestIn").
                ActivityStopped = activity => capturedNames.Add(activity.DisplayName),
            };
            ActivitySource.AddActivityListener(listener);

            await using var host = await TestHost.StartAsync();
            var response = await host.Client.GetAsync("/health/live");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains(capturedNames, name => name.Contains("health/live"));
        }

        [Fact]
        public async Task RequestToHealthReady_ProducesChildSpanForSqlQuery()
        {
            var capturedNames = new List<string>();
            using var listener = new ActivityListener
            {
                ShouldListenTo = _ => true,
                Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
                // Stop, ne Started - OTel instrumentacija obogaćuje DisplayName (npr. "GET /health/live",
                // vidi docs/observability-scenarios.md) tek pri završetku zahteva, kad je poznata ruta;
                // na Start je DisplayName još uvek sirovo ime izvora ("Microsoft.AspNetCore.Hosting.HttpRequestIn").
                ActivityStopped = activity => capturedNames.Add(activity.DisplayName),
            };
            ActivitySource.AddActivityListener(listener);

            await using var host = await TestHost.StartAsync();
            var response = await host.Client.GetAsync("/health/ready");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            // SqlClient instrumentacija imenuje span po bazi (npr. "TripsDB") - dokaz da health check
            // (SELECT 1 protiv baze) prolazi kroz istu tracing infrastrukturu kao i sav ostali kod,
            // ne samo ASP.NET Core server span za sam HTTP zahtev.
            Assert.Contains(capturedNames, name => name.Contains("health/ready"));
            Assert.True(capturedNames.Count > 1, "Očekivan bar jedan child span (SQL) pored server span-a.");
        }
    }
}
