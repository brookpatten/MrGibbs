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
    public class DistanceToMarkCalculator:ICalculator
    {
        private ILogger _logger;
        private IPlugin _plugin;
        private const double MetersToYards = 1.09361;

        public DistanceToMarkCalculator(ILogger logger, IPlugin plugin)
        {
            _plugin = plugin;
        }

        public void Calculate(State state)
        {
            if(state.Location!=null && state.TargetMark!=null && state.TargetMark.Location!=null)
            {
                state.DistanceToTargetMarkInYards = CoordinatePoint.HaversineDistance(state.Location, state.TargetMark.Location) * MetersToYards;

            }
            else
            {
                state.DistanceToTargetMarkInYards = null;
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
