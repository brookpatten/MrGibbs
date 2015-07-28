using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PovertySail.Models
{
    public enum MarkCaptureMethod : byte { Location = 0, Bearing = 1 }
    public enum MarkType : byte { Windward = 0, Leeward = 1, Line = 2 }

    public class Mark
    {
        

        public CoordinatePoint Location { get; set; }

        public MarkCaptureMethod? CaptureMethod { get; set; }
        public MarkType? MarkType { get; set; }

        public IList<Bearing> Bearings { get; set; }
    }
}
