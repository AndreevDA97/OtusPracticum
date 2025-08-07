using Confluent.Kafka;
using Npgsql;
using NpgsqlTypes;
using OtusPracticum.KafkaQueue.Models;
using OtusPracticum.KafkaQueue;
using System.Text.Json;

namespace OtusPracticum.Services
{
    public class FriendService(NpgsqlService npgsqlService, KafkaProducer<string, string> kafkaProducer)
    {
        private readonly NpgsqlService npgsqlService = npgsqlService;
        private readonly KafkaProducer<string, string> kafkaProducer = kafkaProducer;

        public async Task AddFriendAsync(Guid user_id, Guid friend_id)
        {
            string query = @"INSERT INTO public.friends (user_id, friend_id)
                                VALUES (@User_id, @Friend_id)";
            var parameters = new NpgsqlParameter[]
            {
                new("User_id", NpgsqlDbType.Uuid) { Value = user_id },
                new("Friend_id", NpgsqlDbType.Uuid) { Value = friend_id },
            };
            await npgsqlService.ExecuteNonQueryAsync(query, parameters);
            var message = new Message<string, string>
            {
                Key = user_id.ToString(),
                Value = JsonSerializer.Serialize(
                    new FeedUpdateMessage(ActionTypeEnum.FullReload, null, user_id, null),
                    Consts.JsonSerializerOptions),
                Timestamp = Timestamp.Default
            };
            await kafkaProducer.ProduceAsync("feed-posts", message);
        }

        public async Task DeleteFriendAsync(Guid user_id, Guid friend_id)
        {
            string query = @"DELETE FROM public.friends
                             WHERE user_id = @User_id and friend_id = @Friend_id";
            var parameters = new NpgsqlParameter[]
            {
                new("User_id", NpgsqlDbType.Uuid) { Value = user_id },
                new("Friend_id", NpgsqlDbType.Uuid) { Value = friend_id },
            };
            await npgsqlService.ExecuteNonQueryAsync(query, parameters);
            var message = new Message<string, string>
            {
                Key = user_id.ToString(),
                Value = JsonSerializer.Serialize(
                    new FeedUpdateMessage(ActionTypeEnum.FullReload, null, user_id, null), 
                    Consts.JsonSerializerOptions),
                Timestamp = Timestamp.Default
            };
            await kafkaProducer.ProduceAsync("feed-posts", message);
        }

        public async Task<List<Guid>> GetFriendsAsync(Guid user_id)
        {
            string query = @"select user_id from public.friends
                             where friend_id = @Friend_id";

            var parameters = new NpgsqlParameter[]
            {
                new("Friend_id", NpgsqlDbType.Uuid) { Value = user_id},
            };
            var data = await npgsqlService.GetQueryResultAsync(query, parameters, ["user_id"], TargetSessionAttributes.PreferStandby);
            if (data.Count == 0) return [];
            var posts = new List<Guid>();
            foreach (var row in data)
            {
                posts.Add(Guid.Parse(row["user_id"].ToString()!));
            }
            return posts;
        }
    }
}
