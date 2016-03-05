using System;
using System.Collections.Generic;

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
		private Device1 _device;
		private DBusConnection _connection;

		private double? _speed;
		private double? _direction;

		private string _serviceUUID="713d0000-503e-4c75-ba94-3148f18d941e";
		private string _charVendorName = "713D0001-503E-4C75-BA94-3148F18D941E";
		private string _charRead = "713D0002-503E-4C75-BA94-3148F18D941E";//rx
		private string _charWrite = "713D0003-503E-4C75-BA94-3148F18D941E";//tx
		private string _charAck = "713D0004-503E-4C75-BA94-3148F18D941E";
		private string _charVersion = "713D0005-503E-4C75-BA94-3148F18D941E";
		private string _clientCharacteristic = "00002902-0000-1000-8000-00805f9b34fb";
		private string _service = "org.bluez";

		private ObjectPath _gattProfilePath = new ObjectPath ("/gattprofiles");
		private ObjectPath _blueZPath = new ObjectPath ("/org/bluez");

		private ObjectPath _readCharPath; //= new ObjectPath("/org/bluez/hci0/dev_F6_58_7F_09_5D_E6/service000c/char000f");
		private GattCharacteristic1 _readChar;//= GetObject<GattCharacteristic1>(Service,readCharPath);
		private Properties _properties;// = GetObject<Properties>(Service,readCharPath);

		public BlendMicroAnemometerSensor(ILogger logger, BlendMicroAnemometerPlugin plugin,Device1 device, DBusConnection connection)
        {
            _plugin = plugin;
            _logger = logger;
			_device = device;
			_connection = connection;
		}

		private void InitializePropertyListener ()
		{
			_properties.PropertiesChanged += new PropertiesChangedHandler(
				new Action<string,IDictionary<string,object>,string[]>((@interface,changed,invalidated)=>{
					System.Console.WriteLine("Properties Changed on "+@interface);
					if(changed!=null)
					{
						foreach(var prop in changed.Keys)
						{
							if(changed[prop] is byte[])
							{
								DeserializeSensorValue ((byte[])changed [prop]);
							}
							else
							{
								System.Console.WriteLine("{0}={1}",prop,changed[prop]);
							}
						}
					}

					if(invalidated!=null)
					{
						foreach(var prop in invalidated)
						{
							System.Console.WriteLine(prop+" Invalidated");
						}
					}
				}));
		}

		private void DeserializeSensorValue (byte [] bytes)
		{
			_direction = 0;
			_speed = 0;
		}

        /// <inheritdoc />
        public IPlugin Plugin
        {
            get { return _plugin; }
        }

        /// <inheritdoc />
        public void Start()
        {
			//find service
			//find readChar

			_device.Connect();
			_readChar.StartNotify ();
			InitializePropertyListener ();
        }

        /// <inheritdoc />
        public void Update(Models.State state)
        {
			if (_direction.HasValue) 
			{
				state.StateValues [StateValue.ApparentWindDirection] = _direction.Value;
			}
			if (_speed.HasValue) 
			{
				state.StateValues [StateValue.ApparentWindSpeedKnots] = _speed.Value;
			}
        }

        /// <inheritdoc />
        public void Dispose()
        {
			try {
				_readChar.StopNotify ();
			} catch { }
			try {
				_device.Disconnect ();
			} catch { }
        }

        /// <inheritdoc />
        public void Calibrate()
        {
        }
    }
}
