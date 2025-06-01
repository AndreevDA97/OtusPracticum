using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OtusPracticum.Services;

namespace OtusPracticum.СacheWarmup
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var app = Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddScoped<NpgsqlService>();
                    services.AddScoped<CacheWormuper>();
                    services.AddStackExchangeRedisCache(options =>
                    {
                        options.Configuration = hostContext.Configuration.GetConnectionString("RedisCache");
                    });
                }).Build();
            using var scope = app.Services.CreateScope();
            var generator = scope.ServiceProvider.GetRequiredService<CacheWormuper>();
            await generator.WarmupAsync();
        }
    }
}
