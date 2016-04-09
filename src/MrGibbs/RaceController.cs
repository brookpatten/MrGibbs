using System;
using System.Collections.Generic;
using System.Linq;

using MrGibbs.Contracts;
using MrGibbs.Contracts.Infrastructure;
using MrGibbs.Models;

namespace MrGibbs
{
    /// <summary>
    /// Controller for sailboat race specific logic
    /// </summary>
    public class RaceController:IRaceController
    {
        private State _state;
        private ILogger _logger;
        private double? _autoRoundMarkDistanceMeters;

        public RaceController(ILogger logger, double autoRoundMarkDistanceMeters)
        {
            _state = new State();
            _logger = logger;
            _autoRoundMarkDistanceMeters = autoRoundMarkDistanceMeters;
        }

        /// <inheritdoc />
        public State State
        {
            get
            {
                return _state;  
            }
        }

        /// <inheritdoc />
        public void CountdownAction()
        {
            if (_state.StartTime.HasValue)
            {
                if (_state.StartTime > _state.BestTime)
                {
                    _logger.Info("Countdown Synced");
                    var remaining = _state.StartTime.Value - _state.BestTime;

                    if (remaining.Minutes >= 4)
                    {
                        _state.StartTime = _state.StartTime.Value.Subtract(new TimeSpan(0, 0, 0, remaining.Seconds, remaining.Milliseconds));
                        _state.AddMessage(MessageCategory.System, MessagePriority.Normal, 5, "Countdown synced");
                    }
                    else if (remaining.Minutes >= 3)
                    {
                        _state.StartTime = _state.StartTime.Value.Subtract(new TimeSpan(0, 0, 1, remaining.Seconds, remaining.Milliseconds));
                        _state.AddMessage(MessageCategory.System, MessagePriority.Normal, 5, "Countdown synced");
                    }
                    else
                    {
                        _state.StartTime = _state.StartTime.Value.Subtract(new TimeSpan(0, 0, 0, remaining.Seconds, remaining.Milliseconds));
                        _state.AddMessage(MessageCategory.System, MessagePriority.Normal, 5, "Countdown synced");
                    }
                }
                else
                {
                    _logger.Info("Countdown Reset");
                    _state.StartTime = null;
					_state.RaceStarted = false;
                    _state.AddMessage(MessageCategory.System, MessagePriority.Normal, 5, "Countdown reset");
                }
            }
            else
            {
                _logger.Info("Countdown Started");
				_state.RaceStarted = false;
                _state.StartTime = _state.BestTime.AddMinutes(5);
                _state.AddMessage(MessageCategory.System, MessagePriority.Normal, 5, "Countdown started");
            }
        }

        /// <inheritdoc />
        public void SetCourseType(int index)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void SetMarkLocation(MarkType markType)
        {
            if(_state.Course ==null || !(_state.Course is CourseByMarks))
            {
                _state.Course = new CourseByMarks();
            }

            if (_state.Location != null)
            {
                var course = _state.Course as CourseByMarks;

                if (course.Marks == null)
                {
                    course.Marks = new List<Mark>();
                }

                if (!course.Marks.Any())
                {
                    var mark = new Mark() { MarkType = markType, CaptureMethod = MarkCaptureMethod.Location, Location = _state.Location };

                    course.Marks.Add(mark);
                    State.TargetMark = mark;
                }
                else if (_state.TargetMark != null && _state.TargetMark.MarkType == markType)
                {
                    _state.TargetMark.CaptureMethod = MarkCaptureMethod.Location;
                    _state.TargetMark.Location = _state.Location;
                }
                else if (State.TargetMark != null && _state.TargetMark.MarkType != markType)
                {
                    var mark = new Mark() { MarkType = markType, CaptureMethod = MarkCaptureMethod.Location, Location = _state.Location };

                    course.Marks.Add(mark);
                    //State.TargetMark = mark;
                }
                else
                {
                    _logger.Error("User set mark location for " + markType + " but unsure what to do with it");
                }
            }
        }

