using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MrGibbs.Models
{
    public class Coordinate
    {
        private double _value;
        public Coordinate(double value)
        {
            _value = value;
        }
        public Coordinate(int degrees, double minutes)
        {
            _value = CoordinateToDouble(degrees, minutes, 0);
        }
        public Coordinate(int degrees, int minutes, double seconds)
        {
            _value = CoordinateToDouble(degrees, (double)minutes, seconds);
        }
        public static double CoordinateToDouble(int degrees, double minutes, double seconds)
        {
            return (double)degrees + ((double)minutes / 60.0) + (seconds / 3600.0);
        }
        public double Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
            }
        }
        public int Degrees
        {
            get
            {
                double remainder = _value % 1.0;
                return (int)(_value - remainder);
            }
        }
        public int Minutes
        {
            get
            {
                double remainder = _value % 1.0;

                double minuteRemainder = remainder % (1.0 / 60.0);

                return (int)Math.Round((remainder - minuteRemainder) * 60.0);
            }
        }
        public double Seconds
        {
            get
            {
                double remainder = _value % 1.0;

                double minuteRemainder = remainder % (1.0 / 60.0);

                return Math.Round(minuteRemainder * 3600.0, 2);
            }
        }
        public override string ToString()
        {
            return Degrees + "º" + Minutes + "'" + Seconds;
        }
    }
}
