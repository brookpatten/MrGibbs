using System;

namespace MrGibbs.Models
{
    /// <summary>
    /// helper methods to work with coordinate points
    /// </summary>
    public static class CoordinatePointUtilities
    {
        /// <summary>
        /// finds "great circle" intersection of 2 lines
        /// http://www.movable-type.co.uk/scripts/latlong.html
        /// http://www.movable-type.co.uk/scripts/js/geodesy/latlon-spherical.js
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="brng1"></param>
        /// <param name="p2"></param>
        /// <param name="brng2"></param>
        /// <returns></returns>
        public static CoordinatePoint FindIntersection(CoordinatePoint p1, double brng1, CoordinatePoint p2,
            double brng2)
        {

            double a = p1.Latitude.Value.ToRadians();
            double c = p1.Longitude.Value.ToRadians();
            double b = p2.Latitude.Value.ToRadians();
            double d = p2.Longitude.Value.ToRadians();
            var e = brng1.ToRadians();
            var f = brng2.ToRadians();
            var g = b - a;
            var h = d - c;

            var i = 2*Math.Asin(Math.Sqrt(Math.Sin(g/2)*Math.Sin(g/2) +
                                          Math.Cos(a)*Math.Cos(b)*Math.Sin(h/2)*Math.Sin(h/2)));
            if (i == 0) 
                return null;

            // initial/final bearings between points
            var j = Math.Acos((Math.Sin(b) - Math.Sin(a)*Math.Cos(i))/
                              (Math.Sin(i)*Math.Cos(a)));
            //if (isNaN(j)) j = 0; // protect against rounding
            var k = Math.Acos((Math.Sin(a) - Math.Sin(b)*Math.Cos(i))/
                              (Math.Sin(i)*Math.Cos(b)));

            double j2;
            double k1;
            if (Math.Sin(d - c) > 0)
            {
                j2 = j;
                k1 = 2*Math.PI - k;
            }
            else
            {
                j2 = 2*Math.PI - j;
                k1 = k;
            }

            var l = (e - j2 + Math.PI)%(2*Math.PI) - Math.PI; // angle 2-1-3
            var m = (k1 - f + Math.PI)%(2*Math.PI) - Math.PI; // angle 1-2-3

            if (Math.Sin(l) == 0 && Math.Sin(m) == 0) 
                return null; // infinite intersections
            if (Math.Sin(l)*Math.Sin(m) < 0) 
                return null; // ambiguous intersection

            //l = Math.abs(l);
            //m = Math.abs(m);
            // ... Ed Williams takes abs of l/m, but seems to break calculation?

            var n = Math.Acos(-Math.Cos(l)*Math.Cos(m) +
                              Math.Sin(l)*Math.Sin(m)*Math.Cos(i));
            var o = Math.Atan2(Math.Sin(i)*Math.Sin(l)*Math.Sin(m),
                Math.Cos(m) + Math.Cos(l)*Math.Cos(n));
            var p = Math.Asin(Math.Sin(a)*Math.Cos(o) +
                              Math.Cos(a)*Math.Sin(o)*Math.Cos(e));
            var q = Math.Atan2(Math.Sin(e)*Math.Sin(o)*Math.Cos(a),
                Math.Cos(o) - Math.Sin(a)*Math.Sin(p));
            var r = c + q;
            r = (r + 3*Math.PI)%(2*Math.PI) - Math.PI; // normalise to -180..+180Â°

            return new CoordinatePoint(new Coordinate(p.ToDegrees()), new Coordinate(r.ToDegrees()), p1.HeightAboveGeoID);

        }

        public static double ToRadians(this double deg)
        {
            return deg*Math.PI/180.0;
        }

        /* Convert radians to degrees */

        public static double ToDegrees(this double rad)
        {
            return rad*180.0/Math.PI;
        }
    }
}