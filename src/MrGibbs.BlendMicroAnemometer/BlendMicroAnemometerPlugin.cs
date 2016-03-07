using System;
using System.Collections.Generic;

using Mono.BlueZ.DBus;
using DBus;
using org.freedesktop.DBus;

using MrGibbs.Contracts;
using MrGibbs.Contracts.Infrastructure;

namespace MrGibbs.BlendMicroAnemometer
{
    /// <summary>
    /// plugin to gather wind data from the blendmicroanemometer via BLE
    /// </summary>
    public class BlendMicroAnemometerPlugin:IPlugin
    {
        private ILogger _logger;
        private bool _initialized = false;
        private IList<IPluginComponent> _components;
		private DBusConnection _connection;
		private string _btAdapterName;
		private string _deviceAddress;
		private ObjectPath _devicePath;

        public BlendMicroAnemometerPlugin(ILogger logger,DBusConnection connection,string btAdapterName,string deviceAddress)
        {
			if (string.IsNullOrWhiteSpace (deviceAddress)) 
			{
				throw new ArgumentNullException ("deviceAddress");
			}
			_btAdapterName = btAdapterName;
			_deviceAddress = deviceAddress;
			_connection = connection;
            _logger = logger;
		}

        /// <inheritdoc />
        public void Initialize(PluginConfiguration configuration, Action<Action<ISystemController, IRaceController>> queueCommand)
        {
			_components = new List<IPluginComponent>();
            _initialized = false;

			_devicePath = BlueZPath.Device (_btAdapterName, _deviceAddress);
			var device = _connection.System.GetObject<Device1> (BlueZPath.Service, _devicePath);

			var sensor = new BlendMicroAnemometerSensor (_logger, this, device, _connection);
			sensor.Start ();
			_components.Add (sensor);
			configuration.Sensors.Add (sensor);

            _initialized = true;
        }

        /// <inheritdoc />
        public bool Initialized
        {
            get { return _initialized; }
        }

        /// <inheritdoc />
        public IList<IPluginComponent> Components
        {
            get { return _components; }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_components != null)
            {
                foreach (var component in _components)
                {
                    component.Dispose();
                }
            }
			if (_devicePath != null) 
			{
				_connection.System.Unregister (_devicePath);
			}
        }
    }
}
