using System;

namespace MrGibbs.Models
{
    /// <summary>
    /// angle helper functions, function names should be self explanatory
    /// </summary>
    public static class AngleUtilities
    {
        public static double DegreestoRadians(double val)
        {
            return (Math.PI / 180) * val;
        }
        public static double RadiansToDegrees(double angle)
        {
            return angle * (180.0 / Math.PI);
        }
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
		public static Vector2 PolarToCartesian(this Vector2Polar p)
		{
			Vector2 result = new Vector2();
			result.X = (float)(p.Radius * Math.Cos(p.Theta));
			result.Y = (float)(p.Radius * Math.Sin(p.Theta));
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

		public static Vector2Polar CartesianToPolar (this Vector2 a)
		{
			var theta = Math.Atan2(a.Y,a.X);
			var radius = Math.Sqrt (a.X * a.X + a.Y * a.Y);
			return new Vector2Polar () { Theta = (float)theta, Radius = (float)radius };
		}

		public static Vector2Polar Add(this Vector2Polar a,Vector2Polar b)
		{
			var X = Math.Cos(a.Theta)*a.Radius + Math.Cos(b.Theta)*b.Radius;
			var Y = Math.Sin (a.Theta) * a.Radius + Math.Sin (b.Theta) * b.Radius;
			var theta = Math.Atan2(Y,X);
			var radius = Math.Sqrt (X * X + Y * Y);
			return new Vector2Polar () { Theta = (float)theta, Radius = (float)radius };
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
