using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using PovertySail.Contracts.Infrastructure;
using PovertySail.Contracts;
using PovertySail.Models;

namespace PovertySail.Calculators
{
    public class MagneticVariationCalculator:ICalculator
    {
        private ILogger _logger;
        private IPlugin _plugin;
        private MagVar _magVar;

        public MagneticVariationCalculator(ILogger logger, IPlugin plugin)
        {
            _plugin = plugin;
            _magVar = new MagVar();
            _logger = logger;
        }

        public void Calculate(State state)
        {
            double[] fields=new double[6];
            if (state.Location!=null && state.Location.Latitude!=null && state.Location.Longitude!=null && state.AltitudeInMeters.HasValue)
            {
                double julianDate = JulianDate.JD(state.BestTime);
                long jd = (long) julianDate;

                state.MagneticDeviation = _magVar.SGMagVar(state.Location.Latitude.Value, state.Location.Longitude.Value,
                    state.AltitudeInMeters.Value, jd, 10, fields);

                _logger.Info("Calculated Magnetic Deviation as " + state.MagneticDeviation + " for " + state.Location.Latitude.Value + "," + state.Location.Longitude.Value + " altitude " + state.AltitudeInMeters.Value);
            }
        }

        public IPlugin Plugin
        {
            get { return _plugin; }
        }

        public void Dispose()
        {
            
        }


    }

    public class JulianDate
    {
        public static bool isJulianDate(int year, int month, int day)
        {
            // All dates prior to 1582 are in the Julian calendar
            if (year < 1582)
                return true;
            // All dates after 1582 are in the Gregorian calendar
            else if (year > 1582)
                return false;
            else
            {
                // If 1582, check before October 4 (Julian) or after October 15 (Gregorian)
                if (month < 10)
                    return true;
                else if (month > 10)
                    return false;
                else
                {
                    if (day < 5)
                        return true;
                    else if (day > 14)
                        return false;
                    else
                        // Any date in the range 10/5/1582 to 10/14/1582 is invalid 
                        throw new ArgumentOutOfRangeException(
                            "This date is not valid as it does not exist in either the Julian or the Gregorian calendars.");
                }
            }
        }

        static private double DateToJD(int year, int month, int day, int hour, int minute, int second, int millisecond)
        {
            // Determine correct calendar based on date
            bool JulianCalendar = isJulianDate(year, month, day);

            int M = month > 2 ? month : month + 12;
            int Y = month > 2 ? year : year - 1;
            double D = day + hour / 24.0 + minute / 1440.0 + (second + millisecond * 1000) / 86400.0;
            int B = JulianCalendar ? 0 : 2 - Y / 100 + Y / 100 / 4;

            return (int)(365.25 * (Y + 4716)) + (int)(30.6001 * (M + 1)) + D + B - 1524.5;
        }

        static public double JD(int year, int month, int day, int hour, int minute, int second, int millisecond)
        {
            return DateToJD(year, month, day, hour, minute, second, millisecond);
        }


        static public double JD(DateTime date)
        {
            return DateToJD(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, date.Millisecond);
        }
    }
}
