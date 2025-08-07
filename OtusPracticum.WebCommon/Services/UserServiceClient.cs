using Microsoft.Extensions.Configuration;
using OtusPracticum.Entities;
using System.Net.Http.Json;

namespace OtusPracticum.Services
{
    public class UserServiceClient(IHttpClientFactory clientFactory, IConfiguration configuration)
    {
        private readonly IHttpClientFactory _httpClientFactory = clientFactory;
        private readonly string _url = configuration["UserService:URL"]!;

        public async Task<User?> GetUserAsync(Guid id)
        {
            using var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"{_url}/api/user/get/{id}");
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<User>();
        }
    }
}
