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

        private Dictionary<string,double> Metrics { get; set; } 

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
