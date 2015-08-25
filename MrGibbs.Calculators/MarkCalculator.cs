using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MrGibbs.Contracts.Infrastructure;
using MrGibbs.Contracts;
using MrGibbs.Models;

namespace MrGibbs.Calculators
{
    public class MarkCalculator:ICalculator
    {
        class MarkCalculation
        {
            public CoordinatePoint Location{get;set;}
            public DateTime Time{get;set;}
            public double? VelocityMadeGoodOnCourse{get;set;}
            public double? VelocityMadeGood{get;set;}
        }

        private ILogger _logger;
        private IPlugin _plugin;
        private const double MetersToYards = 1.09361;
        public const double MetersPerSecondToKnots = 1.94384;
        private readonly double _distanceCutoff = 100 * 1000;//100km

        private const int _previousCalculationCount = 5;

        private IList<MarkCalculation> _previousCalculations;

        public MarkCalculator(ILogger logger, IPlugin plugin)
        {
            _plugin = plugin;
            _previousCalculations = new List<MarkCalculation>();
        }

        public void Calculate(State state)
        {
            if(state.Location!=null && state.TargetMark!=null && state.TargetMark.Location!=null)
            {
                double meters = CoordinatePoint.HaversineDistance(state.Location, state.TargetMark.Location);
                if (meters < _distanceCutoff)//if the reported distance is more than this threshold, it's probably garbage data
                {
                    state.DistanceToTargetMarkInYards = meters * MetersToYards;
                }
                else
                {
                    state.DistanceToTargetMarkInYards = null;
                }

                var calculation = new MarkCalculation();
                calculation.Location = state.Location;
                calculation.Time = state.BestTime;

                _previousCalculations.Add(calculation);
                while(_previousCalculations.Count>_previousCalculationCount)
                {
                    _previousCalculations.RemoveAt(0);
                }

                if (_previousCalculations.Count > 1)
                {
                    var previous = _previousCalculations[_previousCalculations.Count - 2];
                    var duration = calculation.Time - previous.Time;
                    
                    //calculate vmc
                    var previousDistanceMeters = CoordinatePoint.HaversineDistance(previous.Location,
                        state.TargetMark.Location);
                    var distanceDelta = previousDistanceMeters - meters;
                    var vmcMetersPerSecond = distanceDelta/duration.TotalSeconds;
                    var vmcKnots = MetersPerSecondToKnots * vmcMetersPerSecond;
                    calculation.VelocityMadeGoodOnCourse = vmcKnots;
                    state.VelocityMadeGoodOnCourse = _previousCalculations.Average(x => x.VelocityMadeGoodOnCourse);

                    //TODO: calculate vmg
                    if (state.PreviousMark != null && state.SpeedInKnots.HasValue)
                    {
                        calculation.VelocityMadeGood = VelocityMadeGood(state.TargetMark, state.PreviousMark,
                            calculation.Location, previous.Location, state.SpeedInKnots.Value);
                    }
                }

            }
            else
            {
                state.DistanceToTargetMarkInYards = null;
                state.VelocityMadeGoodOnCourse = null;
                state.VelocityMadeGood = null;
            }
        }

        private double VelocityMadeGood(Mark targetMark, Mark previousMark, CoordinatePoint current, CoordinatePoint previous,double speed)
        {
            return Math.Cos(Math.Abs(RelativeAngleToCourse(targetMark,previousMark,current,previous))) * speed;
        }

        private double RelativeAngleToCourse(Mark targetMark, Mark previousMark, CoordinatePoint current, CoordinatePoint previous)
        {
            if (previousMark != null && targetMark != null)
            {
                float courseAngle = (float)AngleUtilities.FindAngle(targetMark.Location.Project(), previousMark.Location.Project());
                float boatAngle = (float)AngleUtilities.FindAngle(previous.Project(), current.Project()); ;
                return AngleUtilities.AngleDifference(courseAngle, boatAngle);
            }
            else
            {
                return 0;
            }
        }

        public IPlugin Plugin
        {
            get { return _plugin; }
        }

        public void Dispose()
        {
            
        }
    }
}
