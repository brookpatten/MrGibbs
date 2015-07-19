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
        private int? _targetMarkIndex;
        public DateTime Time { get; set; }
        public CoordinatePoint Location { get; set; }
        
        public double CourseOverGround { get; set; }
        public double SpeedInKnots { get; set; }
        
        public DateTime? StartTime { get; set; }

        public string Message
        {
            get;
            private set;
        }

        public TimeSpan? Countdown
        {
            get
            {
                if(StartTime.HasValue)
                {
                    if(StartTime.Value>Time)
                    {
                        return StartTime.Value - Time;
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

        private List<Tuple<int,string>> _messages;

        public State()
        {
            _messages = new List<Tuple<int, string>>();
            Marks = new List<Mark>();
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
        public IList<Mark> Marks { get; set; } 

        public void AddMessage(int priority,string message)
        {
            lock (_messages)
            {
                _messages.Add(new Tuple<int, string>(priority, message));
            }
            //if we're not showing anything right now, we can go ahead and show it
            if(string.IsNullOrWhiteSpace(Message))
            {
                CycleMessages();
            }
        }

        public void CycleMessages()
        {
            lock (_messages)
            {
                if (_messages.Any())
                {
                    var highest = _messages.OrderBy(x => x.Item1).First();
                    Message = highest.Item2;
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
