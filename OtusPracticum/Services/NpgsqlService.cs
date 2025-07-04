﻿using Npgsql;
using System.Data;

namespace OtusPracticum.Services
{
    public enum NpgsqlDatabase
    {
        OtusPracticum,
        ChatService
    }
    public class NpgsqlService : IAsyncDisposable, IDisposable
    {
        public NpgsqlMultiHostDataSource Npgsql { get; }
        public NpgsqlService(IConfiguration configuration,
            NpgsqlDatabase database = NpgsqlDatabase.OtusPracticum)
        {
            var connectionString = configuration.GetConnectionString(database.ToString())
                ?? throw new Exception("Connection string not found");
            Npgsql = new NpgsqlDataSourceBuilder(connectionString).BuildMultiHost();
            switch (database)
            {
                case NpgsqlDatabase.OtusPracticum:
                    CreateDbSchema();
                    break;
                case NpgsqlDatabase.ChatService:
                    CreateChatDbSchema();
                    break;
            }
        }

        public void Dispose()
        {
            Npgsql.Dispose();
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            await Npgsql.DisposeAsync();
            GC.SuppressFinalize(this);
        }

        public async Task<int> ExecuteNonQueryAsync(string query, NpgsqlParameter[] parameters)
        {
            await using var connection = await Npgsql.OpenConnectionAsync(TargetSessionAttributes.Primary);
            await using var cmd = new NpgsqlCommand(query, connection);
            if (parameters.Length > 0)
                cmd.Parameters.AddRange(parameters);
            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<Dictionary<string, object>>> GetQueryResultAsync(string query,
            NpgsqlParameter[] parameters, string[] columns, TargetSessionAttributes targetSessionAttributes = TargetSessionAttributes.Any)
        {
            List<Dictionary<string, object>> result = [];
            await using var connection = await Npgsql.OpenConnectionAsync(targetSessionAttributes);
            await using var cmd = new NpgsqlCommand(query, connection);
            if (parameters.Length > 0)
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
            var query = @"CREATE TABLE IF NOT EXISTS public.users
                        (
                            user_id uuid NOT NULL,
                            first_name character varying(30) NOT NULL,
                            second_name character varying(30) NOT NULL,
                            birthdate character varying(11) NOT NULL,
                            biography character varying(1000) NOT NULL,
                            city character varying(255) NOT NULL,
                            password character varying(255) NOT NULL,
                            CONSTRAINT pk_users PRIMARY KEY (user_id)
                        );
                        CREATE INDEX IF NOT EXISTS users_fname_sname_idx ON public.users(first_name varchar_pattern_ops, second_name varchar_pattern_ops);
                        CREATE TABLE IF NOT EXISTS public.friends 
                        (
                            user_id uuid,
                            friend_id uuid,
                            PRIMARY KEY(user_id, friend_id),
                            FOREIGN KEY (user_id) REFERENCES users (user_id),
                            FOREIGN KEY (friend_id) REFERENCES users (user_id)
                        );
                        CREATE TABLE IF NOT EXISTS public.posts 
                        (
                            post_id uuid not null,
                            user_id uuid not null,
                            post varchar(2000) not null,
                            creation_datetime timestamp not null default CURRENT_TIMESTAMP,
                            PRIMARY KEY(post_id),
                            FOREIGN KEY (user_id) REFERENCES users (user_id)
                        );
                        CREATE INDEX IF NOT EXISTS posts_userid_idx ON public.posts(user_id);";
            ExecuteNonQueryAsync(query, []).Wait();
        }

        private void CreateChatDbSchema()
        {
            var query = @"CREATE TABLE IF NOT EXISTS public.chats
                        (
                            chat_id uuid NOT NULL,
                            chat_name character varying(50) NOT NULL,
                            creator_id uuid NOT NULL,
                            creation_datetime timestamp without time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
                            last_update_datetime timestamp without time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
                            CONSTRAINT chats_pkey PRIMARY KEY (chat_id)
                        );
                        CREATE TABLE IF NOT EXISTS public.chat_users
                        (
                            chat_id uuid NOT NULL,
                            user_id uuid NOT NULL,
                            creation_datetime timestamp without time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
                            CONSTRAINT chat_users_pkey PRIMARY KEY (chat_id, user_id),
                            CONSTRAINT chat_users_chat_id_fkey FOREIGN KEY (chat_id) REFERENCES public.chats (chat_id)
                        );
                        CREATE TABLE IF NOT EXISTS public.messages
                        (
                            message_id uuid NOT NULL,
                            chat_id uuid NOT NULL,
                            user_id uuid NOT NULL,
                            message character varying(2000) NOT NULL,
                            creation_datetime timestamp without time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
                            CONSTRAINT messages_pkey PRIMARY KEY (message_id, chat_id)
                        );";
            ExecuteNonQueryAsync(query, []).Wait();
        }
    }
}
