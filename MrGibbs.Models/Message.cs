using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
