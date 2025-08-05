using Microsoft.Extensions.Options;

namespace OtusPracticum.Services
{
    public class UserAuthService(IHttpClientFactory httpClientFactory, IOptions<UserAuthServiceOptions> options)
    {
        private readonly IHttpClientFactory httpClientFactory = httpClientFactory;
        private readonly IOptions<UserAuthServiceOptions> options = options;
        private UserAuthServiceToken? Token { get; set; }

        public async Task<string?> GetTokenAsync()
        {
            if (Token is null || Token.IsExpired)
            {
                using var client = httpClientFactory.CreateClient();
                string url = options.Value.URL;
                var response = await client.PostAsJsonAsync(url, new
                {
                    id = options.Value.User_id,
                    password = options.Value.Password
                });

                Token = await response.Content.ReadFromJsonAsync<UserAuthServiceToken>();
                return Token?.Access_token;
            }
            else
            {
                return Token.Access_token;
            }
        }
    }
    public class UserAuthServiceOptions
    {
        public string URL { get; set; } = null!;
        public string Password { get; set; } = null!;
        public Guid User_id { get; set; }
    }
    public class UserAuthServiceToken
    {
        public string Access_token { get; set; } = null!;
        public int ExpiresIn { get; set; }
        private readonly DateTime _createdAt = DateTime.Now;
        public bool IsExpired => (DateTime.Now - _createdAt).TotalSeconds > ExpiresIn - 120;
    }
}
