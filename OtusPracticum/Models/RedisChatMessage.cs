namespace OtusPracticum.Models
{
    public class RedisChatMessage
    {
        public string Text { get; set; } = null!;
        public Guid User_id { get; set; }
        public DateTime Created_at { get; set; }
        public Guid Message_id { get; set; }
    }
}
