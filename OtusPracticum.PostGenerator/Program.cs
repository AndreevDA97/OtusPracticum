using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OtusPracticum.Services;

namespace OtusPracticum.PostGenerator
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var app = Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddScoped(_ => new NpgsqlService(hostContext.Configuration, NpgsqlDatabase.UserService));
                    services.AddScoped<PostGenerator>();
                }).Build();
            using var scope = app.Services.CreateScope();
            var generator = scope.ServiceProvider.GetRequiredService<PostGenerator>();
            await generator.GeneratePostsAsync();
        }
    }
}
