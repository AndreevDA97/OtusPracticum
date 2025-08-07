using OtusPracticum.Entities;

namespace OtusPracticum.Models
{
    public class RedisUserChats
    {
        public Guid User_id { get; set; }
        public List<Chat> Chats { get; set; } = [];
    }
}
