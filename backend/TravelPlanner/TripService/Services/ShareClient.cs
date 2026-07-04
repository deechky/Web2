using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace TripService.Services
{
    public class ShareValidationResult
    {
        public Guid PlanId { get; set; }
        public string Tip { get; set; } = string.Empty;
        public bool Valid { get; set; }
    }

    public class ShareClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public ShareClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<ShareValidationResult?> ValidateAsync(string kod)
        {
            var sharingServiceUrl = _configuration["Services:SharingServiceUrl"];
            var response = await _httpClient.GetAsync($"{sharingServiceUrl}/api/shares/{kod}/validate");
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<ShareValidationResult>(
                new JsonSerializerOptions(JsonSerializerDefaults.Web));
        }
    }
}
