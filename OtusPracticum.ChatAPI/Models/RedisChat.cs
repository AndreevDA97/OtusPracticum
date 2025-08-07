namespace OtusPracticum.Models
{
    public class RedisChat
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public List<Guid> Users { get; set; } = [];
        public List<RedisChatMessage> Messages { get; set; } = [];
        public DateTime Created_at { get; set; }
    }
}
