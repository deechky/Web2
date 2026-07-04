using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace SharingService.Services
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

        public async Task<bool> CanShareAsync(Guid tripId, string bearerToken)
        {
            var tripServiceUrl = _configuration["Services:TripServiceUrl"];
            var request = new HttpRequestMessage(HttpMethod.Get, $"{tripServiceUrl}/api/trips/{tripId}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
    }
}
