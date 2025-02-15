using Npgsql;
using System.Data;

namespace OtusPracticum.Services
{
    public class NpgsqlService : IAsyncDisposable
    {
        public NpgsqlDataSource Connection { get; }
        public NpgsqlService(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString(nameof(OtusPracticum))
                ?? throw new Exception("Connection string not found");
            Connection = NpgsqlDataSource.Create(connectionString);
            CreateDbSchema();
        }

        public async ValueTask DisposeAsync()
        {
            await Connection.DisposeAsync();
            GC.SuppressFinalize(this);
        }

        public async Task<int> ExecuteNonQueryAsync(string query, NpgsqlParameter[] parameters)
        {
            await using var cmd = Connection.CreateCommand(query);
            cmd.Parameters.AddRange(parameters);
            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<Dictionary<string, object>>> GetQueryResultAsync(string query, NpgsqlParameter[] parameters, string[] columns)
        {
            List<Dictionary<string, object>> result = [];
            await using var cmd = Connection.CreateCommand(query);
            cmd.Parameters.AddRange(parameters);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object>();
                foreach (var column in columns)
                {
                    row.Add(column, reader.GetValue(column));
                }
                result.Add(row);
            }
            return result;
        }

        private void CreateDbSchema()
        {
            var query = @"create table if not exists public.""Users"" (
                        ""User_id"" uuid NOT NULL,
                        ""First_name"" varchar(30) NOT NULL,
                        ""Second_name"" varchar(30) NOT NULL,
                        ""Birthdate"" varchar(11) NOT NULL,
                        ""Biography"" varchar(1000) NOT NULL,
                        ""City"" varchar(30) NOT NULL,
                        ""Password"" varchar(255) NOT NULL,
                        constraint ""PK_Users"" PRIMARY KEY (""User_id"")
                    )";
            ExecuteNonQueryAsync(query, []).Wait();
        }
    }
}
