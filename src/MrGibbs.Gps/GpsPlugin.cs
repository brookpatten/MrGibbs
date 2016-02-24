using System;
using System.Collections.Generic;

using MrGibbs.Contracts;
using MrGibbs.Contracts.Infrastructure;

namespace MrGibbs.Gps
{
    /// <summary>
    /// plugin for communicating with a serial GPS
    /// maintains its own thread for reading data off the port
    /// also handles parsing of gps data via nmea
    /// </summary>
    public class GpsPlugin:IPlugin
    {
        private ILogger _logger;
        private bool _initialized = false;
        private IList<IPluginComponent> _components;
		private string _gpsPort;
		private int _gpsBaud;
		private bool _simulated;


		public GpsPlugin(ILogger logger,bool simulated,string gpsPort, int gpsBaud)
        {
			_simulated = simulated;
			_gpsPort = gpsPort;
			_gpsBaud = gpsBaud;
            _logger = logger;
        }

        /// <inheritdoc />
        public void Initialize(PluginConfiguration configuration, Action<Action<ISystemController, IRaceController>> queueCommand)
        {
            _components = new List<IPluginComponent>();
            _initialized = false;
			GpsSensor sensor;
			if (_simulated)
			{
				sensor = new SimulatedGpsSensor (_logger, this);
			}
			else
			{
				sensor = new GpsSensor(_logger, this, _gpsPort, _gpsBaud);
			}
			sensor.Start ();
            configuration.Sensors.Add(sensor);
            _components.Add(sensor);
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
