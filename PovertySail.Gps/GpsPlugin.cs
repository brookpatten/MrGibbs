using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PovertySail.Configuration;
using PovertySail.Contracts;
using PovertySail.Contracts.Infrastructure;

namespace PovertySail.Gps
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

        public void Initialize(PluginConfiguration configuration)
        {
            _components = new List<IPluginComponent>();
            _initialized = false;
            var sensor = new GpsSensor(_logger, this,AppConfig.GpsPort);
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
