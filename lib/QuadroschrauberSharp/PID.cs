using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuadroschrauberSharp
{
    public class VectorPID
    {

        public VectorPID()
        {
        }

        public VectorFloat Update(VectorFloat error, float dtime)
        {
            error_integral += error * dtime;
            error_derivate = (error - previous_error) / dtime;
            previous_error = error;

            error_integral.x = MathUtil.Clamp(error_integral.x, -I_max.x, I_max.x);
            error_integral.y = MathUtil.Clamp(error_integral.y, -I_max.y, I_max.y);
            error_integral.z = MathUtil.Clamp(error_integral.z, -I_max.z, I_max.z);

            return error * P + error_integral * I + error_derivate * D;
        }
    
        public VectorFloat error_derivate;
        public VectorFloat error_integral;
    
        public VectorFloat P;
        public VectorFloat I;
        public VectorFloat D;
    
        public VectorFloat I_max = new VectorFloat(0,0,0);
        protected VectorFloat previous_error;
    };
}
