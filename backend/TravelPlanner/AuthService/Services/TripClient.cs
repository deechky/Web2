using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AuthService.Services
{
    public class TripClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TripClient> _logger;

        public TripClient(HttpClient httpClient, IConfiguration configuration, ILogger<TripClient> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> DeleteUserTripsAsync(Guid userId)
        {
            var tripServiceUrl = _configuration["Services:TripServiceUrl"];
            var internalKey = _configuration["Internal:ApiKey"];

            var request = new HttpRequestMessage(HttpMethod.Delete, $"{tripServiceUrl}/api/internal/trips/by-user/{userId}");
            request.Headers.Add("X-Internal-Key", internalKey);

            try
            {
                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    // Ranije se ovo tiho gutalo (samo "return false") - bez ovoga, brisanje naloga koje
                    // ostavi TripService nekonzistentnim (planovi obrisanog korisnika i dalje postoje)
                    // ne bi ostavilo NIKAKAV trag.
                    _logger.LogWarning(
                        "TripService je odbio brisanje planova korisnika {UserId}: HTTP {StatusCode}.",
                        userId, (int)response.StatusCode);
                }
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex,
                    "Brisanje planova korisnika {UserId} u TripService nije uspelo - servis nedostupan.",
                    userId);
                return false;
            }
        }
    }
}
