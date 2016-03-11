using System;
using System.Collections.Generic;
using System.Linq;

using Mono.BlueZ.DBus;
using DBus;
using org.freedesktop.DBus;

using MrGibbs.Contracts;
using MrGibbs.Models;
using MrGibbs.Contracts.Infrastructure;

namespace MrGibbs.BlendMicroAnemometer
{
    /// <summary>
    /// sensor implementation of the blend micro wind sensor
    /// </summary>
    public class BlendMicroAnemometerSensor:ISensor
    {
        private ILogger _logger;
        private BlendMicroAnemometerPlugin _plugin;
		private IClock _clock;

		private double? _speed;
		private double? _direction;
		private double? _heel;
		private double? _pitch;
		private DateTime? _lastReceivedAt;
		private DateTime? _lastReconnectAttempt;
		private TimeSpan _maximumDataAge;
		private const double MphToKnots = 0.868976;
		private const double AccelFactor = 1.0 / 16384.0;

		private string _serviceUUID="713d0000-503e-4c75-ba94-3148f18d941e";
		private string _charVendorName = "713D0001-503E-4C75-BA94-3148F18D941E";
		private string _charRead = "713D0002-503E-4C75-BA94-3148F18D941E";//rx
		private string _charWrite = "713D0003-503E-4C75-BA94-3148F18D941E";//tx
		private string _charAck = "713D0004-503E-4C75-BA94-3148F18D941E";
		private string _charVersion = "713D0005-503E-4C75-BA94-3148F18D941E";
		private string _clientCharacteristic = "00002902-0000-1000-8000-00805f9b34fb";

		private Device1 _device;
		private DBusConnection _connection;
		private ObjectPath _gattProfilePath = new ObjectPath ("/gattprofiles");
		private ObjectPath _servicePath;
		private GattService1 _service;
		private ObjectPath _readCharPath; //= new ObjectPath("/org/bluez/hci0/dev_F6_58_7F_09_5D_E6/service000c/char000f");
		private GattCharacteristic1 _readChar;//= GetObject<GattCharacteristic1>(Service,readCharPath);
		private Properties _properties;// = GetObject<Properties>(Service,readCharPath);

		public BlendMicroAnemometerSensor(ILogger logger,IClock clock,TimeSpan maximumDataAge, BlendMicroAnemometerPlugin plugin,Device1 device, DBusConnection connection)
        {
            _plugin = plugin;
            _logger = logger;
			_device = device;
			_connection = connection;
			_clock = clock;
			_maximumDataAge = maximumDataAge;
		}

		private void InitializePropertyListener ()
		{
			_properties.PropertiesChanged += (@interface,changed,invalidated)=>{
				try {
					_logger.Debug ("GATT Properties Changed on " + @interface);
					if (changed != null) {
						foreach (var prop in changed.Keys) {
							_logger.Debug (prop + " changed");
							if (changed [prop] is byte [] && prop == "Value") {
								DeserializeSensorValue ((byte [])changed [prop]);
							}
						}
					}

					if (invalidated != null) {
						foreach (var prop in invalidated) {
							_logger.Debug (prop + " invalidated");
						}
					}
				} 
				catch (Exception ex) 
				{
					//we do not want to bubble any exceptions back into dbus-sharp because it will crash
					//the dbus event loop
					_logger.Warn ("Exception Processing GATT Change", ex);
				}
			};
		}

		private void DeserializeSensorValue (byte [] bytes)
		{
			if (bytes.Length == 15) 
			{
				_lastReceivedAt = _clock.GetUtcTime ();
				uint anemometerDifference;
				uint vaneDifference;
				short x;
				short y;
				short z;

				anemometerDifference = BitConverter.ToUInt32 (new byte [] { bytes [3], bytes [2], bytes [1], bytes [0] }, 0);
				vaneDifference = BitConverter.ToUInt32 (new byte [] { bytes [7], bytes [6], bytes [5], bytes [4] }, 0);

				x = BitConverter.ToInt16 (bytes, 8);
				y = BitConverter.ToInt16 (bytes, 10);
				z = BitConverter.ToInt16 (bytes, 12);

				_logger.Info ("Received BLE Data from Blend Micro");
				_logger.Info (string.Format ("a={0},v={1},x={2},y={3},z={4}", anemometerDifference, vaneDifference, x, y, z));

				_direction = CalculateAngle (vaneDifference, anemometerDifference);
				_speed = CalculateSpeedInKnots (anemometerDifference);

				_heel = (double)x * AccelFactor * (360.0 / 4.0);
				_pitch = (double)y * AccelFactor * (360.0 / 4.0);

				_logger.Info (string.Format ("speed={0:0.0},direction={1:0.0},heel={2:0.0},pitch={3:0.0}", _speed, _direction, _heel, _pitch));
			}
		}

