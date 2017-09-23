using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Mono.BlueZ.DBus;
using DBus;
using org.freedesktop.DBus;

using MrGibbs.Contracts;
using MrGibbs.Models;
using MrGibbs.Contracts.Infrastructure;

namespace MrGibbs.BlunoBeetleBlePressure
{
	public class BlunoBeetleBlePressureSensor:ISensor
	{
		
		private ILogger _logger;
		private BlunoBeetleBlePressurePlugin _plugin;
		private IClock _clock;

		private class SensorReading
		{
			public double Temperature{ get; set; }
			public double Pressure{ get; set; }
			public double Altitude{get;set;}
		}

		private class PressureSensor
		{
			public Side Side{get;private set;}
			public int Index{get;private set;}
			public string DeviceAddress{ get; private set; }

			public double? DeltaMax{ get; set; }

			public PressureSensor(Side side,int index,string deviceAddress)
			{
				Side=side;
				Index=index;
				DeviceAddress = deviceAddress;
			}

			public Device1 Device{ get; set; }
			public SensorReading LatestReading{ get; set; }
			public DateTime? LastReceivedAt{ get; set; }
			public DateTime? LastReconnectAttempt{ get; set; }
			//private ObjectPath _gattProfilePath = new ObjectPath ("/gattprofiles");
			public ObjectPath ServicePath{ get; set; }
			public GattService1 Service{ get; set; }

			public ObjectPath ReadCharPath{ get; set; } //= new ObjectPath("/org/bluez/hci0/dev_F6_58_7F_09_5D_E6/service000c/char000f");
			public GattCharacteristic1 ReadChar{ get; set; }//= GetObject<GattCharacteristic1>(Service,readCharPath);
			public Properties Properties{ get; set; }// = GetObject<Properties>(Service,readCharPath);

			public Task Reconnect;

			public double? Offset{get;set;}
		}

		IList<PressureSensor> _sensors;

		private TimeSpan _maximumDataAge;

		private string _adapterName;

		private DBusConnection _connection;

		//0023
		//service 0000dfb0-0000-1000-8000-00805f9b34fb
		//0024
		//char 0000dfb1-0000-1000-8000-00805f9b34fb
		private string _serviceId = "0023";
		private string _readCharId = "0024";


		public BlunoBeetleBlePressureSensor(ILogger logger,IClock clock,TimeSpan maximumDataAge, BlunoBeetleBlePressurePlugin plugin,string adapterName,string portDeviceAddress,string starboardDeviceAddress, DBusConnection connection)
		{
			_plugin = plugin;
			_logger = logger;
			_adapterName = adapterName;

			_connection = connection;
			_clock = clock;
			_maximumDataAge = maximumDataAge;

			_sensors = new List<PressureSensor> ();
			_sensors.Add (new PressureSensor (Side.Port, 1, portDeviceAddress));
			_sensors.Add (new PressureSensor (Side.Starboard, 1, starboardDeviceAddress));
		}

