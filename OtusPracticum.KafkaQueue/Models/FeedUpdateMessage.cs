namespace OtusPracticum.KafkaQueue.Models
{
    public class FeedUpdateMessage
    {
        public FeedUpdateMessage() { }

        public FeedUpdateMessage(ActionTypeEnum actionType, Guid? post_id)
        {
            ActionType = actionType;
            Post_id = post_id;
        }

        public ActionTypeEnum ActionType { get; set; }
        public Guid? Post_id { get; set; }

    }
    public enum ActionTypeEnum
    {
        Create = 0,
        Update = 1,
        Delete = 2,
        FullReload = 3
    }
}
