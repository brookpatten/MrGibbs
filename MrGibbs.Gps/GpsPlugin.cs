using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MrGibbs.Configuration;
using MrGibbs.Contracts;
using MrGibbs.Contracts.Infrastructure;

namespace MrGibbs.Gps
{
    public class GpsPlugin:IPlugin
    {
        private ILogger _logger;
        private bool _initialized = false;
        private IList<IPluginComponent> _components; 

        public GpsPlugin(ILogger logger)
        {
            _logger = logger;
        }

        public void Initialize(PluginConfiguration configuration, Action<Action<ISystemController, IRaceController>> queueCommand)
        {
            _components = new List<IPluginComponent>();
            _initialized = false;
            //var sensor = new SimulatedGpsSensor(_logger, this);
            var sensor = new GpsSensor(_logger, this, AppConfig.GpsPort, AppConfig.GpsBaud);
            configuration.Sensors.Add(sensor);
            _components.Add(sensor);
            sensor.Start();
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
