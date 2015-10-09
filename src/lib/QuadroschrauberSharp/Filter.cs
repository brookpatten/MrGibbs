using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuadroschrauberSharp
{
    public static class FilterUtil
    {
        public static float Alpha(float HZ, float DT)
        {
            return (DT)/((DT)+(1.0f/(2.0f*3.141f*(float)(HZ))));
        }
    }

    public class VectorLowPass
    {
        public VectorLowPass(float hz)
        {
            this.hz = hz;
        }

        public VectorLowPass()
        {
            hz = 100000000;
        }

        public VectorFloat Filter(VectorFloat v, float dt)
        {
            return Filter(v, dt, hz);
        }

        public VectorFloat Filter(VectorFloat v, float dt, float hz)
        {
            float alpha = FilterUtil.Alpha(hz,dt);
            VectorFloat last = v * alpha + current * (1.0f - alpha);
            current.x = last.x;
            current.y = last.y;
            current.z = last.z;
            return last;
        }

        public VectorFloat Filter(VectorFloat v, float dt, float hz_x, float hz_y, float hz_z)
        {
            VectorFloat alpha = new VectorFloat(FilterUtil.Alpha(hz_x,dt), FilterUtil.Alpha(hz_y,dt), FilterUtil.Alpha(hz_z,dt));
            VectorFloat last = v * alpha + current * (new VectorFloat(){x=1,y=1,z=1} - alpha);
            current.x = last.x;
            current.y = last.y;
            current.z = last.z;
            return last;
        }
    
        public VectorFloat Current
        {
            get
            {
                return current;
            }
        }


        protected VectorFloat current;
        protected float hz;
    };

    public class VectorHighPass
    {
        public VectorHighPass(float hz)
        {
            this.hz = hz;
        }

        public VectorHighPass()
        {
            hz = 0.001f;
        }

        public VectorFloat Filter(VectorFloat v, float dt)
        {
            return Filter(v, dt, hz);
        }

        public VectorFloat Filter(VectorFloat v, float dt, float hz)
        {
            return Filter(v, dt, hz, hz, hz);
        }

        public VectorFloat Filter(VectorFloat v, float dt, float hz_x, float hz_y, float hz_z)
        {
            if(first)
            {
                first = false;
                last = v;
            }
    
            VectorFloat d = (v - last) / dt;
            last = v;
    
            VectorFloat alpha = new VectorFloat(FilterUtil.Alpha(hz_x,dt), FilterUtil.Alpha(hz_y,dt), FilterUtil.Alpha(hz_z,dt));
            current = alpha * (current + d);
    
            return current;
        }

        protected VectorFloat last;
        protected VectorFloat current;
        protected bool first = true;
        protected float hz;
    };

    public class VectorIntegrator
    {
        public VectorFloat Filter(VectorFloat v, float dt)
        {
            integral += v * dt;
            return integral;
        }

        protected VectorFloat integral;
    };

    public class VectorDerivator
    {
        public VectorDerivator()
        {
        }

        public VectorFloat Filter(VectorFloat v, float dt)
        {
            if (first)
            {
                first = false;
                last = v;
            }

            VectorFloat ret = (v - last) / dt;

            last = v;

            return ret;
        }

        protected VectorFloat last;
        protected bool first = true;
    };

}
