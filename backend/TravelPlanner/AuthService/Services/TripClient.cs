using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace AuthService.Services
{
    public class TripClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public TripClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task DeleteUserTripsAsync(Guid userId)
        {
            var tripServiceUrl = _configuration["Services:TripServiceUrl"];
            var internalKey = _configuration["Internal:ApiKey"];

            var request = new HttpRequestMessage(HttpMethod.Delete, $"{tripServiceUrl}/api/internal/trips/by-user/{userId}");
            request.Headers.Add("X-Internal-Key", internalKey);

            await _httpClient.SendAsync(request);
        }
    }
}
