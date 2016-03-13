using System;
using System.Collections.Generic;
using System.Linq;

using MrGibbs.Contracts;
using MrGibbs.Models;
using MrGibbs.Contracts.Infrastructure;

namespace MrGibbs.BlendMicroAnemometer
{
	public class SimulatedWindSensor:ISensor
	{
		private ILogger _logger;
		private IPlugin _plugin;
		private IClock _clock;

		public SimulatedWindSensor(ILogger logger, IPlugin plugin)
		{
			_plugin = plugin;
			_logger = logger;
		}

		/// <inheritdoc />
		public IPlugin Plugin
		{
			get { return _plugin; }
		}

		/// <inheritdoc />
		public void Start()
		{
			
		}

		/// <inheritdoc />
		public void Update(State state)
		{
			state.StateValues [StateValue.ApparentWindDirection] = 45;
			state.StateValues [StateValue.ApparentWindSpeedKnots] =5;
			state.StateValues [StateValue.MastHeel] = 20;
			state.StateValues [StateValue.MastPitch] =2;
		}

		/// <inheritdoc />
		public void Dispose()
		{
			

		}

		/// <inheritdoc />
		public void Calibrate()
		{
			
		}
	}
}
