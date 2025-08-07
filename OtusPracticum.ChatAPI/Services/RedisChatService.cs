using OtusPracticum.Entities;
using OtusPracticum.Models;
using StackExchange.Redis;
using System.Text.Json;

namespace OtusPracticum.Services
{
    public class RedisChatService : IChatService, IDisposable, IAsyncDisposable
    {
        private readonly ConnectionMultiplexer redis;
        private readonly JsonSerializerOptions jsonOptions = new(JsonSerializerDefaults.Web);
        private readonly UserServiceClient userService;

        public RedisChatService(IConfiguration configuration, UserServiceClient userService)
        {
            var connectionString = configuration.GetConnectionString("RedisCache")!;
            redis = ConnectionMultiplexer.Connect(connectionString);
            this.userService = userService;
        }

        public async Task<Guid> CreateChatAsync(CreateChatRequest request, Guid creator_id)
        {
            var db = redis.GetDatabase(0);
            if (!request.Users_ids.Contains(creator_id))
            {
                request.Users_ids.Add(creator_id);
            }

            var chat = new RedisChat
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Users = request.Users_ids,
                Created_at = DateTime.Now
            };
            await db.ExecuteAsync("FCALL", "create_something", 1, $"chat-{chat.Id}", JsonSerializer.Serialize(chat, jsonOptions));

            var chatEntity = new Chat
            {
                Chat_id = chat.Id,
                Chat_name = chat.Name,
                Creation_datetime = chat.Created_at,
                Creator_id = creator_id
            };

            foreach (var user_id in request.Users_ids)
            {
                var remoteUser = await userService.GetUserAsync(user_id);
                if (remoteUser is null) continue;

                var user_chats_res = await db.ExecuteAsync("FCALL", "get_something", 1, $"user_chats-{user_id}");
                var user_chats_str = user_chats_res.ToString();

                if (string.IsNullOrWhiteSpace(user_chats_str))
                {
                    var new_user_chats = new RedisUserChats
                    {
                        User_id = user_id,
                        Chats = [chatEntity]
                    };
                    await db.ExecuteAsync("FCALL", "create_something", 1, $"user_chats-{user_id}", JsonSerializer.Serialize(new_user_chats, jsonOptions));
                    continue;
                }
                var user_chats = JsonSerializer.Deserialize<RedisUserChats>(user_chats_str, jsonOptions)!;
                user_chats.Chats.Add(chatEntity);
                await db.ExecuteAsync("FCALL", "create_something", 1, $"user_chats-{user_id}", JsonSerializer.Serialize(user_chats, jsonOptions));
            }

            return chat.Id;
        }

        public async Task<MessageEntity[]> GetChatAsync(Guid chat_id, int limit, int offset, Guid user_id)
        {
            var db = redis.GetDatabase(0);
            var user_chats_res = await db.ExecuteAsync("FCALL", "get_something", 1, $"chat-{chat_id}");
            var user_chat = JsonSerializer.Deserialize<RedisChat>(user_chats_res.ToString(), jsonOptions);
            return user_chat!.Messages
                .OrderByDescending(m => m.Created_at)
                .Skip(offset)
                .Take(limit)
                .Select(m => new MessageEntity
                {
                    Chat_id = chat_id,
                    Creation_datetime = m.Created_at,
                    Message = m.Text,
                    User_id = m.User_id,
                    Message_id = m.Message_id
                }).ToArray();
        }

        public async Task<List<Chat>> GetUserChatListAsync(Guid user_id, int limit, int offset)
        {
            var db = redis.GetDatabase(0);
            var user_chats_res = await db.ExecuteAsync("FCALL", "get_something", 1, $"user_chats-{user_id}");
            var user_chats = JsonSerializer.Deserialize<RedisUserChats>(user_chats_res.ToString(), jsonOptions);
            return user_chats!.Chats.OrderByDescending(m => m.Creation_datetime).Skip(offset).Take(limit).ToList();
        }

        public async Task<Guid> SendMessageToChatAsync(Guid chat_id, SendMessageRequest request, Guid creator_id)
        {
            var db = redis.GetDatabase(0);
            var chat_res = await db.ExecuteAsync("FCALL", "get_something", 1, $"chat-{chat_id}");
            var chat = JsonSerializer.Deserialize<RedisChat>(chat_res.ToString(), jsonOptions)!;
            var msg = new RedisChatMessage
            {
                Created_at = DateTime.Now,
                Message_id = Guid.NewGuid(),
                Text = request.Message,
                User_id = creator_id
            };
            chat.Messages.Add(msg);
            await db.ExecuteAsync("FCALL", "create_something", 1, $"chat-{chat_id}", JsonSerializer.Serialize(chat, jsonOptions));
            return msg.Message_id;
        }

        public void Dispose()
        {
            redis.Dispose();
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            await redis.DisposeAsync();
            GC.SuppressFinalize(this);
        }
    }
}
