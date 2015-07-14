using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PovertySail.Contracts;
using PovertySail.Contracts.Infrastructure;

namespace PovertySail.I2CAccelerometer
{
    public class I2CAccelerometerSensor:ISensor
    {
        private ILogger _logger;
        private I2CAccelerometerPlugin _plugin;

        public I2CAccelerometerSensor(ILogger logger, I2CAccelerometerPlugin plugin)
        {
            _plugin = plugin;
            _logger = logger;
        }

        public IPlugin Plugin
        {
            get { return _plugin; }
        }
    }
}
