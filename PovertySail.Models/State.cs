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
        public double Speed { get; set; }
        public DateTime? StartTime { get; set; }

        public State()
        {
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
    }
}
