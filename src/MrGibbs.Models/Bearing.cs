using System;

namespace MrGibbs.Models
{
    /// <summary>
    /// dto for bearing details
    /// </summary>
    public class Bearing
    {
        public DateTime RecordedAt { get; set; }
        public CoordinatePoint Location { get; set; }
        public double CompassHeading { get; set; }
    }
}
