using System;

using MrGibbs.Models;
using MrGibbs.Contracts;
using MrGibbs.Contracts.Infrastructure;

namespace MrGibbs.BlendMicroAnemometer
{
	public class TrueWindCalculator:ICalculator
	{
		private ILogger _logger;
		private IPlugin _plugin;

		public TrueWindCalculator(ILogger logger, IPlugin plugin)
		{
			_plugin = plugin;
			_logger = logger;
		}

		/// <inheritdoc />
		public void Calculate(State state)
		{
			if (state.Location != null 
			    && state.StateValues.ContainsKey(StateValue.ApparentWindDirection)
			    && state.StateValues.ContainsKey(StateValue.ApparentWindSpeedKnots)
			    && state.StateValues.ContainsKey(StateValue.SpeedInKnots)) 
			{
				var boatPolar = new Vector2Polar ();
				var apparantWindPolar = new Vector2Polar ();

				boatPolar.Radius = state.StateValues [StateValue.SpeedInKnots];
				boatPolar.Theta = AngleUtilities.DegreestoRadians(state.StateValues[StateValue.CourseOverGroundByLocation]);

				apparantWindPolar.Radius = state.StateValues [StateValue.ApparentWindSpeedKnots];
				apparantWindPolar.Theta = AngleUtilities.DegreestoRadians (state.StateValues [StateValue.ApparentWindDirection]);

				var trueWindPolar = 
			}
		}

		/// <inheritdoc />
		public IPlugin Plugin
		{
			get { return _plugin; }
		}

		/// <inheritdoc />
		public void Dispose()
		{

		}
	}
}

