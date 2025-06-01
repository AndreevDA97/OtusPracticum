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
            Creation_datetime = Convert.ToDateTime(data["creation_datetime"]);
        }

        public Post(Dictionary<string, object> data)
        {
            Post_id = Guid.Parse(data["post_id"].ToString()!);
            User_id = Guid.Parse(data["user_id"].ToString()!);
            Text = data["post"].ToString()!;
            Creation_datetime = Convert.ToDateTime(data["creation_datetime"]);
        }

        public Guid Post_id { get; set; }
        public Guid User_id { get; set; }
        public string Text { get; set; } = null!;
        public DateTime Creation_datetime { get; set; }
    }
}
