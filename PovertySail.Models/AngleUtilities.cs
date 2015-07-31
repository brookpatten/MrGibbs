using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PovertySail.Models
{
    public class AngleUtilities
    {
        public static double NormalizeAngle(double angle)
        {
            angle = angle % (Math.PI * 2d);
            if (angle < 0)
            {
                angle = Math.PI * 2d + angle;
            }
            return angle;
        }
        public static Vector2 PolarToRectangular(Vector2 origin, double theta, double r)
        {
            Vector2 result = new Vector2();
            result.X = (float)(r * Math.Cos(theta));
            result.Y = (float)(r * Math.Sin(theta));
            result.X += origin.X;
            result.Y += origin.Y;
            return result;
        }
        public static ProjectedPoint PolarToRectangular(ProjectedPoint origin, double theta, double r)
        {
            ProjectedPoint result = new ProjectedPoint();
            result.Easting = r * Math.Cos(theta);
            result.Northing = r * Math.Sin(theta);
            result.Easting += origin.Easting;
            result.Northing += origin.Northing;
            return result;
        }
        public static double FindHalfwayCounterClockwiseAngle(double previous, double next)
        {
            double roundingAngle = 0f;
            if (Math.Abs(AngleUtilities.AngleDifference(previous, next)) > Math.Abs(AngleUtilities.AngleDifference(next, previous)))
            {
                roundingAngle = previous + (AngleUtilities.AngleDifference(previous, next) / 2f);
            }
            else
            {
                roundingAngle = previous + (AngleUtilities.AngleDifference(next, previous) / 2f);
            }
            roundingAngle = AngleUtilities.NormalizeAngle(roundingAngle + Math.PI);
            return roundingAngle;
        }
        public static double FindAngle(Vector3 a, Vector3 b)
        {
            return -(double)Math.Atan2(a.Z - b.Z, a.X - b.X);
        }
        public static double FindAngle(ProjectedPoint a, ProjectedPoint b)
        {
            return -Math.Atan2(a.Easting - b.Easting, a.Northing - b.Northing);
        }
        public static double FindAngleWTF(ProjectedPoint a, ProjectedPoint b)
        {
            return -Math.Atan2(a.Northing - b.Northing, a.Easting - b.Easting);
        }
        public static double AngleDifference(double a, double b)
        {
            a = NormalizeAngle(a);
            b = NormalizeAngle(b);

            double c = a - b;

            if (c < 0)
            {
                if (c < -Math.PI)
                {
                    return (Math.PI * 2d + c);
                }
                else
                {
                    return c;
                }
            }
            else
            {
                if (c > Math.PI)
                {
                    return -(Math.PI * 2d - c);
                }
                else
                {
                    return c;
                }
            }
        }
    }
}
