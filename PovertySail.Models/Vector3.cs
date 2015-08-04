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

        

        public static Vector3 operator *(Vector3 v, float f)
        {
            return new Vector3(v.X * f, v.Y * f, v.Z * f);
        }
        public static Vector3 operator /(Vector3 v, float f)
        {
            return new Vector3(v.X / f, v.Y / f, v.Z / f);
        }
        public static Vector3 operator *(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.X * v2.X, v1.Y * v2.Y, v1.Z * v2.Z);
        }
        public static Vector3 operator +(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.X + v2.X, v1.Y + v2.Y, v1.X + v2.X);
        }
        public static Vector3 operator -(Vector3 v1, Vector3 v2)
        {
            return new Vector3(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
        }
        public Vector3(float nx, float ny, float nz)
        {
            X = nx;
            Y = ny;
            Z = nz;
        }

        public float GetMagnitude()
        {
            return (float)Math.Sqrt(X*X + Y*Y + Z*Z);
        }

        public void Normalize()
        {
            float m = GetMagnitude();
            X /= m;
            Y /= m;
            Z /= m;
        }
        
        public Vector3 GetNormalized() {
            Vector3 r = new Vector3(X, Y, Z);
            r.Normalize();
            return r;
        }
    }
}
