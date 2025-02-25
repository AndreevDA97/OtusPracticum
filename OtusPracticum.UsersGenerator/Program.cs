using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OtusPracticum.Services;

namespace OtusPracticum.UsersGenerator
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var app = Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddScoped<NpgsqlService>();
                    services.AddScoped<UsersGenerator>();
                }).Build();
            using var scope = app.Services.CreateScope();
            var generator = scope.ServiceProvider.GetRequiredService<UsersGenerator>();
            await generator.GenerateUsersAsync();
        }
    }
}
