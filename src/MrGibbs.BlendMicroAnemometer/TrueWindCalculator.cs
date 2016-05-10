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
			if (state.StateValues.ContainsKey(StateValue.ApparentWindDirection)
			    && state.StateValues.ContainsKey(StateValue.ApparentWindSpeedKnots)
			    && state.StateValues.ContainsKey(StateValue.SpeedInKnots)
			    && (state.StateValues.ContainsKey(StateValue.CourseOverGroundByLocation)
			        || state.StateValues.ContainsKey(StateValue.MagneticHeading)
			        || state.StateValues.ContainsKey(StateValue.MagneticHeadingWithVariation))) 
			{
				var boatPolar = new Vector2Polar ();
				var apparantWindPolar = new Vector2Polar ();

				//find the apprant wind vector
				apparantWindPolar.Radius = (float)state.StateValues[StateValue.ApparentWindSpeedKnots];
				apparantWindPolar.Theta = (float)AngleUtilities.DegreestoRadians(state.StateValues[StateValue.ApparentWindDirection]);

				//find the boat vector, this might come from several places, though we prefer gps
				boatPolar.Radius = (float)state.StateValues [StateValue.SpeedInKnots];
				double boatHeading=0;
				if (state.StateValues.ContainsKey (StateValue.CourseOverGroundByLocation)) 
				{
					boatHeading = state.StateValues [StateValue.CourseOverGroundByLocation];
				} 
				else if (state.StateValues.ContainsKey (StateValue.MagneticHeadingWithVariation)) 
				{
					boatHeading = state.StateValues [StateValue.MagneticHeadingWithVariation];
				} 
				else if (state.StateValues.ContainsKey (StateValue.MagneticHeading)) 
				{
					boatHeading = state.StateValues [StateValue.MagneticHeading];
				}
				boatPolar.Theta = (float)AngleUtilities.DegreestoRadians(boatHeading);

				//combine the vectors to find true wind
				var trueWindPolar = boatPolar.Add(apparantWindPolar);

				state.StateValues[StateValue.TrueWindDirection] = AngleUtilities.RadiansToDegrees(trueWindPolar.Theta);
				state.StateValues[StateValue.TrueWindSpeedKnots] = trueWindPolar.Radius;

				//calculate absolute wind
				state.StateValues [StateValue.AbsoluteWindDirection] = AngleUtilities.RadiansToDegrees (AngleUtilities.NormalizeAngle(trueWindPolar.Theta - boatPolar.Theta));
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

