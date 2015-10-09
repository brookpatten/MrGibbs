using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MrGibbs.Models
{
    public interface ICourse
    {
    }

    public class CourseByMarks:ICourse
    {
        public IList<Mark> Marks { get; set; }
    }

    public class CourseByAngle:ICourse
    {
        public double CourseAngle { get; set; }
    }
}
