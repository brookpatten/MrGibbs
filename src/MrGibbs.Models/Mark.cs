using System.Collections.Generic;

namespace MrGibbs.Models
{
    public enum MarkCaptureMethod : byte { Location = 0, Bearing = 1 }
    public enum MarkType : byte { Windward = 0, Leeward = 1, Line = 2, Course=byte.MaxValue }

    /// <summary>
    /// represents a mark on a racecourse, or rather, as much as we know about a mark
    /// </summary>
    public class Mark
    {
        public CoordinatePoint Location { get; set; }

        public MarkCaptureMethod? CaptureMethod { get; set; }
        public MarkType MarkType { get; set; }

        public IList<Bearing> Bearings { get; set; }

        public string Abbreviation
        {
            get
            {
                return this.MarkType.ToString().Substring(0, 1);
            }
        }
    }
}
