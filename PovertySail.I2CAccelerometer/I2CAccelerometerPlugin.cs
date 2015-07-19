using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PovertySail.Contracts;
using PovertySail.Contracts.Infrastructure;

namespace PovertySail.I2CAccelerometer
{
    public class I2CAccelerometerPlugin:IPlugin
    {
        private ILogger _logger;
        private bool _initialized = false;
        private IList<IPluginComponent> _components; 

        public I2CAccelerometerPlugin(ILogger logger)
        {
            _logger = logger;
            _components = new List<IPluginComponent>();
        }

        public void Initialize(PluginConfiguration configuration, EventHandler onWatchButton, EventHandler onHeadingButton, EventHandler onSpeedButton)
        {
            _initialized = false;
            configuration.Sensors.Add(new I2CAccelerometerSensor(_logger, this));
            _initialized = true;
        }

        public bool Initialized
        {
            get { return _initialized; }
        }


        public IList<IPluginComponent> Components
        {
            get { return _components; }
        }

        public void Dispose()
        {
            if (_components != null)
            {
                foreach (var component in _components)
                {
                    component.Dispose();
                }
            }
        }
    }
}
