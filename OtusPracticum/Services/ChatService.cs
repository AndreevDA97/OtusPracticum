using Npgsql;
using NpgsqlTypes;
using OtusPracticum.Entities;
using OtusPracticum.Models;

namespace OtusPracticum.Services
{
    public class ChatService(
        [FromKeyedServices(nameof(NpgsqlDatabase.ChatService))] NpgsqlService npgsqlService) : IChatService
    {
        private readonly NpgsqlService npgsqlService = npgsqlService;

        public async Task<MessageEntity[]> GetChatAsync(Guid chat_id, int limit, int offset, Guid user_id)
        {
            string query = @"SELECT m.message_id, m.user_id, m.message, m.creation_datetime
                             FROM public.messages m
                             INNER JOIN public.chat_users cu 
                                ON m.chat_id = cu.chat_id
                             WHERE m.chat_id = @Chat_id and cu.user_id = @User_id
                             order by m.creation_datetime desc
                             offset @Offset limit @Limit";
            var parameters = new NpgsqlParameter[]
            {
                new("Chat_id", NpgsqlDbType.Uuid) { Value = chat_id },
                new("User_id", NpgsqlDbType.Uuid) { Value = user_id },
                new("Limit", NpgsqlDbType.Integer) { Value = limit },
                new("Offset", NpgsqlDbType.Integer) { Value = offset }
            };
            var data = await npgsqlService.GetQueryResultAsync(query, parameters, ["user_id", "message", "creation_datetime", "message_id"]);
            return data.Select(d => new MessageEntity(chat_id, d)).ToArray();
        }

        public async Task<List<Chat>> GetUserChatListAsync(Guid user_id, int limit, int offset)
        {
            string query = @"SELECT c.chat_id, c.creator_id, c.creation_datetime, c.chat_name, c.last_update_datetime  
                             FROM public.chat_users cu
                             INNER JOIN public.chats c on cu.chat_id = c.chat_id
                             WHERE cu.user_id = @User_id
                             order by c.last_update_datetime desc
                             limit @Limit offset @Offset";
            var parameters = new NpgsqlParameter[]
            {
                new("User_id", NpgsqlDbType.Uuid) { Value = user_id },
                new("Limit", NpgsqlDbType.Integer) { Value = limit },
                new("Offset", NpgsqlDbType.Integer) { Value = offset }
            };
            var data = await npgsqlService.GetQueryResultAsync(query, parameters, ["chat_id", "creator_id", "creation_datetime", "chat_name", "last_update_datetime"]);
            return data.Select(d => new Chat(d)).ToList();
        }

        public async Task<Guid> SendMessageToChatAsync(Guid chat_id, SendMessageRequest request, Guid creator_id)
        {
            var msg_id = Guid.NewGuid();
            string query = @"INSERT INTO public.messages(message_id, chat_id, user_id, message)
	                            VALUES (@Message_id, @Chat_id, @User_id, @Message)";
            var parameters = new NpgsqlParameter[]
                {
                   new("Chat_id", NpgsqlDbType.Uuid) { Value = chat_id },
                   new("User_id", NpgsqlDbType.Uuid) { Value = creator_id },
                   new("Message_id", NpgsqlDbType.Uuid) { Value = msg_id },
                   new("Message", NpgsqlDbType.Varchar) { Value = request.Message },
                };
            await npgsqlService.ExecuteNonQueryAsync(query, parameters);
            //query = @"UPDATE public.chats
            //          SET last_update_datetime = CURRENT_TIMESTAMP
            //          WHERE chat_id = @Chat_id";
            //parameters =
            //    [
            //       new("Chat_id", NpgsqlDbType.Uuid) { Value = chat_id }
            //    ];
            //await npgsqlService.ExecuteNonQueryAsync(query, parameters);
            return msg_id;
        }

        public async Task<Guid> CreateChatAsync(CreateChatRequest request, Guid creator_id)
        {
            var chat_id = Guid.NewGuid();
            string query = @"INSERT INTO public.chats(chat_id, chat_name, creator_id)
                               VALUES (@Chat_id, @Chat_name, @Creator_id)";
            var parameters = new NpgsqlParameter[]
                {
                   new("Chat_id", NpgsqlDbType.Uuid) { Value = chat_id },
                   new("Creator_id", NpgsqlDbType.Uuid) { Value = creator_id },
                   new("Chat_name", NpgsqlDbType.Varchar) { Value = request.Name }
                };
            await npgsqlService.ExecuteNonQueryAsync(query, parameters);

            query = @"INSERT INTO public.chat_users(chat_id, user_id)
                        VALUES (@Chat_id, @User_id)";
            if (!request.Users_ids.Contains(creator_id))
                request.Users_ids.Add(creator_id);
            foreach (var user_id in request.Users_ids)
            {
                parameters =
                [
                   new("Chat_id", NpgsqlDbType.Uuid) { Value = chat_id },
                   new("User_id", NpgsqlDbType.Uuid) { Value = user_id }
                ];
                await npgsqlService.ExecuteNonQueryAsync(query, parameters);
            }
            return chat_id;
        }
    }
}
