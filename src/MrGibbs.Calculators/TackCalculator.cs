using System;
using System.Collections.Generic;
using System.Linq;

using MrGibbs.Contracts.Infrastructure;
using MrGibbs.Contracts;
using MrGibbs.Models;


namespace MrGibbs.Calculators
{
    /// <summary>
    /// calculates stats based on most recent tack
    /// </summary>
    public class TackCalculator : ICalculator
    {
        class CourseHistory
        {
            public DateTime Time { get; set; }
            public double CourseOverGroundRadians { get; set; }
        }

        private ILogger _logger;
        private IPlugin _plugin;

        private double _tackThreshold = AngleUtilities.DegreestoRadians(60);
        private TimeSpan _tackThresholdTime = new TimeSpan(0, 0, 10);
        private DateTime? _lastTackAt;
        private TimeSpan _dataExclusionTime = new TimeSpan(0, 0, 10);

        private double? _previousTackCourseOverGroundRadians;
        private double? _currentTackStartCourseOverGroundRadians;

        private IList<CourseHistory> _history;
        
        public TackCalculator(ILogger logger, IPlugin plugin)
        {
            _history = new List<CourseHistory>();
            _plugin = plugin;
            _logger = logger;
        }

        /// <inheritdoc />
        public void Calculate(State state)
        {
			if (state.StateValues.ContainsKey(StateValue.CourseOverGroundDirection))
            {
				var cogRads = AngleUtilities.DegreestoRadians(state.StateValues[StateValue.CourseOverGroundDirection]);

				//make sure whe're not in an "exclusion" aka a few seconds before/after a known tack
				if (!_lastTackAt.HasValue || (_lastTackAt.Value + _dataExclusionTime < state.BestTime)) {
					if (!_currentTackStartCourseOverGroundRadians.HasValue) {
						_currentTackStartCourseOverGroundRadians = cogRads;
					}
					_history.Add (new CourseHistory () { Time = state.BestTime, CourseOverGroundRadians = cogRads });

					//make sure we have enough data to do the calculation accurately
					if (_history.Count > 1) {
						if (_history.Max (x => x.Time) - _history.Min (x => x.Time) > _tackThresholdTime) {
							CheckForTack (state);
						}
					}
				} 

                //calculate the delta on the current tack
				if (state.StateValues.ContainsKey(StateValue.CourseOverGroundDirection) && _currentTackStartCourseOverGroundRadians.HasValue)
                {
                    var delta = AngleUtilities.AngleDifference(cogRads, _currentTackStartCourseOverGroundRadians.Value);
					state.StateValues[StateValue.CurrentTackCourseOverGroundDelta] = AngleUtilities.RadiansToDegrees(delta);
                }
            }

            PurgeOldHistory();
        }

        /// <summary>
        /// compare the new state to the last values and determine if a tack has occured
        /// if so, update the state with the new tack
        /// </summary>
        /// <param name="state"></param>
        private void CheckForTack(State state)
        {
            var latest = _history.Last();

            var deltas = _history.Where(x=>x.Time > latest.Time - _tackThresholdTime).Select(x => Math.Abs(AngleUtilities.AngleDifference(latest.CourseOverGroundRadians, x.CourseOverGroundRadians))).Max();

            if(deltas>_tackThreshold)
            {
                //tack detected
                _lastTackAt = latest.Time;

                var priorToTack = _history.Where(x => x.Time < latest.Time - _dataExclusionTime).OrderByDescending(x => x.Time).FirstOrDefault();
                if (priorToTack != null)
                {
                    _previousTackCourseOverGroundRadians = priorToTack.CourseOverGroundRadians;
                }
                else
                {
                    _previousTackCourseOverGroundRadians = null;
                }

                _history.Clear();
                _currentTackStartCourseOverGroundRadians = null;
                var difference = AngleUtilities.AngleDifference(_previousTackCourseOverGroundRadians.Value, latest.CourseOverGroundRadians);
                var differenceDegrees = AngleUtilities.RadiansToDegrees(difference);

                string message = string.Format("Tack: {0:0.0}°", differenceDegrees);
                _logger.Info(message);
                state.AddMessage(MessageCategory.Tactical, MessagePriority.Normal, 5, message);
            }
        }

        /// <summary>
        /// used to purge history data prior to the most recent data when a tack occurs
        /// </summary>
        private void PurgeOldHistory()
        {
            if (_history.Count>1)
            {
                var latest = _history.Last();

                _history = _history.Where(x => x.Time > latest.Time - (_tackThresholdTime + _dataExclusionTime)).ToList();
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
			if (_history != null) 
			{
				_history.Clear ();
				_history = null;
			}
        }
    }
}
