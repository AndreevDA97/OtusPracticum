using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace OtusPracticum.LiveFeedService.Controllers
{
    [Authorize]
    public class FeedHub : Hub
    {
        public async Task Send(string message, string userId)
        {
            bool canPublishMessages = Context.User!.Claims
                .FirstOrDefault(c => c.Type == "can_publish_messages")?.Value == bool.TrueString;
            Console.WriteLine($"User {userId} canPublishMessages - {canPublishMessages} message {message}");
            if (canPublishMessages)
            {
                await Clients.User(userId).SendAsync("Receive", message);
            }
        }
    }
}
