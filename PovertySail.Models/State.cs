using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PovertySail.Models
{
    public class State
    {
        //provided by system clock or gps
        public DateTime SystemTime { get; set; }
        
        //gps provided data
        public DateTime? GpsTime { get; set; }
        public CoordinatePoint Location { get; set; }
        public double? CourseOverGroundByLocation { get; set; }
        public double? SpeedInKnots { get; set; }
        public double? MagneticCourseMadeGood { get; set; }
        public double? TrueCourseMadeGood { get; set; }
        public double? MagneticDeviation { get; set; }
        public double? MagneticDeviationDirection { get; set; }

        //accel provided data
        public Vector3 Accel { get; set; }
        public Vector3 Gyro { get; set; }
        public double? Heel { get; set; }

        //magneto provided data
        public Vector3 Magneto { get; set; }
        
        //calculated values
        public double? MagneticHeading { get; set; }

        //race state
        public DateTime? StartTime { get; set; }
        public IList<Mark> Marks { get; set; }
        private int? _targetMarkIndex;

        //system state
        private List<Message> _messages;

        public State()
        {
            _messages = new List<Message>();
            Marks = new List<Mark>();
        }

        public Message Message
        {
            get;
            private set;
        }

        public TimeSpan? Countdown
        {
            get
            {
                if (StartTime.HasValue)
                {
                    if (StartTime.Value > BestTime)
                    {
                        return StartTime.Value - BestTime;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
        }

        public Mark TargetMark
        {
            get
            {
                if (_targetMarkIndex.HasValue)
                {
                    return Marks[_targetMarkIndex.Value];
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (Marks.Contains(value))
                {
                    _targetMarkIndex = Marks.IndexOf(value);
                }
                else
                {
                    throw new InvalidDataException("Unknown mark");
                }
            }
        }

        public DateTime BestTime
        {
            get
            {
                if (GpsTime.HasValue)
                {
                    return GpsTime.Value;
                }
                else
                {
                    return SystemTime;
                }
            }
        }
        
        public void AddMessage(Message message)
        {
            lock (_messages)
            {
                message.ShownAt = null;
                _messages.Add(message);
            }
            //if we're not showing anything right now, we can go ahead and show it
            if(Message==null)
            {
                CycleMessages();
            }
        }

        public void AddMessage(MessageCategory category, MessagePriority priority, int secondsDuration, string text)
        {
            var message = new Message();
            message.CreatedAt = BestTime;
            message.Text = text;
            message.Priority = priority;
            message.Duration = new TimeSpan(0,0,0,secondsDuration);
            AddMessage(message);
        }

        public void CycleMessages()
        {
            lock (_messages)
            {
                if ((Message == null || Message.HideAt < BestTime) && _messages.Any())
                {
                    var highest = _messages.OrderBy(x => (int)x.Priority).First();
                    Message = highest;
                    Message.ShownAt = BestTime;
                    _messages.Remove(highest);
                }
                else
                {
                    Message = null;
                }
            }
        }
    }
}
