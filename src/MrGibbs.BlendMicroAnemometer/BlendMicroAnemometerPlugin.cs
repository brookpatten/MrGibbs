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
		private IClock _clock;
		private TimeSpan _maximumDataAge;
		private bool _simulated;

		public BlendMicroAnemometerPlugin(ILogger logger,IClock clock,TimeSpan maximumDataAge,DBusConnection connection,string btAdapterName,string deviceAddress,bool simulated)
        {
			if (string.IsNullOrWhiteSpace (deviceAddress)) 
			{
				throw new ArgumentNullException ("deviceAddress");
			}
			_btAdapterName = btAdapterName;
			_deviceAddress = deviceAddress;
			_connection = connection;
            _logger = logger;
			_clock = clock;
			_maximumDataAge = maximumDataAge;
			_simulated = simulated;
		}

        /// <inheritdoc />
        public void Initialize(PluginConfiguration configuration, Action<Action<ISystemController, IRaceController>> queueCommand)
        {
			_components = new List<IPluginComponent>();
            _initialized = false;

			//TODO: maybe use the container or child container for this?
			ISensor sensor;
			//if (_simulated) {
				//sensor = new SimulatedWindSensor (_logger,this);
			//} //else {
				var bmas = new BlendMicroAnemometerSensor (_logger, _clock, _maximumDataAge, this,_btAdapterName,_deviceAddress, _connection);
				bmas.Start ();
				sensor = bmas;
			//}

			_components.Add (sensor);
			configuration.Sensors.Add (sensor);

			var trueCalculator = new TrueWindCalculator (_logger, this);
			_components.Add (trueCalculator);
			configuration.Calculators.Add (trueCalculator);

			var mastBendCalculator = new MastBendCalculator (_logger, this);
			_components.Add (mastBendCalculator);
			configuration.Calculators.Add (mastBendCalculator);

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
			if (_components != null) {
				foreach (var component in _components) {
					component.Dispose ();
				}
			} 
        }
    }
}
