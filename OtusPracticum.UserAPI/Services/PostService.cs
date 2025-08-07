using Confluent.Kafka;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Hosting;
using Npgsql;
using NpgsqlTypes;
using OtusPracticum.Entities;
using OtusPracticum.KafkaQueue;
using OtusPracticum.KafkaQueue.Models;
using System.Text.Json;

namespace OtusPracticum.Services
{
    public class PostService(
        PostRepository postRepo, 
        IDistributedCache distributedCache, 
        KafkaProducer<string, string> kafkaProducer,
        FriendService friendService)
    {
        private readonly PostRepository postRepo = postRepo;
        private readonly KafkaProducer<string, string> kafkaProducer = kafkaProducer;

        public async Task<Guid> AddPostAsync(Guid user_id, string post)
        {
            var post_id = await postRepo.AddPostAsync(user_id, post);
            var friends = await friendService.GetFriendsAsync(user_id);
            foreach (var friend in friends)
            {
                var message = new Message<string, string>
                {
                    Key = friend.ToString(),
                    Value = JsonSerializer.Serialize(new FeedUpdateMessage(ActionTypeEnum.Create, post_id, user_id, post), Consts.JsonSerializerOptions),
                    Timestamp = Timestamp.Default
                };
                await kafkaProducer.ProduceAsync("feed-posts", message);
            }
            return post_id;
        }

        public async Task UpdatePostAsync(Guid post_id, string post, Guid user_id)
        {
            await postRepo.UpdatePostAsync(post_id, post, user_id);
            var friends = await friendService.GetFriendsAsync(user_id);
            foreach (var friend in friends)
            {
                var message = new Message<string, string>
                {
                    Key = friend.ToString(),
                    Value = JsonSerializer.Serialize(new FeedUpdateMessage(ActionTypeEnum.Update, post_id, user_id, post), Consts.JsonSerializerOptions),
                    Timestamp = Timestamp.Default
                };
                await kafkaProducer.ProduceAsync("feed-posts", message);
            }
        }

        public async Task DeletePostAsync(Guid post_id, Guid user_id)
        {
            await postRepo.DeletePostAsync(post_id, user_id);
            var friends = await friendService.GetFriendsAsync(user_id);
            foreach (var friend in friends)
            {
                var message = new Message<string, string>
                {
                    Key = friend.ToString(),
                    Value = JsonSerializer.Serialize(new FeedUpdateMessage(ActionTypeEnum.Delete, post_id, user_id, null), Consts.JsonSerializerOptions),
                    Timestamp = Timestamp.Default
                };
                await kafkaProducer.ProduceAsync("feed-posts", message);
            }
        }

        public async Task<Post?> GetPostAsync(Guid post_id)
        {
            return await postRepo.GetPostAsync(post_id);
        }

        public async Task<IEnumerable<Post>> GetFeedAsync(Guid user_id, int offset, int limit)
        {
            string key = $"feed-{user_id}";
            List<Post>? cachedFeed = null;
            var cachedFeedJson = await distributedCache.GetStringAsync(key);
            if (cachedFeedJson != null)
            {
                cachedFeed = JsonSerializer.Deserialize<List<Post>>(cachedFeedJson, Consts.JsonSerializerOptions);
            }
            // если кеша нет или в нем нет нужного кол-ва данных, то берем из БД
            return cachedFeed != null && cachedFeed.Count >= offset + limit
                ? cachedFeed.OrderByDescending(f => f.Creation_datetime).Skip(offset).Take(limit)
                : await postRepo.GetFeedAsync(user_id, offset, limit);
        }
    }
}
