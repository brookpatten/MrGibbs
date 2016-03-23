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
		public void SailingNorthIntoSouthWindShouldShowSouthWind()
		{
			_state.StateValues [StateValue.ApparentWindDirection] = 0;
			_state.StateValues [StateValue.ApparentWindSpeedKnots] = 10;
			_state.StateValues [StateValue.CourseOverGroundByLocation] = 0;
			_state.StateValues [StateValue.SpeedInKnots] = 10;
			_calculator.Calculate (_state);

			Assert.AreEqual (20, _state.StateValues [StateValue.TrueWindSpeedKnots]);
			Assert.AreEqual (0, _state.StateValues [StateValue.TrueWindDirection]);
		}

		[Test]
		public void SailingSouthFromSouthWindShouldShowSouthWind()
		{
			_state.StateValues [StateValue.ApparentWindDirection] = 180;
			_state.StateValues [StateValue.ApparentWindSpeedKnots] = 1;
			_state.StateValues [StateValue.CourseOverGroundByLocation] = 180;
			_state.StateValues [StateValue.SpeedInKnots] = 9;
			_calculator.Calculate (_state);

			Assert.AreEqual (180, _state.StateValues [StateValue.TrueWindDirection],_directionDelta);
			Assert.AreEqual (10, _state.StateValues [StateValue.TrueWindSpeedKnots]);
		}

		[Test]
		public void SailingEastInSouthWindShouldShowSouthWind()
		{
			//                 |\
			//                 | \
			//            10kts|  \14.14kts apparant wind speed
			//            wind |   \
			//                 L____\ 315° apparant wind direction
			// 90° boat heading 10kts boat speed
			//wind appears to be 45 degrees to the left at ~14 knots
			_state.StateValues [StateValue.ApparentWindDirection] = 315;
			_state.StateValues [StateValue.ApparentWindSpeedKnots] = Math.Sqrt(Math.Pow(10,2)+Math.Pow(10,2));//~14.14

			//boat is traveling due east at 10 knots
			_state.StateValues [StateValue.CourseOverGroundByLocation] = 90;
			_state.StateValues [StateValue.SpeedInKnots] = 10;
			_calculator.Calculate (_state);

			var directionResult = _state.StateValues [StateValue.TrueWindDirection];

			//does't come out to exactly 0 or 360 due to precision, deal with it clumsily
			Assert.IsTrue((directionResult > 360-_directionDelta && _directionDelta < 360)
			              || (directionResult > 0 && directionResult < _directionDelta));

			Assert.AreEqual (10, _state.StateValues [StateValue.TrueWindSpeedKnots], _directionDelta);
		}
	}
}

