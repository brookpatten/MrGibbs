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
		private Task _reconnect;
		private ILogger _logger;
		private BlunoBeetleBlePressurePlugin _plugin;
		private IClock _clock;

		private class SensorReading
		{
			public double Temperature{ get; set; }
			public double Pressure{ get; set; }
			public double Altitude{get;set;}
		}

		private SensorReading _latestReading;
		private DateTime? _lastReceivedAt;
		private DateTime? _lastReconnectAttempt;
		private TimeSpan _maximumDataAge;

		private string _adapterName;
		private string _deviceAddress;
		private Device1 _device;
		private DBusConnection _connection;
		private ObjectPath _gattProfilePath = new ObjectPath ("/gattprofiles");
		private ObjectPath _servicePath;
		private GattService1 _service;

		//0023
		//service 0000dfb0-0000-1000-8000-00805f9b34fb
		//0024
		//char 0000dfb1-0000-1000-8000-00805f9b34fb
		private string _serviceId = "0023";
		private string _readCharId = "0024";

		private ObjectPath _readCharPath; //= new ObjectPath("/org/bluez/hci0/dev_F6_58_7F_09_5D_E6/service000c/char000f");
		private GattCharacteristic1 _readChar;//= GetObject<GattCharacteristic1>(Service,readCharPath);
		private Properties _properties;// = GetObject<Properties>(Service,readCharPath);

		public BlunoBeetleBlePressureSensor(ILogger logger,IClock clock,TimeSpan maximumDataAge, BlunoBeetleBlePressurePlugin plugin,string adapterName,string deviceAddress, DBusConnection connection)
		{
			_plugin = plugin;
			_logger = logger;
			_adapterName = adapterName;
			_deviceAddress = deviceAddress;
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
			if (bytes.Length == 12) 
			{
				_lastReceivedAt = _clock.GetUtcTime ();

				var reading = new SensorReading ();
				reading.Temperature = BitConverter.ToDouble (bytes, 0);
				reading.Pressure = BitConverter.ToDouble (bytes, 4);
				reading.Altitude = BitConverter.ToDouble (bytes, 8);

				_logger.Warn(string.Format ("t={0},p={1},a={2}", reading.Temperature, reading.Pressure, reading.Altitude));

			}
		}

		/// <inheritdoc />
		public IPlugin Plugin
		{
			get { return _plugin; }
		}


		/// <inheritdoc />
		public void Start()
		{
			_device = _connection.System.GetObject<Device1> (BlueZPath.Service, BlueZPath.Device (_adapterName, _deviceAddress));
			int retries = 3;
			for (int i = 0; i < retries;i++)
			{
				try 
				{
					_logger.Info ("Connecting...");
					_device.Connect ();
					_logger.Info ("Connected");
					System.Threading.Thread.Sleep (3000);
					break;
				} 
				catch(Exception ex)
				{
					_logger.Warn ("Failed",ex);
					//we can't really do much other than try again
					if (i == retries - 1) {
						throw new Exception ("Failed to connect to BLE Anemometer", ex);
					} 
					else 
					{
						System.Threading.Thread.Sleep (3000);
					}
				}

			}

			string name = _device.Name;
			for (int i = 0; i < retries; i++) 
			{
				try 
				{
					var readCharPath = BlueZPath.GattCharacteristic (_adapterName, _deviceAddress, _serviceId, _readCharId);
					_readChar = _connection.System.GetObject<GattCharacteristic1> (BlueZPath.Service, readCharPath);
					_properties = _connection.System.GetObject<Properties> (BlueZPath.Service, readCharPath);

					_readChar.StartNotify ();
					InitializePropertyListener ();
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
			if (_lastReceivedAt.HasValue 
				&& _clock.GetUtcTime () - _lastReceivedAt < _maximumDataAge) 
			{
				if (_latestReading!=null) 
				{
					state.StateValues [StateValue.BarometricPressurePascals] = _latestReading.Pressure;
					state.StateValues [StateValue.TemperatureCelsius] = _latestReading.Temperature;
				}

			} 
			else 
			{
				_latestReading = null;

				//if we're not getting data attempt to reconnect
				if (!_device.Connected
					&& (!_lastReconnectAttempt.HasValue 
						|| _clock.GetUtcTime()-_lastReconnectAttempt > new TimeSpan(0,0,5))) 
				{
					//begin a reconnect thread out of process only if there isn't already one in progress
					if (_reconnect == null || _reconnect.IsFaulted || _reconnect.IsCanceled || _reconnect.IsCompleted) {
						_reconnect = BeginReconnect ();
					}
				}
			}
		}

		private Task BeginReconnect ()
		{
			return Task.Factory.StartNew (() => {
				_logger.Warn("BLE Pressure Lost, Attempting to Reconnect");

				try 
				{
					_device.Connect ();
					_logger.Warn ("BLE Pressure Reconnected Successfully");
					_lastReconnectAttempt = null;
				} 
				catch 
				{
					_logger.Warn ("BLE Pressure Reconnection Failed");
					_lastReconnectAttempt = _clock.GetUtcTime ();
					throw;
				}
			});
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
			
		}
	}
}

