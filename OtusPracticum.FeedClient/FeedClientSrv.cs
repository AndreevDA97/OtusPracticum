using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using OtusPracticum.Services;

namespace OtusPracticum.FeedClient
{
    public class FeedClientSrv(UserAuthService userAuthService, IConfiguration configuration) : BackgroundService
    {
        private readonly UserAuthService userAuthService = userAuthService;
        private readonly string signalrHost = configuration["LiveFeedService:URL"]!;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested) 
            {
                await using var connection = new HubConnectionBuilder()
                    .WithUrl(signalrHost, x => x.AccessTokenProvider = async ()
                            => await userAuthService.GetTokenAsync())
                    .WithAutomaticReconnect()
                    .Build();

                connection.On<string>("Receive", Console.WriteLine);
                await connection.StartAsync(stoppingToken);
                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }
    }
}
