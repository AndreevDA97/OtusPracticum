using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OtusPracticum.KafkaQueue;
using OtusPracticum.Services;

namespace OtusPracticum.CacheUpdateService
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = Host.CreateDefaultBuilder(args)
                .UseSystemd()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddOptions();
                    services.Configure<KafkaSettings>(hostContext.Configuration.GetSection("KafkaSettings"));
                    services.AddSingleton<NpgsqlService>();
                    services.AddTransient<PostRepository>();
                    services.AddStackExchangeRedisCache(options =>
                    {
                        options.Configuration = hostContext.Configuration.GetConnectionString("RedisCache");
                    });
                    services.AddHostedService<Worker>();
                });

            var host = builder.Build();
            await host.RunAsync();
        }
    }
}
