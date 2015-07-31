using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PovertySail.Contracts.Infrastructure;
using PovertySail.Contracts;
using PovertySail.Models;

namespace PovertySail.Calculators
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

            }
            else
            {
                state.DistanceToTargetMarkInYards = null;
                state.VelocityMadeGoodOnCourse = null;
                state.VelocityMadeGood = null;
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
