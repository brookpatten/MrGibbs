using System;
using System.Collections.Generic;
using PovertySail.Contracts;
using PovertySail.Contracts.Infrastructure;

namespace PovertySail.HMC5883
{
	public partial class Hmc5883Plugin:IPlugin
	{
		private bool _initialized = false;
		private ILogger _logger;
		private IList<IPluginComponent> _components;

		public Hmc5883Plugin(ILogger logger)
		{
			_logger = logger;
		}

		public void Initialize(PluginConfiguration configuration, EventHandler onWatchButton, EventHandler onHeadingButton, EventHandler onSpeedButton)
		{
			_components = new List<IPluginComponent>();
			_initialized = false;
			var sensor = new Hmc5883Sensor(_logger,this);
			configuration.Sensors.Add(sensor);
			_components.Add(sensor);
		    _initialized = true;
		}

		public bool Initialized
		{
			get { return _initialized; }
		}

		public IList<IPluginComponent> Components
		{
			get { return _components; }
		}

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

