using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuadroschrauberSharp.Hardware
{
    public abstract class Motor
    {
        public abstract void SetMilli(int permille);
        public abstract void Set(float power);
    }
}
