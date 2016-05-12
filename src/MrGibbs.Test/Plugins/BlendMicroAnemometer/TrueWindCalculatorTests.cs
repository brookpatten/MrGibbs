using System;

using NUnit.Framework;

using MrGibbs.Models;
using MrGibbs.BlendMicroAnemometer;

namespace MrGibbs.Test
{
	[TestFixture]
	public class TrueWindCalculatorTests
	{
		private TrueWindCalculator _calculator;
		private State _state;
		private const double _directionDelta = 0.1;

		[SetUp]
		public void Setup ()
		{
			_state = new State ();
			//it's actually OK with nulls, which raises some questions....
			_calculator = new TrueWindCalculator (null, null);
		}

		[Test]
		public void SailingNorthIntoNorthWindShouldShowNorthWind()
		{
			_state.StateValues [StateValue.ApparentWindAngle] = 0;
			_state.StateValues [StateValue.ApparentWindSpeedKnots] = 20;
			_state.StateValues [StateValue.CourseOverGroundDirection] = 0;
			_state.StateValues [StateValue.SpeedInKnots] = 10;
			_calculator.Calculate (_state);

			Assert.AreEqual (0, _state.StateValues [StateValue.TrueWindAngle],"True Wind Angle");
			Assert.AreEqual (10, _state.StateValues [StateValue.TrueWindSpeedKnots],"True Wind Speed");
			Assert.AreEqual (360, _state.StateValues [StateValue.TrueWindDirection],_directionDelta,"True Wind Direction");
		}

		[Test]
		public void SailingSouthFromNorthWindShouldShowNorthWind()
		{
			_state.StateValues [StateValue.ApparentWindAngle] = 180;
			_state.StateValues [StateValue.ApparentWindSpeedKnots] = 10;
			_state.StateValues [StateValue.CourseOverGroundDirection] = 180;
			_state.StateValues [StateValue.SpeedInKnots] = 5;
			_calculator.Calculate (_state);

			Assert.AreEqual (180, _state.StateValues [StateValue.TrueWindAngle],_directionDelta,"True Wind Angle");
			Assert.AreEqual (15, _state.StateValues [StateValue.TrueWindSpeedKnots],"True Wind Speed");
			Assert.AreEqual (360, _state.StateValues [StateValue.TrueWindDirection],_directionDelta,"True Wind Direction");
		}

		[Test]
		public void SailingWestInNorthWindShouldShowSouthWind()
		{
			//                 |\
			//                 | \
			//            10kts|  \14.14kts apparant wind speed
			//          s wind |   \
			//                 L__<-- 45° apparant wind direction
			// 90° boat heading 10kts boat speed
			//wind appears to be 45 degrees to the left at ~14 knots
			_state.StateValues [StateValue.ApparentWindAngle] = 45;
			_state.StateValues [StateValue.ApparentWindSpeedKnots] = Math.Sqrt(Math.Pow(10,2)+Math.Pow(10,2));//~14.14

			//boat is traveling due east at 10 knots
			_state.StateValues [StateValue.CourseOverGroundDirection] = 270;
			_state.StateValues [StateValue.SpeedInKnots] = 10;
			_calculator.Calculate (_state);

			Assert.AreEqual (90, _state.StateValues [StateValue.TrueWindAngle], _directionDelta,"True Wind Angle");
			Assert.AreEqual (10, _state.StateValues [StateValue.TrueWindSpeedKnots], _directionDelta,"True Wind Speed");
			Assert.AreEqual (360, _state.StateValues [StateValue.TrueWindDirection],_directionDelta,"True Wind Direction");
		}

		[Test]
		public void SailingEastInNorthWindShouldShowSouthWind()
		{
			//                /|
			//               / | 
			//      14.14kts/  |10kts s wind
			//       app   /   |
			//       315° -->__|
			// 90° boat heading 10kts boat speed
			//wind appears to be 45 degrees to the left at ~14 knots
			_state.StateValues [StateValue.ApparentWindAngle] = 315;
			_state.StateValues [StateValue.ApparentWindSpeedKnots] = Math.Sqrt(Math.Pow(10,2)+Math.Pow(10,2));//~14.14

			//boat is traveling due east at 10 knots
			_state.StateValues [StateValue.CourseOverGroundDirection] = 90;
			_state.StateValues [StateValue.SpeedInKnots] = 10;
			_calculator.Calculate (_state);

			Assert.AreEqual (270, _state.StateValues [StateValue.TrueWindAngle], _directionDelta,"True Wind Angle");
			Assert.AreEqual (10, _state.StateValues [StateValue.TrueWindSpeedKnots], _directionDelta,"True Wind Speed");
			Assert.AreEqual (360, _state.StateValues [StateValue.TrueWindDirection],_directionDelta,"True Wind Direction");
		}
	}
}

