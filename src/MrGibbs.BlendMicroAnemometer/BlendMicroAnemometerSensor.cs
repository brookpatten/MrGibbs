using System;
using MrGibbs.Contracts;
using MrGibbs.Contracts.Infrastructure;

namespace MrGibbs.BlendMicroAnemometer
{
    /// <summary>
    /// sensor implementation of the blend micro wind sensor
    /// </summary>
    public class BlendMicroAnemometerSensor:ISensor
    {
        private ILogger _logger;
        private BlendMicroAnemometerPlugin _plugin;

        public BlendMicroAnemometerSensor(ILogger logger, BlendMicroAnemometerPlugin plugin)
        {
            _plugin = plugin;
            _logger = logger;
        }

        /// <inheritdoc />
        public IPlugin Plugin
        {
            get { return _plugin; }
        }

        /// <inheritdoc />
        public void Start()
        {
        }

        /// <inheritdoc />
        public void Update(Models.State state)
        {
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }

        /// <inheritdoc />
        public void Calibrate()
        {
        }
    }
}
