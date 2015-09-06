using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MrGibbs.Contracts;
using MrGibbs.Contracts.Infrastructure;
using System.IO.Ports;
using MrGibbs.Gps.NMEA;
using MrGibbs.Models;

namespace MrGibbs.Gps
{
    public class GpsSensor:ISensor
    {
        private string _portName;
        private SerialPort _port;

        protected ILogger _logger;
        private GpsPlugin _plugin;
        protected Queue<string> _buffer;
        private NmeaParser _parser;
        private System.Globalization.CultureInfo _numberCulture;
        private Task _task;
        private bool _run = false;
        private int _baud;

        private double _minimumSpeed = 0.4;//knots

        public GpsSensor(ILogger logger, GpsPlugin plugin,string serialPort, int baud)
        {
            _numberCulture = System.Globalization.CultureInfo.GetCultureInfo("en-us");
            _plugin = plugin;
            _logger = logger;
            _portName = serialPort;
            _parser = new NmeaParser();
            _baud = baud;
        }

        public IPlugin Plugin
        {
            get { return _plugin; }
        }

        public virtual void Start()
        {
            _buffer = new Queue<string>();
            _port = new SerialPort(_portName);
            _port.BaudRate = _baud;

            _run = true;
            _task = new Task(Run);
            
            try
            {
                _port.Close();
            }
            catch { }
            _port.Open();
            _task.Start();
        }

        private void Run()
        {
            while (_run)
            {
                while (_port.BytesToRead > 0)
                {
                    string line = _port.ReadLine();
                    _logger.Debug("GPS Read:" + line);
                    lock (_buffer)
                    {
                        _buffer.Enqueue(line);
                    }
                }
            }
        }

