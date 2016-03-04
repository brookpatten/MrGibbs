using System;

using Mono.BlueZ.DBus;

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
		private Device1 _device;
		private GattCharacteristic1 _characteristic;

		public BlendMicroAnemometerSensor(ILogger logger, BlendMicroAnemometerPlugin plugin,Device1 device)
        {
            _plugin = plugin;
            _logger = logger;
			_device = device;
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
