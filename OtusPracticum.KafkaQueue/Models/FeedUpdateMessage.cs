using static System.Net.Mime.MediaTypeNames;

namespace OtusPracticum.KafkaQueue.Models
{
    public class FeedUpdateMessage
    {
        public FeedUpdateMessage() { }

        public FeedUpdateMessage(ActionTypeEnum actionType, Guid? post_id, Guid author_id, string? text)
        {
            ActionType = actionType;
            Post_id = post_id;
            Author_id = author_id;
            Text = text;
        }

        public ActionTypeEnum ActionType { get; set; }
        public Guid? Post_id { get; set; }
        public Guid Author_id { get; set; }
        public string? Text { get; set; }

    }
    public enum ActionTypeEnum
    {
        Create = 0,
        Update = 1,
        Delete = 2,
        FullReload = 3
    }
}
