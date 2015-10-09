using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MrGibbs.Models
{
    public class Bearing
    {
        public DateTime RecordedAt { get; set; }
        public CoordinatePoint Location { get; set; }
        public double CompassHeading { get; set; }
    }
}
