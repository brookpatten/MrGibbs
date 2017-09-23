using System;
using System.Collections.Generic;

using Mono.BlueZ.DBus;
using DBus;
using org.freedesktop.DBus;

using MrGibbs.Contracts;
using MrGibbs.Contracts.Infrastructure;

namespace MrGibbs.BlunoBeetleBlePressure
{
	public class BlunoBeetleBlePressurePlugin:IPlugin
	{
		private ILogger _logger;
		private bool _initialized = false;
		private IList<IPluginComponent> _components;
		private DBusConnection _connection;
		private string _btAdapterName;
		private string _portAddress;
		private string _starboardAddress;
		private IClock _clock;
		private TimeSpan _maximumDataAge;
		private bool _simulated;

		public BlunoBeetleBlePressurePlugin(ILogger logger,IClock clock,TimeSpan maximumDataAge,DBusConnection connection,string btAdapterName,string portAddress,string starboardAddress, bool simulated)
		{

			_btAdapterName = btAdapterName;
			_portAddress = portAddress;
			_starboardAddress = starboardAddress;
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
			if (_simulated) 
			{
				sensor = null;
				//sensor = new SimulatedWindSensor (_logger,this);
			} 
			else 
			{
				if (string.IsNullOrWhiteSpace (_portAddress) && string.IsNullOrWhiteSpace (_starboardAddress)) 
				{
					throw new ArgumentNullException ("pressure sensor address");
				}

				var bmas = new BlunoBeetleBlePressureSensor (_logger, _clock, _maximumDataAge, this,_btAdapterName,_portAddress,_starboardAddress, _connection);
				bmas.Start ();
				sensor = bmas;
			}

			_components.Add (sensor);
			configuration.Sensors.Add (sensor);

			//var trueCalculator = new TrueWindCalculator (_logger, this);
			//_components.Add (trueCalculator);
			//configuration.Calculators.Add (trueCalculator);


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

