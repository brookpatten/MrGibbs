using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MrGibbs.Contracts;
using MrGibbs.Contracts.Infrastructure;
using MrGibbs.Models;

namespace MrGibbs.ChaosMonkey
{
    public class ChaosCalculator : ICalculator
    {
        private ILogger _logger;
        private IPlugin _plugin;
        public ChaosCalculator(ILogger logger, IPlugin plugin)
        {
            _plugin = plugin;
            _logger = logger;
        }

        public void Calculate(State state)
        {
            var random = new Random();

            if(random.Next(3)==1)
            {
                state.AddMessage(MessageCategory.System, MessagePriority.Normal, 5, "Chaos Monkey "+DateTime.Now.Minute+":"+DateTime.Now.Second);
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
