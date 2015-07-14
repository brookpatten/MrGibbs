using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PovertySail.Contracts;
using PovertySail.Contracts.Infrastructure;
using PovertySail.Models;

namespace PovertySail.Calculators
{
    public class VmgCalculator:ICalculator
    {
        private ILogger _logger;
        private CalculatorPlugin _plugin;

        public VmgCalculator(ILogger logger, CalculatorPlugin plugin)
        {
            _plugin = plugin;
            _logger = logger;
        }

        public void Calculate(Dashboard dashboard)
        {
            
        }

        public IPlugin Plugin
        {
            get { return _plugin; }
        }
    }
}