        public virtual void Update(Models.State state)
        {
            lock (_buffer)
            {
                double latitude = double.MinValue;
                double longitude = double.MinValue;
                double speed = double.MinValue;
                double height = double.MinValue;
                double courseOverGroundByLocation=double.MinValue;

                double magneticCourseMadeGood=double.MinValue;
                double trueCourseMadeGood = double.MinValue;

                double magneticDeviation = double.MinValue;
                DateTime? date = null;

                double altitude = double.MinValue;

                while (_buffer.Count > 0)
                {
                    string line = _buffer.Dequeue();
                    try
                    {
                        var parsed = _parser.Parse(line);

                        foreach (var sentence in parsed.Keys)
                        {
                            _logger.Debug("GPS Parsed: "+sentence);
                            if (sentence == "Global Positioning System Fix Data")
                            {
                                //time
                                string timestring = parsed[sentence]["Time"];
                                char[] splitter = { '.' };
                                string[] parts = timestring.Split(splitter);
                                if (parts[0].Length == 6)
                                {
                                    int hour = int.Parse(parts[0].Substring(0, 2));
                                    int minute = int.Parse(parts[0].Substring(2, 2));
                                    int second = int.Parse(parts[0].Substring(4, 2));
                                    int millisecond = 0;
                                    if (parts.Length > 1)
                                    {
                                        millisecond = (int)(double.Parse("0." + parts[1], _numberCulture.NumberFormat) * 1000.0);
                                    }

                                    if (!date.HasValue)
                                    {
                                        date = DateTime.UtcNow;
                                    }

                                    date = new DateTime(date.Value.Year,date.Value.Month,date.Value.Day,hour,minute,second,millisecond);
                                }
                                else
                                {
                                    throw new Exception("Invalid Time Format");
                                }

                                //latitude
                                string latitudeString = parsed[sentence]["Latitude"];
                                string latitudeDirectionString = parsed[sentence]["Latitude Direction"];
                                int latdegrees = int.Parse(latitudeString.Substring(0, 2));
                                double latminute = double.Parse(latitudeString.Substring(2), _numberCulture.NumberFormat);
                                latitude = Coordinate.CoordinateToDouble(latdegrees, latminute, 0);
                                if (latitudeDirectionString.ToLower() == "s")
                                {
                                    latitude = -latitude;
                                }

                                //longitude
                                string longitudeString = parsed[sentence]["Longitude"];
                                string longitudeDirectionString = parsed[sentence]["Longitude Direction"];
                                int longdegrees = int.Parse(longitudeString.Substring(0, 3));
                                double longminute = double.Parse(longitudeString.Substring(3), _numberCulture.NumberFormat);
                                longitude = Coordinate.CoordinateToDouble(longdegrees, longminute, 0);
                                if (longitudeDirectionString.ToLower() == "w")
                                {
                                    longitude = -longitude;
                                }

                                string altitudeString = parsed[sentence]["Altitude"];
                                string altitudeUnitString = parsed[sentence]["Altitude Unit"];
                                if (double.TryParse(altitudeString, out altitude))
                                {
                                    if (altitudeUnitString.Trim().ToLower() == "m")
                                    {
                                        //do nothing, it's meters
                                    }
                                    else if (altitudeUnitString.Trim().ToLower() == "k")
                                    {
                                        altitude = altitude * 1000.0;
                                    }
                                }
                                
                            }
                            else if (sentence == "Recommended Minimum Specific GPS/TRANSIT Data")
                            {
                                //time
                                //don't uuse this because it doesn't have millis
                                //string timestring = parsed[sentence]["Time of Fix"];
                                //char[] splitter = { '.' };
                                //string[] parts = timestring.Split(splitter);
                                //if (parts[0].Length == 6)
                                //{
                                //    int hour = int.Parse(parts[0].Substring(0, 2));
                                //    int minute = int.Parse(parts[0].Substring(2, 2));
                                //    int second = int.Parse(parts[0].Substring(4, 2));
                                //    time = new TimeSpan(0, hour, minute, second, 0);
                                //}
                                //else
                                //{
                                //    throw new Exception("Invalid Time Format");
                                //}

                                
                                //latitude
                                string latitudeString = parsed[sentence]["Latitude"];
                                string latitudeDirectionString = parsed[sentence]["Latitude Direction"];
                                if (latitudeString.Length >= 3)
                                {
                                    int latdegrees = int.Parse(latitudeString.Substring(0, 2));
                                    double latminute = double.Parse(latitudeString.Substring(2), _numberCulture.NumberFormat);
                                    latitude = Coordinate.CoordinateToDouble(latdegrees, latminute, 0);
                                    if (latitudeDirectionString.ToLower() == "s")
                                    {
                                        latitude = -latitude;
                                    }
                                }

                                //longitude
                                string longitudeString = parsed[sentence]["Longitude"];
                                string longitudeDirectionString = parsed[sentence]["Longitude Direction"];
                                if (longitudeString.Length >= 3)
                                {
                                    int longdegrees = int.Parse(longitudeString.Substring(0, 3));
                                    double longminute = double.Parse(longitudeString.Substring(3), _numberCulture.NumberFormat);
                                    longitude = Coordinate.CoordinateToDouble(longdegrees, longminute, 0);
                                    if (longitudeDirectionString.ToLower() == "w")
                                    {
                                        longitude = -longitude;
                                    }
                                }

                                string speedString = parsed[sentence]["Speed over ground"];
                                double.TryParse(speedString, out speed);
                                //speed = double.Parse(speedString, _numberCulture.NumberFormat);

                                string cogString = parsed[sentence]["Course Made Good"];
                                double.TryParse(cogString, out courseOverGroundByLocation);

                                string datestring = parsed[sentence]["Date of Fix"];
                                if (datestring.Length == 6)
                                {
                                    int day = int.Parse(datestring.Substring(0, 2));
                                    int month = int.Parse(datestring.Substring(2, 2));
                                    int year = 2000 + int.Parse(datestring.Substring(4, 2));

                                    if (!date.HasValue)
                                    {
                                        date = DateTime.UtcNow;
                                    }

                                    date = new DateTime(year,month,day, date.Value.Hour, date.Value.Minute, date.Value.Second, date.Value.Millisecond);
                                }
                                else
                                {
                                    throw new Exception("Invalid Time Format");
                                }

                                string magneticVariationString = parsed[sentence]["Magnetic Variation"];
                                if(double.TryParse(magneticVariationString, out magneticDeviation))
                                {
                                    if(parsed[sentence]["Magnetic Variation Direction"].ToLower()=="w")
                                    {
                                        magneticDeviation = -magneticDeviation;
                                    }
                                }
                            }
                            else if (sentence == "Data and Time")
                            {
                                //time
                                string timestring = parsed[sentence]["Time"];
                                char[] splitter = { '.' };
                                string[] parts = timestring.Split(splitter);
                                if (parts[0].Length == 6)
                                {
                                    int hour = int.Parse(parts[0].Substring(0, 2));
                                    int minute = int.Parse(parts[0].Substring(2, 2));
                                    int second = int.Parse(parts[0].Substring(4, 2));
                                    int millisecond = (int)(double.Parse("0." + parts[1], _numberCulture.NumberFormat) * 1000.0);
                                    int day = int.Parse(parsed[sentence]["Day"]);
                                    int month = int.Parse(parsed[sentence]["Month"]);
                                    int year = int.Parse(parsed[sentence]["Year"]);

                                    if (!date.HasValue)
                                    {
                                        date = DateTime.UtcNow;
                                    }

                                    date = new DateTime(year, month, day, hour,minute,second,millisecond);
                                }
                                else
                                {
                                    throw new Exception("Invalid Time Format");
                                }
                            }
                            else if (sentence == "Track Made Good and Ground Speed")
                            {
                                string trueCourseString = parsed[sentence]["True Course Made Good Over Gound"];
                                double.TryParse(trueCourseString, out trueCourseMadeGood);
                                string magneticCourseStrin = parsed[sentence]["Magnetic Course Made Good over Ground"];
                                double.TryParse(magneticCourseStrin, out magneticCourseMadeGood);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Warn("Exception parsing gps line:"+line);
                    }
                }

                lock (state)
                {
                    if (latitude != double.MinValue && longitude != double.MinValue)
                    {
                        state.Location = new CoordinatePoint(new Coordinate(latitude), new Coordinate(longitude), height);
                    }
                    if (speed != double.MinValue)
                    {
                        state.SpeedInKnots = speed;

                        //don't set COG if we're not really moving
                        if (courseOverGroundByLocation != double.MinValue && speed>_minimumSpeed)
                        {
                            state.CourseOverGroundByLocation = courseOverGroundByLocation;
                        }
                    }
                    
                    if (date.HasValue)
                    {
                        state.GpsTime = date.Value;
                    }

                    if (trueCourseMadeGood != double.MinValue)
                    {
                        state.TrueCourseMadeGood = trueCourseMadeGood;
                    }
                    if (magneticCourseMadeGood != double.MinValue)
                    {
                        state.MagneticCourseMadeGood = magneticCourseMadeGood;
                    }
                    //if (magneticDeviation != double.MinValue)
                    //{
                    //    _logger.Info("GPS set variation to "+magneticDeviation);
                    //    state.MagneticDeviation = magneticDeviation;
                    //}

                    if (altitude != double.MinValue)
                    {
                        //_logger.Info("GPS set altitude to " + altitude);
                        state.AltitudeInMeters = altitude;
                    }
                }
                //update the state
            }
            
        }
        
        public virtual void Dispose()
        {
            _run = false;
            _task.Wait(1000);

            if (_port != null)
            {
                if (_port.IsOpen)
                {
                    _port.Close();
                }
                _port.Dispose();
            }
            _buffer.Clear();
        }


        public void Calibrate()
        {
        }
    }
}
