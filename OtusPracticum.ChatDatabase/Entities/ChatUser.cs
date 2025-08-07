namespace OtusPracticum.Entities
{
    public class ChatUser
    {
        public Guid Chat_id { get; set; }
        public Guid User_id { get; set; }
        public DateTime Creation_datetime { get; set; }
    }
}