		private void InitializePropertyListener (PressureSensor sensor)
		{
			sensor.Properties.PropertiesChanged += (@interface,changed,invalidated)=>{
				try {
					_logger.Debug ("GATT Properties Changed on " + @interface);
					if (changed != null) {
						foreach (var prop in changed.Keys) {
							_logger.Debug (prop + " changed");
							if (changed [prop] is byte [] && prop == "Value") {
								DeserializeSensorValue ((byte [])changed [prop],sensor);
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

		private void DeserializeSensorValue (byte [] bytes,PressureSensor sensor)
		{
			if (bytes.Length == 12)
			{
				sensor.LastReceivedAt = _clock.GetUtcTime ();

				var reading = new SensorReading ();
				reading.Temperature = BitConverter.ToSingle (bytes, 0);
				reading.Pressure = BitConverter.ToSingle (bytes, 4);
				reading.Altitude = BitConverter.ToSingle (bytes, 8);

				sensor.LatestReading = reading;

				_logger.Warn (string.Format ("Pressure {0},{1} t={2},p={3},a={4}", sensor.Side, sensor.Index, reading.Temperature, reading.Pressure, reading.Altitude));
			} else
			{
				_logger.Warn ("Received " + (bytes != null ? bytes.Length + " bytes " : "null ") + "from " + sensor.Index + " " + sensor.Side);

			}
		}

		/// <inheritdoc />
		public IPlugin Plugin
		{
			get { return _plugin; }
		}

		public void Start()
		{
			bool anySuccess = false;
			Exception lastException = null;

			foreach (var sensor in _sensors)
			{
				try
				{
					StartSensor (sensor);
					anySuccess=true;
				}
				catch(Exception ex)
				{
					lastException = ex;
				}
			}

			if (!anySuccess)
			{
				throw lastException;
			}
		}

		/// <inheritdoc />
		private void StartSensor(PressureSensor sensor)
		{
			sensor.Device = _connection.System.GetObject<Device1> (BlueZPath.Service, BlueZPath.Device (_adapterName, sensor.DeviceAddress));
			int retries = 3;
			for (int i = 0; i < retries;i++)
			{
				try 
				{
					_logger.Info ("Connecting "+sensor.Side+" "+sensor.Index+"...");
					sensor.Device.Connect ();
					_logger.Info ("Connected "+sensor.Side+" "+sensor.Index);
					System.Threading.Thread.Sleep (3000);
					break;
				} 
				catch(Exception ex)
				{
					_logger.Warn ("Failed",ex);
					//we can't really do much other than try again
					if (i == retries - 1) {
						throw new Exception ("Failed to connect to BLE Pressure Sensor "+sensor.Side+" "+sensor.Index, ex);
					} 
					else 
					{
						System.Threading.Thread.Sleep (3000);
					}
				}

			}

			string name = sensor.Device.Name;
			for (int i = 0; i < retries; i++) 
			{
				try 
				{
					var readCharPath = BlueZPath.GattCharacteristic (_adapterName, sensor.DeviceAddress, _serviceId, _readCharId);
					sensor.ReadChar = _connection.System.GetObject<GattCharacteristic1> (BlueZPath.Service, readCharPath);
					sensor.Properties = _connection.System.GetObject<Properties> (BlueZPath.Service, readCharPath);

					sensor.ReadChar.StartNotify ();
					InitializePropertyListener(sensor);
					_logger.Info ("Now listening for pressure data");
					break;
				} 
				catch (Exception ex) 
				{
					_logger.Warn ("Failed to configure listener",ex);

					if (i == retries - 1) 
					{
						throw new Exception ("Are you sure BlueZ is running in experimental mode?", ex);
					} 
					else 
					{
						System.Threading.Thread.Sleep (3000);
					}
				}
			}
		}

		/// <inheritdoc />
		public void Update(State state)
		{
			foreach (var sensor in _sensors)
			{
				if (sensor.LastReceivedAt.HasValue
					&& _clock.GetUtcTime () - sensor.LastReceivedAt < _maximumDataAge)
				{
					//the sensor is connected and "current"
					//if (sensor.LatestReading != null)
					//{
					//	state.StateValues [StateValue.BarometricPressurePascals] = _latestReading.Pressure;
					//	state.StateValues [StateValue.TemperatureCelsius] = _latestReading.Temperature;
					//}

				} else
				{
					sensor.LatestReading = null;

					//if we're not getting data attempt to reconnect
					if (!sensor.Device.Connected
						&& (!sensor.LastReconnectAttempt.HasValue
							|| _clock.GetUtcTime () - sensor.LastReconnectAttempt > new TimeSpan (0, 0, 5)))
					{
						//begin a reconnect thread out of process only if there isn't already one in progress
						if (sensor.Reconnect == null || sensor.Reconnect.IsFaulted || sensor.Reconnect.IsCanceled || sensor.Reconnect.IsCompleted)
						{
							sensor.Reconnect = BeginReconnect (sensor);
						}
					}
				}
			}

			//determine how to set pressure and temp by using the lowest indexed sensor that has data
			var lowest = _sensors.Where(x=>x.LatestReading!=null).OrderByDescending(x=>x.Index).FirstOrDefault();
			if (lowest != null)
			{
				state.StateValues [StateValue.BarometricPressurePascals] = lowest.LatestReading.Pressure;
				state.StateValues [StateValue.TemperatureCelsius] = lowest.LatestReading.Temperature;
			}

			//calculate deltas for each pairing
			for (int i = 1; i < 10; i++)
			{
				var port = _sensors.Where (x => x.Index == i && x.Side == Side.Port).SingleOrDefault ();
				var starboard = _sensors.Where (x => x.Index == i && x.Side == Side.Starboard).SingleOrDefault ();

				if (port != null && starboard != null)
				{
					if (port.LatestReading != null && starboard.LatestReading != null)
					{
						if (!port.Offset.HasValue || !starboard.Offset.HasValue)
						{
							if (port.LatestReading.Pressure > starboard.LatestReading.Pressure)
							{
								//this yeilds a negative offset
								port.Offset = starboard.LatestReading.Pressure - port.LatestReading.Pressure;
								starboard.Offset = 0;
							} else
							{
								starboard.Offset = port.LatestReading.Pressure - starboard.LatestReading.Pressure;
								port.Offset = 0;
							}
						}

						var delta = /*Math.Abs*/ ((port.LatestReading.Pressure + port.Offset.Value) - (starboard.LatestReading.Pressure + starboard.Offset.Value));
						state.StateValues [StateValue.BarometricPressureDelta] = delta;

						delta = Math.Abs (delta);

						if (!port.DeltaMax.HasValue || delta > port.DeltaMax)
						{
							port.DeltaMax = delta;
							starboard.DeltaMax = delta;
						}
						var percent = (delta / port.DeltaMax) * 100.0;
						_logger.Warn (string.Format ("Pressure Delta {0:0.00} ({1:0.0}%)", delta, percent));
						state.StateValues [StateValue.BarometricPressureDeltaPercent] = percent.Value;
					}
				} 
				else if (port == null && starboard == null)
				{
					break;
				}
			}
		}

		private Task BeginReconnect (PressureSensor sensor)
		{
			return Task.Factory.StartNew (() => {
				_logger.Warn("BLE Pressure Lost to "+sensor.Side+" "+sensor.Index+", Attempting to Reconnect");

				try 
				{
					sensor.Device.Connect ();
					//foreach(Delegate d in sensor.Properties.PropertiesChanged.GetInvocationList())
					//{
					//	sensor.Properties.PropertiesChanged -= (PropertiesChangedHandler)d;
					//}
					//StartSensor(sensor);
					_logger.Warn ("BLE Pressure Reconnected Successfully");
					sensor.LastReconnectAttempt = null;
				} 
				catch 
				{
					_logger.Warn ("BLE Pressure Reconnection Failed");
					sensor.LastReconnectAttempt = _clock.GetUtcTime ();
					throw;
				}
			});
		}

		/// <inheritdoc />
		public void Dispose()
		{
			foreach (var sensor in _sensors)
			{
				try
				{
					sensor.ReadChar.StopNotify ();
				} catch
				{ 
				}
				try
				{
					sensor.Device.Disconnect ();
				} catch
				{ 
				}
			}

		}

		/// <inheritdoc />
		public void Calibrate()
		{
			
		}
	}
}

