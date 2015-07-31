using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PovertySail.Models
{
    public class Vector2
    {
        public float X { get; set; }
        public float Y { get; set; }
    }
    public class Vector3 : Vector2
    {
        public float Z { get; set; }
    }
}
