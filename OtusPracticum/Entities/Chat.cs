namespace OtusPracticum.Entities
{
    public class Chat
    {
        public Guid Chat_id { get; set; }
        public Guid Creator_id { get; set; }
        public string Chat_name { get; set; } = null!;
        public DateTime Creation_datetime { get; set; }
        public DateTime LastUpdate_datetime { get; set; }

        public Chat() { }
        public Chat(Dictionary<string, object> data)
        {
            Chat_id = Guid.Parse(data["chat_id"].ToString()!);
            Creator_id = Guid.Parse(data["creator_id"].ToString()!);
            Chat_name = data["chat_name"].ToString()!;
            Creation_datetime = Convert.ToDateTime(data["creation_datetime"]);
            LastUpdate_datetime = Convert.ToDateTime(data["last_update_datetime"]);
        }
    }
}
