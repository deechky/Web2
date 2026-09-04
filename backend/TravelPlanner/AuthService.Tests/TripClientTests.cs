using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AuthService.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Xunit;

namespace AuthService.Tests
{
    /// <summary>
    /// Regresioni test za AS-IS nalaz: TripClient.DeleteUserTripsAsync je ranije gutao greške bez
    /// ijednog loga (catch (HttpRequestException) { return false; }). Ovi testovi dokazuju da je fix
    /// (LogWarning na neuspešan HTTP status, LogError na izuzetak) stvaran, ne samo da kod ne baca.
    /// </summary>
    public class TripClientTests
    {
        private static TripClient CreateClient(HttpMessageHandler handler, CapturingLogger<TripClient> logger)
        {
            var httpClient = new HttpClient(handler);
            var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Services:TripServiceUrl"] = "http://localhost:8726",
                ["Internal:ApiKey"] = "test-key",
            }).Build();
            return new TripClient(httpClient, config, logger);
        }

        [Fact]
        public async Task DeleteUserTripsAsync_WhenSuccessful_ReturnsTrueAndLogsNothing()
        {
            var logger = new CapturingLogger<TripClient>();
            var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NoContent));
            var client = CreateClient(handler, logger);

            var result = await client.DeleteUserTripsAsync(Guid.NewGuid());

            Assert.True(result);
            Assert.Empty(logger.Entries);
        }

        [Fact]
        public async Task DeleteUserTripsAsync_WhenTripServiceReturnsError_ReturnsFalseAndLogsWarning()
        {
            var logger = new CapturingLogger<TripClient>();
            var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));
            var client = CreateClient(handler, logger);

            var result = await client.DeleteUserTripsAsync(Guid.NewGuid());

            Assert.False(result);
            Assert.Contains(logger.Entries, e => e.Level == LogLevel.Warning);
        }

        [Fact]
        public async Task DeleteUserTripsAsync_WhenTripServiceUnreachable_ReturnsFalseAndLogsError()
        {
            var logger = new CapturingLogger<TripClient>();
            var handler = new FakeThrowingHandler();
            var client = CreateClient(handler, logger);

            var result = await client.DeleteUserTripsAsync(Guid.NewGuid());

            Assert.False(result);
            Assert.Contains(logger.Entries, e => e.Level == LogLevel.Error);
        }

        private sealed class FakeHttpMessageHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;
            public FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) => _responder = responder;

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
                => Task.FromResult(_responder(request));
        }

        private sealed class FakeThrowingHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
                => throw new HttpRequestException("simulirani mrežni kvar (test)");
        }

        private sealed class CapturingLogger<T> : ILogger<T>
        {
            public List<(LogLevel Level, string Message)> Entries { get; } = new();

            public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
                Func<TState, Exception?, string> formatter)
                => Entries.Add((logLevel, formatter(state, exception)));

            private sealed class NullScope : IDisposable
            {
                public static readonly NullScope Instance = new();
                public void Dispose() { }
            }
        }
    }
}
