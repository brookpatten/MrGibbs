using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PovertySail.Contracts;
using PovertySail.Contracts.Infrastructure;

namespace PovertySail.BlendMicroAnemometer
{
    public class BlendMicroAnemometerSensor:ISensor
    {
        private ILogger _logger;
        private BlendMicroAnemometerPlugin _plugin;

        public BlendMicroAnemometerSensor(ILogger logger, BlendMicroAnemometerPlugin plugin)
        {
            _plugin = plugin;
            _logger = logger;
        }

        public IPlugin Plugin
        {
            get { return _plugin; }
        }

        public void Start()
        {
        }

        public void Update(Models.State state)
        {
        }

        public void Dispose()
        {
        }
    }
}
