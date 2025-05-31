using Npgsql;
using NpgsqlTypes;
using OtusPracticum.Entities;

namespace OtusPracticum.Services
{
    public class PostService(NpgsqlService npgsqlService)
    {
        private readonly NpgsqlService npgsqlService = npgsqlService;

        public async Task<Guid> AddPostAsync(Guid user_id, string post)
        {
            string query = @"INSERT INTO public.posts (post_id, user_id, post)
                                VALUES (@Post_id, @User_id, @Post)";
            var post_id = Guid.NewGuid();
            var parameters = new NpgsqlParameter[]
            {
                new("Post_id", NpgsqlDbType.Uuid) { Value = post_id },
                new("User_id", NpgsqlDbType.Uuid) { Value = user_id },
                new("Post", NpgsqlDbType.Varchar) { Value = post }
            };
            await npgsqlService.ExecuteNonQueryAsync(query, parameters);
            return post_id;
        }

        public async Task UpdatePostAsync(Guid post_id, string post, Guid user_id)
        {
            string query = @"UPDATE public.posts
                             SET post = @Post
                             WHERE post_id = @Post_id and user_id = @User_id";
            var parameters = new NpgsqlParameter[]
            {
                new("Post_id", NpgsqlDbType.Uuid) { Value = post_id },
                new("User_id", NpgsqlDbType.Uuid) { Value = user_id },
                new("Post", NpgsqlDbType.Varchar) { Value = post }
            };
            await npgsqlService.ExecuteNonQueryAsync(query, parameters);
        }

        public async Task DeletePostAsync(Guid post_id, Guid user_id)
        {
            string query = @"DELETE FROM public.posts
                             WHERE post_id = @Post_id and user_id = @User_id";
            var parameters = new NpgsqlParameter[]
            {
                new("Post_id", NpgsqlDbType.Uuid) { Value = post_id },
                new("User_id", NpgsqlDbType.Uuid) { Value = user_id }
            };
            await npgsqlService.ExecuteNonQueryAsync(query, parameters);
        }

        public async Task<Post?> GetPostAsync(Guid post_id)
        {
            string query = @"SELECT user_id, post, creation_datetime FROM public.posts
                             WHERE post_id = @Post_id";
            var parameters = new NpgsqlParameter[]
            {
                new("Post_id", NpgsqlDbType.Uuid) { Value = post_id }
            };
            var data = await npgsqlService.GetQueryResultAsync(query, parameters,
                ["user_id", "post", "creation_datetime"], TargetSessionAttributes.PreferStandby);
            if (data.Count == 0) return null;
            return new Post(post_id, data[0]);
        }

        public async Task<List<Post>> GetFeedAsync(Guid user_id, int offset, int limit)
        {
            string query = @"SELECT p.user_id, p.post_id, p.creation_datetime, p.post 
                             FROM friends f
                             INNER JOIN posts p ON f.friend_id = p.user_id
                             WHERE f.user_id = @User_id
                             ORDER BY p.creation_datetime DESC
                             LIMIT @Limit OFFSET @Offset";
            var parameters = new NpgsqlParameter[]
            {
                new("User_id", NpgsqlDbType.Uuid) { Value = user_id },
                new("Limit", NpgsqlDbType.Integer) { Value = limit },
                new("Offset", NpgsqlDbType.Integer) { Value = offset }
            };
            var data = await npgsqlService.GetQueryResultAsync(query, parameters,
                ["user_id", "post", "creation_datetime", "post_id"], TargetSessionAttributes.PreferStandby);
            if (data.Count == 0) return [];
            var posts = new List<Post>();
            foreach (var post in data)
            {
                posts.Add(new Post(post));
            }
            return posts;
        }
    }
}
