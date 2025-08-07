using Npgsql;
using NpgsqlTypes;
using OtusPracticum.Entities;
using OtusPracticum.Helpers;
using OtusPracticum.Models;

namespace OtusPracticum.Services
{
    public class UserService(NpgsqlService npgsqlService)
    {
        private readonly NpgsqlService npgsqlService = npgsqlService;

        public async Task<User?> GetUserAsync(Guid id)
        {
            string query = @"SELECT first_name, second_name, birthdate, biography, city, password, can_publish_messages
                             FROM public.users
                             WHERE user_id = @User_id";
            var parameters = new NpgsqlParameter[]
            {
                new(nameof(User.User_id), NpgsqlDbType.Uuid) { Value = id }
            };
            var data = await npgsqlService.GetQueryResultAsync(query, parameters,
                ["first_name", "second_name", "birthdate", "biography", "city", "password", "can_publish_messages"],
                TargetSessionAttributes.PreferStandby);
            if (data.Count == 0) return null;
            return new User(id, data[0]);
        }

        public async Task<UserRegisterResponse> RegisterUserAsync(UserRegisterRequest request)
        {
            string query = @"INSERT INTO public.users (user_id, first_name, second_name, birthdate, biography, city, password)
                                VALUES (@User_id, @First_name, @Second_name, @Birthdate, @Biography, @City, @Password)";
            var userId = Guid.NewGuid();
            var parameters = new NpgsqlParameter[]
            {
                new(nameof(User.User_id), NpgsqlDbType.Uuid) { Value = userId },
                new(nameof(User.First_name), NpgsqlDbType.Varchar) { Value = request.First_name },
                new(nameof(User.Second_name), NpgsqlDbType.Varchar) { Value = request.Second_name },
                new(nameof(User.Birthdate), NpgsqlDbType.Varchar) { Value = request.Birthdate },
                new(nameof(User.Biography), NpgsqlDbType.Varchar) { Value = request.Biography },
                new(nameof(User.City), NpgsqlDbType.Varchar) { Value = request.City },
                new(nameof(User.Password), NpgsqlDbType.Varchar) { Value = PasswordHelper.Hash(request.Password) }
            };
            await npgsqlService.ExecuteNonQueryAsync(query, parameters);
            return new UserRegisterResponse { User_id = userId };
        }

        public async Task<List<User>?> SearchUserAsync(string first_name, string second_name)
        {
            string query = @"SELECT first_name, second_name, birthdate, biography, city, password, user_id
                             FROM public.users
                             WHERE first_name like @First_name and second_name like @Second_name
                             ORDER BY user_id";

            var parameters = new NpgsqlParameter[]
            {
                new(nameof(User.First_name), NpgsqlDbType.Varchar) { Value = first_name + '%' },
                new(nameof(User.Second_name), NpgsqlDbType.Varchar) { Value = second_name + '%'}
            };
            var data = await npgsqlService.GetQueryResultAsync(query, parameters,
                ["first_name", "second_name", "birthdate", "biography", "city", "password", "user_id"], TargetSessionAttributes.PreferStandby);
            if (data.Count == 0) return null;
            var users = new List<User>();
            foreach (var user in data)
            {
                users.Add(new User(Guid.Parse(user["user_id"].ToString()!), user));
            }
            return users;
        }
    }
}