        /// <inheritdoc />
        public IPlugin Plugin
        {
            get { return _plugin; }
        }

		/// <summary>
		/// given ms between closures, returns speed in mph
		/// per peet bros ultimeter pro documentation
		/// </summary>
		/// <returns>The speed.</returns>
		/// <param name="closureRate">Closure rate.</param>
		private double CalculateSpeedInKnots(long closureRate)
		{
			double rps = 1000.0/(double)closureRate;

			double mph;

			//initial calculations are all mph, so that constants match peet bros documentation
			if(0.010 < rps && rps < 3.23)
			{
				mph = -0.1095*(rps*rps) + 2.9318*rps - 0.1412;
			}
			else if(3.23 <= rps && rps <54.362)
			{
				mph = 0.0052 * (rps * rps) + 2.1980 * rps + 1.1091;
			}
			else if(54.362 <= rps && rps < 66.332)
			{
				mph = 0.1104 * (rps * rps) - 9.5685 * rps + 329.87;
			}
			else
			{
				mph = 0.0;
			}

			//convert to knots
			//TODO: move conversions like this to somewhere handy
			double knots = mph * MphToKnots;
			return knots;
		}

		/// <summary>
		/// Calculates the angle of the vane based on when it was passed by the anemometer
		/// </summary>
		/// <returns>The angle.</returns>
		/// <param name="vaneDifference">Vane difference.</param>
		/// <param name="anemometerDifference">Anemometer difference.</param>
		private double CalculateAngle(long vaneDifference,long anemometerDifference)
		{
			return ((double)vaneDifference/(double)anemometerDifference)*360.0;
		}

        /// <inheritdoc />
        public void Start()
        {
			while (!_device.Connected) 
			{
				_device.Connect ();
				System.Threading.Thread.Sleep (3000);
			}

			string name = _device.Name;
			var servicePaths = _device.GattServices;
			_servicePath = servicePaths.Single();
			_service = _connection.System.GetObject<GattService1> (BlueZPath.Service, _servicePath);
			var charPaths = _service.Characteristics;
			foreach (var charPath in charPaths) 
			{
				var vChar = _connection.System.GetObject<GattCharacteristic1> (BlueZPath.Service, charPath);

				if (vChar.UUID.ToUpper() == _charRead) 
				{
					_readChar = vChar;
					_properties = _connection.System.GetObject<Properties> (BlueZPath.Service, charPath);
					break;
				}
			}

			_readChar.StartNotify();
			InitializePropertyListener ();
        }

        /// <inheritdoc />
        public void Update(State state)
        {
			if (_lastReceivedAt.HasValue 
			    && _clock.GetUtcTime () - _lastReceivedAt < _maximumDataAge) 
			{
				if (_direction.HasValue) 
				{
					state.StateValues [StateValue.ApparentWindDirection] = _direction.Value;
				}
				if (_speed.HasValue) 
				{
					state.StateValues [StateValue.ApparentWindSpeedKnots] = _speed.Value;
				}
				if (_heel.HasValue) 
				{
					state.StateValues [StateValue.MastHeel] = _heel.Value;
				}
				if (_pitch.HasValue) 
				{
					state.StateValues [StateValue.MastPitch] = _pitch.Value;
				}
			} 
			else 
			{
				_direction = null;
				_speed = null;
				_heel = null;
				_pitch = null;

				//if we're not getting data attempt to reconnect
				if (!_device.Connected
				    && (!_lastReconnectAttempt.HasValue 
				        || _clock.GetUtcTime()-_lastReconnectAttempt > new TimeSpan(0,0,3))) 
				{
					_logger.Warn("BLE Connection to Anemometer Lost, Attempting to Reconnect");
					_lastReconnectAttempt = _clock.GetUtcTime ();
					_device.Connect ();
				}
			}
        }

        /// <inheritdoc />
        public void Dispose()
        {
			try 
			{
				_readChar.StopNotify ();
			} 
			catch 
			{ 
			}
			try 
			{
				_device.Disconnect ();
			} 
			catch 
			{ 
			}

        }

        /// <inheritdoc />
        public void Calibrate()
        {
			//TODO: send some sort of calibration message to the blend to tell it the accel is centered
        }
    }
}
