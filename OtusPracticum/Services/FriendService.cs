using Npgsql;
using NpgsqlTypes;

namespace OtusPracticum.Services
{
    public class FriendService(NpgsqlService npgsqlService)
    {
        private readonly NpgsqlService npgsqlService = npgsqlService;

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
        }
    }
}
