using System;

namespace MrGibbs.Models
{
    public enum MessageCategory
    {
        System,
        Tactical,
    }
    public enum MessagePriority
    {
        Low=0,
        Normal=1,
        High=2
    }

    /// <summary>
    /// Message which needs to be communicated to the user via some means
    /// </summary>
    public class Message
    {
        public MessageCategory Category { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ShownAt { get; set; }
        public TimeSpan Duration { get; set;}

        public DateTime? HideAt
        {
            get
            {
                if (ShownAt.HasValue)
                {
                    return ShownAt + Duration;
                }
                else
                {
                    return null;
                }
            }
        }
        public MessagePriority Priority { get; set; }
        public string Text { get; set; }
    }
}
