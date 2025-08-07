using OtusPracticum.Entities;
using OtusPracticum.Models;

namespace OtusPracticum.Services
{
    public interface IChatService
    {
        Task<MessageEntity[]> GetChatAsync(Guid chat_id, int limit, int offset, Guid user_id);
        Task<List<Chat>> GetUserChatListAsync(Guid user_id, int limit, int offset);
        Task<Guid> SendMessageToChatAsync(Guid chat_id, SendMessageRequest request, Guid creator_id);
        Task<Guid> CreateChatAsync(CreateChatRequest request, Guid creator_id);
    }
}
