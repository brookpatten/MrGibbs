using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MrGibbs.Models
{
    /// <summary>
    /// the current state of all sensor data and calculated values
    /// TODO: refactor this to allow plugins to add/remove things as needed, it's kind of a 
    /// god class right now
    /// </summary>
    public class State
    {
        enum Tack : byte { Port = 0, Starboard = 1 }
        enum Leg : byte { Windward = 0, Leeward = 1 }

        //provided by system clock or gps
        public DateTime SystemTime { get; set; }
        
        //gps provided data
        public DateTime? GpsTime { get; set; }
        public CoordinatePoint Location { get; set; }
		public IDictionary<StateValue, double> StateValues { get; set; }
        
        public static void Run()
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            GpsTime = null;
            Location = null;
			StateValues.Clear ();
        }
        
        //race state
        public DateTime? StartTime { get; set; }
        public ICourse Course { get; set; }
        private int? _targetMarkIndex;
        private int? _previousMarkIndex;
        public bool RaceStarted { get; set; }

        //system state
        private List<Message> _messages;

        public State()
        {
            _messages = new List<Message>();
            Course = null;
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
                if (_targetMarkIndex.HasValue 
                    && Course!=null 
                    && Course is CourseByMarks 
                    && _targetMarkIndex.Value < (Course as CourseByMarks).Marks.Count)
                {
                    return (Course as CourseByMarks).Marks[_targetMarkIndex.Value];
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (value == null)
                {
                    _targetMarkIndex = null;
                }
                else if (Course !=null && Course is CourseByMarks && (Course as CourseByMarks).Marks.Contains(value))
                {
                    _targetMarkIndex = (Course as CourseByMarks).Marks.IndexOf(value);
                }
                else
                {
                    throw new InvalidDataException("Unknown mark");
                }
            }
        }

        public Mark PreviousMark
        {
            get
            {
                if (_previousMarkIndex.HasValue
                    && Course != null
                    && Course is CourseByMarks
                    && _previousMarkIndex.Value < (Course as CourseByMarks).Marks.Count)
                {
                    return (Course as CourseByMarks).Marks[_previousMarkIndex.Value];
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (value == null)
                {
                    _previousMarkIndex = null;
                }
                else if (Course != null && Course is CourseByMarks && (Course as CourseByMarks).Marks.Contains(value))
                {
                    _previousMarkIndex = (Course as CourseByMarks).Marks.IndexOf(value);
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

        public int MessageCount
        {
            get
            {
                if(_messages!= null)
                {
                    return _messages.Count;
                }
                else
                {
                    return 0;
                }
            }
        }

        public void CycleMessages()
        {
            lock (_messages)
            {
                if (Message == null || Message.HideAt < BestTime)
                {
                    if (_messages.Any())
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
                else
                {
                    //Message = null;
                }
            }
        }
    }
}
