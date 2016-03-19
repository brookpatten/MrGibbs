using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using prog_uchar = System.Byte;
using uint8_t = System.Byte;
using int8_t = System.SByte;
using uint16_t = System.UInt16;
using int16_t = System.Int16;

namespace MrGibbs.MPU6050
{
    public static class MathUtil
    {

        public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            else if (val.CompareTo(max) > 0) return max;
            else return val;
        }
    }

    public struct Quaternion {
        public float w;
        public float x;
        public float y;
        public float z;
        
        public Quaternion(float nw, float nx, float ny, float nz)
        {
            w = nw;
            x = nx;
            y = ny;
            z = nz;
        }

        public Quaternion getProduct(Quaternion q)
        {
            // Quaternion multiplication is defined by:
            // (Q1 * Q2).w = (w1w2 - x1x2 - y1y2 - z1z2)
            // (Q1 * Q2).x = (w1x2 + x1w2 + y1z2 - z1y2)
            // (Q1 * Q2).y = (w1y2 - x1z2 + y1w2 + z1x2)
            // (Q1 * Q2).z = (w1z2 + x1y2 - y1x2 + z1w2
            return new Quaternion(
                w*q.w - x*q.x - y*q.y - z*q.z, // new w
                w*q.x + x*q.w + y*q.z - z*q.y, // new x
                w*q.y - x*q.z + y*q.w + z*q.x, // new y
                w*q.z + x*q.y - y*q.x + z*q.w); // new z
        }

        public Quaternion getConjugate()
        {
            return new Quaternion(w, -x, -y, -z);
        }

        public float getMagnitude()
        {
            return (float)Math.Sqrt(w*w + x*x + y*y + z*z);
        }

        public void normalize()
        {
            float m = getMagnitude();
            w /= m;
            x /= m;
            y /= m;
            z /= m;
        }

        public Quaternion getNormalized()
        {
            Quaternion r = new Quaternion(w, x, y, z);
            r.normalize();
            return r;
        }
};

public struct VectorInt16 {
        public int16_t x;
        public int16_t y;
        public int16_t z;

        public VectorInt16(int16_t nx, int16_t ny, int16_t nz)
        {
            x = nx;
            y = ny;
            z = nz;
        }

        public float getMagnitude()
        {
            return (float)Math.Sqrt(x*x + y*y + z*z);
        }

        public void normalize()
        {
            float m = getMagnitude();
            x /= (short)m;
            y /= (short)m;
            z /= (short)m;
        }
        
        public VectorInt16 getNormalized() {
            VectorInt16 r = new VectorInt16(x, y, z);
            r.normalize();
            return r;
        }
        
        public void rotate(Quaternion q) {
            // http://www.cprogramming.com/tutorial/3d/quaternions.html
            // http://www.euclideanspace.com/maths/algebra/realNormedAlgebra/quaternions/transforms/index.htm
            // http://content.gpwiki.org/index.php/OpenGL:Tutorials:Using_Quaternions_to_represent_rotation
            // ^ or: http://webcache.googleusercontent.com/search?q=cache:xgJAp3bDNhQJ:content.gpwiki.org/index.php/OpenGL:Tutorials:Using_Quaternions_to_represent_rotation&hl=en&gl=us&strip=1
        
            // P_out = q * P_in * conj(q)
            // - P_out is the output vector
            // - q is the orientation quaternion
            // - P_in is the input vector (a*aReal)
            // - conj(q) is the conjugate of the orientation quaternion (q=[w,x,y,z], q*=[w,-x,-y,-z])
            Quaternion p = new Quaternion(0, x, y, z);

            // quaternion multiplication: q * p, stored back in p
            p = q.getProduct(p);

            // quaternion multiplication: p * conj(q), stored back in p
            p = p.getProduct(q.getConjugate());

            // p quaternion is now [0, x', y', z']
            x = (int16_t)p.x;
            y = (int16_t)p.y;
            z = (int16_t)p.z;
        }

        VectorInt16 getRotated(Quaternion q) {
            VectorInt16 r = new VectorInt16(x, y, z);
            r.rotate(q);
            return r;
        }
};

public struct VectorFloat {
        public float x;
        public float y;
        public float z;


    public static VectorFloat operator *(VectorFloat v, float f)
    {
        return new VectorFloat(v.x * f, v.y * f, v.z * f);
    }
    public static VectorFloat operator /(VectorFloat v, float f)
    {
        return new VectorFloat(v.x / f, v.y / f, v.z / f);
    }
    public static VectorFloat operator *(VectorFloat v1, VectorFloat v2)
    {
        return new VectorFloat(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z);
    }
    public static VectorFloat operator +(VectorFloat v1, VectorFloat v2)
    {
        return new VectorFloat(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z);
    }
    public static VectorFloat operator -(VectorFloat v1, VectorFloat v2)
    {
        return new VectorFloat(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);
    }
        public VectorFloat(float nx, float ny, float nz)
        {
            x = nx;
            y = ny;
            z = nz;
        }

        public float getMagnitude()
        {
            return (float)Math.Sqrt(x*x + y*y + z*z);
        }

        public void normalize()
        {
            float m = getMagnitude();
            x /= m;
            y /= m;
            z /= m;
        }
        
        public VectorFloat getNormalized() {
            VectorFloat r = new VectorFloat(x, y, z);
            r.normalize();
            return r;
        }
        
        public void rotate(Quaternion q) {
            Quaternion p = new Quaternion(0, x, y, z);

            // quaternion multiplication: q * p, stored back in p
            p = q.getProduct(p);

            // quaternion multiplication: p * conj(q), stored back in p
            p = p.getProduct(q.getConjugate());

            // p quaternion is now [0, x', y', z']
            x = p.x;
            y = p.y;
            z = p.z;
        }

        public VectorFloat getRotated(Quaternion q)
        {
            VectorFloat r = new VectorFloat(x, y, z);
            r.rotate(q);
            return r;
        }
};
}
