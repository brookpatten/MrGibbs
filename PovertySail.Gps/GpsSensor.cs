using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PovertySail.Contracts;
using PovertySail.Contracts.Infrastructure;
using System.IO.Ports;
using PovertySail.Gps.NMEA;
using PovertySail.Models;

namespace PovertySail.Gps
{
    public class GpsSensor:ISensor
    {
        private string _portName;
        private SerialPort _port;

        private ILogger _logger;
        private GpsPlugin _plugin;
        private Queue<string> _buffer;
        private NmeaParser _parser;
        private System.Globalization.CultureInfo _numberCulture;
        private Task _task;
        private bool _run = false;

        public GpsSensor(ILogger logger, GpsPlugin plugin,string serialPort)
        {
            _numberCulture = System.Globalization.CultureInfo.GetCultureInfo("en-us");
            _plugin = plugin;
            _logger = logger;
            _portName = serialPort;
            _parser = new NmeaParser();
        }

        public IPlugin Plugin
        {
            get { return _plugin; }
        }

        public void Start()
        {
            _buffer = new Queue<string>();
            _port = new SerialPort(_portName);
            _port.BaudRate = 4800;
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

        public void Update(Models.State state)
        {
            lock (_buffer)
            {
                double latitude = double.MinValue;
                double longitude = double.MinValue;
                double speed = double.MinValue;
                double height = double.MinValue;
                double courseOverGround=double.MinValue;
                DateTime? date = null;

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

                                string speedString = parsed[sentence]["Speed over ground"];
                                double.TryParse(speedString, out speed);
                                //speed = double.Parse(speedString, _numberCulture.NumberFormat);

                                string cogString = parsed[sentence]["Course Made Good"];
                                double.TryParse(cogString, out courseOverGround);

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
                        state.Speed = speed;
                    }
                    if (courseOverGround != double.MinValue)
                    {
                        state.CourseOverGround = courseOverGround;
                    }
                    if (date.HasValue)
                    {
                        state.Time = date.Value;
                    }
                }
                //update the state
            }
            
        }
        
        public void Dispose()
        {
            _run = false;
            _task.Wait(1000);

            if (_port.IsOpen)
            {
                _port.Close();
            }
            _port.Dispose();
            _buffer.Clear();
        }
    }
}
