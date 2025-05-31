namespace OtusPracticum.Entities
{
    public class Post
    {
        public Post() { }
        public Post(Guid id, Dictionary<string, object> data)
        {
            Post_id = id;
            User_id = Guid.Parse(data["user_id"].ToString()!);
            Text = data["post"].ToString()!;
            Creation_datetime = DateTime.Parse(data["creation_datetime"].ToString()!);
        }

        public Post(Dictionary<string, object> data)
        {
            Post_id = Guid.Parse(data["post_id"].ToString()!);
            User_id = Guid.Parse(data["user_id"].ToString()!);
            Text = data["post"].ToString()!;
            Creation_datetime = DateTime.Parse(data["creation_datetime"].ToString()!);
        }

        public Guid Post_id { get; set; }
        public Guid User_id { get; set; }
        public string Text { get; set; } = null!;
        public DateTime Creation_datetime { get; set; }
    }
}
