using System;

namespace MrGibbs.Models
{
    /// <summary>
    /// gps/coordinate point class & helper methods
    /// mostly stolen from visualsail, so it's a bit crusty
    /// </summary>
    public class CoordinatePoint
    {
        private Coordinate _latitude;
        private Coordinate _longitude;
        private double _heightAboveGeoID;
        public static double LongitudeZoneSize = 6.0;
        public static double LongitudeOffset = 0.0;
        //static CoordinatePoint()
        //{
        //    LongitudeZoneSize = 6.0;
        //}
        public CoordinatePoint(Coordinate latitude, Coordinate longitude, double height)
        {
            _latitude = latitude;
            _longitude = longitude;
            _heightAboveGeoID = height;
        }
        public Coordinate Latitude
        {
            get
            {
                return _latitude;
            }
            set
            {
                _latitude = value;
            }
        }
        public Coordinate Longitude
        {
            get
            {
                return _longitude;
            }
            set
            {
                _longitude = value;
            }
        }
        public double HeightAboveGeoID
        {
            get
            {
                return _heightAboveGeoID;
            }
            set
            {
                _heightAboveGeoID = value;
            }
        }

        #region utm projection
        // Parameters for the GWS84 ellipsoid
        const double WGS84_E2 = 0.006694379990197;
        const double WGS84_E4 = WGS84_E2 * WGS84_E2;
        const double WGS84_E6 = WGS84_E4 * WGS84_E2;
        const double WGS84_SEMI_MAJOR_AXIS = 6378137.0/*+134.5*/;//TODO: correct this value with altitude
        const double WGS84_SEMI_MINOR_AXIS = 6356752.314245;
        // Parameters for UTM projection
        const double UTM_LONGITUDE_OF_ORIGIN = 3.0 / 180 * Math.PI;
        const double UTM_LATITUDE_OF_ORIGIN = 0;
        const double UTM_FALSE_EASTING = 500000;
        const double UTM_FALSE_NORTHING_N = 0;    // Northern hemisphere
        const double UTM_FALSE_NORTHING_S = 10000000; // Southern hemisphere
        const double UTM_SCALE_FACTOR = 0.9996;

        // Takes a position in latitude / longitude (WGS84) as input
        // Returns position in UTM easting/northing/zone (in meters)
        public static void ProjectToUTM(double latitude, double longitude, out double easting, out double northing, out int zone)
        {
            // Normalize longitude into Zone, 6 degrees
            longitude = longitude - LongitudeOffset;

            int int_zone = (int)(longitude / LongitudeZoneSize);
            if (longitude < 0)
                int_zone--;

            longitude -= (double)int_zone * LongitudeZoneSize;
            zone = int_zone + 31;    // UTM zone



            // Convert from decimal degrees to radians
            longitude *= Math.PI / 180.0;
            latitude *= Math.PI / 180.0;
            // Projection
            double M = WGS84_SEMI_MAJOR_AXIS * m_calc(latitude);
            double M_origin = WGS84_SEMI_MAJOR_AXIS * m_calc(UTM_LATITUDE_OF_ORIGIN);
            double A = (longitude - UTM_LONGITUDE_OF_ORIGIN) * Math.Cos(latitude);
            double A2 = A * A;
            double e2_prim = WGS84_E2 / (1 - WGS84_E2);
            double C = e2_prim * Math.Pow(Math.Cos(latitude), 2);
            double T = Math.Tan(latitude);
            T *= T;
            double v = WGS84_SEMI_MAJOR_AXIS / Math.Sqrt(1 - WGS84_E2 * Math.Pow(Math.Sin(latitude), 2));

            northing = UTM_SCALE_FACTOR * (M - M_origin + v * Math.Tan(latitude) * (
                A2 / 2 + (5 - T + 9 * C + 4 * C * C) * A2 * A2 / 24 +
                (61 - 58 * T + T * T + 600 * C - 330 * e2_prim) *
                A2 * A2 * A2 / 720));

            if (latitude < 0)
            {
                northing += UTM_FALSE_NORTHING_S;
            }

            easting = UTM_FALSE_EASTING + UTM_SCALE_FACTOR * v * (
                A + (1 - T + C) * A2 * A / 6 +
                (5 - 18 * T + T * T + 72 * C - 58 * e2_prim) * A2 * A2 * A / 120);

        }
        private static double m_calc(double lat)
        {
            return (1 - WGS84_E2 / 4 - 3 * WGS84_E4 / 64 - 5 * WGS84_E6 / 256) * lat -
                 (3 * WGS84_E2 / 8 + 3 * WGS84_E4 / 32 + 45 * WGS84_E6 / 1024) *
                 Math.Sin(2 * lat) + (15 * WGS84_E4 / 256 + 45 * WGS84_E6 / 1024) *
                 Math.Sin(4 * lat) - (35 * WGS84_E6 / 3072) * Math.Sin(6 * lat);
        }
        public static double Bearing(double easting1, double northing1, double easting2, double northing2)
        {
            double a = 0;
            double dEast = easting2 - easting1;
            double dNorth = northing2 - northing1;
            if (dEast == 0)
            {
                if (dNorth < 0)
                {
                    a = Math.PI;
                }
            }
            else
            {
                a = -Math.Atan(dNorth / dEast) + Math.PI / 2;
            }
            if (dEast < 0)
                a = a + Math.PI;
            return a * 180.0 / Math.PI;    // Convert from radians to degrees
        }

        public ProjectedPoint Project()
        {
            double easting;
            double northing;
            int zone;
            ProjectToUTM(_latitude.Value, _longitude.Value, out easting, out northing, out zone);
            ProjectedPoint pp = new ProjectedPoint();
            pp.Easting = easting;
            pp.Northing = northing;
            pp.Zone = zone;
            pp.Height = _heightAboveGeoID;
            return pp;
        }
        #endregion
        #region haversine
        public static double HaversineDistance(CoordinatePoint pa, CoordinatePoint pb)
        {
            double R = 6371;//kilometers, otherwise 6371 for miles
            double dLat = toRadian(pa.Latitude.Value - pb.Latitude.Value);
            double dLon = toRadian(pa.Longitude.Value - pb.Longitude.Value);
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(toRadian(pa.Latitude.Value)) * Math.Cos(toRadian(pb.Latitude.Value)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Asin(Math.Min(1, Math.Sqrt(a)));
            double d = R * c;
            return d * 1000; //convert to meters
        }
        /// <summary>
        /// Convert to Radians.
        /// </summary>
        /// <param name=”val”></param>
        /// <returns></returns>
        private static double toRadian(double val)
        {
            return (Math.PI / 180) * val;
        }
        #endregion
        #region generic helpers
        //note, do not use this on coordinates, results WILL be innaccurate, project to UTM first
        //or use haversine instead.
        public static double TwoDimensionalDistance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow((x1 - x2), 2) + Math.Pow((y1 - y2), 2));
        }
        #endregion

        public override string ToString()
        {
            string lat = Latitude.ToString();
            if (Latitude.Value < 0)
            {
                lat = lat.Substring(1);
                lat = lat + " S";
            }
            else
            {
                lat = lat + " N";
            }

            string lon = Longitude.ToString();
            if (Longitude.Value < 0)
            {
                lon = lon.Substring(1);
                lon = lon + " W";
            }
            else
            {
                lon = lon + " E";
            }

            return lat + " " + lon;
        }
    }
    
    public class ProjectedPoint
    {
        public double Height { get; set; }
        public double Easting { get; set; }
        public double Northing { get; set; }
        public int Zone { get; set; }
    }
}
