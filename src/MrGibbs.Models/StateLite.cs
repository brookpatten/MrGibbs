using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MrGibbs.Models
{
    public class StateLite
    {
        //public ICourse Course { get; set; }
        //private int? _targetMarkIndex;
        //private int? _previousMarkIndex;
        public TimeSpan? Countdown { get; set; }
        public bool RaceStarted { get; set; }
        public IDictionary<StateValue, double> StateValues { get; set; }
        //public IList<Tack> Tacks { get; set; }
    }
}
