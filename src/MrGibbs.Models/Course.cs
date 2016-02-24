using System.Collections.Generic;

namespace MrGibbs.Models
{
    /// <summary>
    /// current course that is being sailed, currently just a marker
    /// </summary>
    public interface ICourse
    {
    }

    /// <summary>
    /// course we can sail because we know at least 2 marks
    /// </summary>
    public class CourseByMarks:ICourse
    {
        public IList<Mark> Marks { get; set; }
    }

    /// <summary>
    /// course we can sail but all we know is the heading not the location
    /// </summary>
    public class CourseByAngle:ICourse
    {
        public double CourseAngle { get; set; }
    }
}
