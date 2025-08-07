namespace OtusPracticum.Entities
{
    public class MessageEntity
    {
        public MessageEntity()
        {
        }

        public MessageEntity(Guid chat_id, Dictionary<string, object> data)
        {
            Chat_id = chat_id;
            Message_id = Guid.Parse(data["message_id"].ToString()!);
            User_id = Guid.Parse(data["user_id"].ToString()!);
            Message = data["message"].ToString()!;
            Creation_datetime = Convert.ToDateTime(data["creation_datetime"]);
        }

        public Guid Message_id { get; set; }
        public Guid Chat_id { get; set; }
        public Guid User_id { get; set; }
        public string Message { get; set; } = null!;
        public DateTime Creation_datetime { get; set; }
    }
}
