using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OtusPracticum.Services;

namespace OtusPracticum.FeedClient
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var app = CreateHostBuilder(args);
            await app.RunAsync();
        }
        public static IHost CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .UseSystemd()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddOptions();
                    services.Configure<UserAuthServiceOptions>(hostContext.Configuration.GetSection("AuthService"));
                    services.AddHttpClient<UserAuthService>();
                    services.AddScoped<UserAuthService>();
                    services.AddHostedService<FeedClientSrv>();
                }).Build();
        }
    }
}
