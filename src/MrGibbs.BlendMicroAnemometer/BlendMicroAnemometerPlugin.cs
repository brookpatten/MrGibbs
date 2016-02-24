using System;
using System.Collections.Generic;

using Mono.BlueZ.DBus;

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

        public BlendMicroAnemometerPlugin(ILogger logger,DBusConnection connection)
        {
			_connection = connection;
            _logger = logger;
        }

        /// <inheritdoc />
        public void Initialize(PluginConfiguration configuration, Action<Action<ISystemController, IRaceController>> queueCommand)
        {
            _components = new List<IPluginComponent>();
            _initialized = false;
            configuration.Sensors.Add(new BlendMicroAnemometerSensor(_logger,this));
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
        }
    }
}