        /// <inheritdoc />
        public void SetMarkBearing(MarkType markType, double bearing, bool magneticBearing)
        {
            if(markType==MarkType.Course)
            {
                if (_state.Course == null || !(_state.Course is CourseByAngle))
                {
                    _state.Course = new CourseByAngle();

                    if(magneticBearing)
                    {
						if(_state.StateValues.ContainsKey(StateValue.MagneticDeviation))
                        {
							(_state.Course as CourseByAngle).CourseAngle = bearing + _state.StateValues[StateValue.MagneticDeviation];
                        }
                        else
                        {
                            _logger.Error("Cannot set course angle using magnetic bearing without magnetic deviation!");
                            return;
                        }
                    }
                    else
                    {
                        (_state.Course as CourseByAngle).CourseAngle = bearing;
                    }
                }
            }
            else if (_state.Location != null)
            {
                if (_state.Course == null || !(_state.Course is CourseByMarks))
                {
                    _state.Course = new CourseByMarks();
                }
                
                Bearing fullBearing = new Bearing() { Location = _state.Location, RecordedAt = _state.BestTime, CompassHeading = bearing };

                //compensate for magnetic deviation
                if (magneticBearing)
                {
					if (_state.StateValues.ContainsKey(StateValue.MagneticDeviation))
                    {
						fullBearing.CompassHeading = fullBearing.CompassHeading + _state.StateValues[StateValue.MagneticDeviation];
                    }
                    else
                    {
                        _logger.Error("Cannot calculate mark location using magnetic bearing without magnetic deviation!");
                        return;
                    }
                }

				_logger.Info(string.Format("Received bearing for {0} of {1:0.00} ({2:0.00} true) from {3},{4}, altitude {5}, deviation {6}", markType, bearing, fullBearing.CompassHeading, fullBearing.Location.Latitude.Value, fullBearing.Location.Longitude.Value, _state.StateValues[StateValue.AltitudeInMeters], _state.StateValues[StateValue.MagneticDeviation]));

                Mark mark;

                var course = _state.Course as CourseByMarks;

                if (!course.Marks.Any(x => x.MarkType == markType))
                {
                    mark = new Mark() { MarkType = markType, CaptureMethod = MarkCaptureMethod.Bearing, Location = null };
                    mark.Bearings = new List<Bearing>();
                    mark.Bearings.Add(fullBearing);

                    course.Marks.Add(mark);
                    State.TargetMark = mark;
                }
                else
                {
                    mark = course.Marks.Where(x => x.MarkType == markType).Last();
                    mark.Bearings.Add(fullBearing);
                }

                if (mark.Bearings.Count > 1 && mark.CaptureMethod == MarkCaptureMethod.Bearing)
                {
                    var bearing1 = mark.Bearings[mark.Bearings.Count - 2];
                    var bearing2 = mark.Bearings[mark.Bearings.Count - 1];

                    var location = CoordinatePointUtilities.FindIntersection(bearing1.Location, bearing1.CompassHeading, bearing2.Location, bearing2.CompassHeading);
                    mark.Location = location;

                    _logger.Info(string.Format("Calculated new location of {0} via bearings to be {1},{2}", markType, mark.Location.Latitude.Value, mark.Location.Longitude.Value));

                    //TODO, if there's more than 2, do we average down?
                }
            }
        }

        /// <inheritdoc />
        public void ClearMark(int markIndex)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void NewRace()
        {
			_state = new State ();
        }

        /// <inheritdoc />
        public void NextMark()
        {
            if (_state.Course is CourseByMarks)
            {
                _state.PreviousMark = _state.TargetMark;
                _state.TargetMark = GetNextMark(_state.TargetMark);
            }
        }

        /// <summary>
        /// find the mark that is next in the course based on the current mark
        /// </summary>
        /// <param name="currentTargetMark">the mark that is prior to the one youre looking for</param>
        /// <returns>the the mark after the one specified as current</returns>
        private Mark GetNextMark(Mark currentTargetMark)
        {
            if (_state.Course is CourseByMarks)
            {
                var course = _state.Course as CourseByMarks;
                if (currentTargetMark == null)
                {
                    return course.Marks.LastOrDefault();
                }
                else if (currentTargetMark.MarkType == MarkType.Line)
                {
                    return course.Marks.Where(x => x.MarkType == MarkType.Windward).LastOrDefault();
                }
                else if (currentTargetMark.MarkType == MarkType.Windward)
                {
                    return course.Marks.Where(x => x.MarkType == MarkType.Leeward).LastOrDefault();
                }
                else if (currentTargetMark.MarkType == MarkType.Leeward)
                {
                    return course.Marks.Where(x => x.MarkType == MarkType.Windward).LastOrDefault();
                }
                else
                {
                    throw new InvalidOperationException("Unknown condition selecting next mark");
                }
            }
            else
            {
                return null;
            }
        }

        /// <inheritdoc />
        public void ProcessMarkRoundings()
        {
            if (_state.Course is CourseByMarks)
            {
                var course = _state.Course as CourseByMarks;
                //if the race just started, set the line
                if (_state.StartTime.HasValue && !_state.RaceStarted && _state.BestTime > _state.StartTime)
                {
					_logger.Info ("Race Started");
                    _state.RaceStarted = true;

                    var line = course.Marks.FirstOrDefault(x => x.MarkType == MarkType.Line);
                    if (line == null)
                    {
                        line = new Mark()
                        {
                            MarkType = MarkType.Line,
                            CaptureMethod = MarkCaptureMethod.Location,
                            Location = _state.Location
                        };
                        course.Marks.Insert(0, line);
                    }
                    _state.PreviousMark = line;

                    if (course.Marks.Any(x => x.MarkType == MarkType.Windward))
                    {
                        State.TargetMark = course.Marks.Where(x => x.MarkType == MarkType.Windward).Last();
                    }
                    else
                    {
                        State.TargetMark = null;
                    }
                }
                else if (_state.StartTime.HasValue && _state.BestTime > _state.StartTime && _state.TargetMark != null &&
                         _state.TargetMark.Location != null && _autoRoundMarkDistanceMeters.HasValue)
                {
                    var nextMark = GetNextMark(_state.TargetMark);
                    if (nextMark != null)
                    {
                        var distanceToMark = CoordinatePoint.HaversineDistance(_state.Location, _state.TargetMark.Location);
                        if (distanceToMark < _autoRoundMarkDistanceMeters)
                        {
                            _logger.Info("Distance to " + _state.TargetMark.MarkType + " is " + string.Format("{0:0.0}m") +
                                         ", advancing to next mark");
                            NextMark();
                        }
                    }
                }
            }
        }
    }
}
