using System;
using System.Collections.Generic;

using Mono.Linux.I2C;

using MrGibbs.Contracts;
using MrGibbs.Contracts.Infrastructure;

namespace MrGibbs.HMC5883
{
    /// <summary>
    /// plugin for reading magnetic heading from an HMC5883 magnetometer
    /// </summary>
	public partial class Hmc5883Plugin:IPlugin
	{
		private bool _initialized = false;
		private ILogger _logger;
		private IList<IPluginComponent> _components;
		private Hmc5883 _compass;

		public Hmc5883Plugin(Hmc5883 compass,ILogger logger)
		{
			_compass = compass;
			_logger = logger;
		}

        /// <inheritdoc />
        public void Initialize(PluginConfiguration configuration, Action<Action<ISystemController, IRaceController>> queueCommand)
		{
			_components = new List<IPluginComponent>();
			_initialized = false;
			var sensor = new Hmc5883Sensor(_logger,this,_compass);
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
			_initialized = false;
			if(_components!=null)
			{
				foreach(var component in _components)
				{
					component.Dispose();
				}
			}
		}
	}
}

