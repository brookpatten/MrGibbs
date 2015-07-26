using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PovertySail.Contracts;
using PovertySail.Contracts.Infrastructure;

namespace PovertySail.MPU6050
{
    public class Mpu6050Plugin:IPlugin
    {
        private bool _initialized = false;
        private ILogger _logger;
        private IList<IPluginComponent> _components;
        
        public Mpu6050Plugin(ILogger logger)
        {
            _logger = logger;
        }

        public void Initialize(PluginConfiguration configuration, ISystemController systemController, IRaceController raceController)
        {
            _components = new List<IPluginComponent>();
            _initialized = false;
            var sensor = new Mpu6050Sensor(_logger,this);
            configuration.Sensors.Add(sensor);
            _components.Add(sensor);
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
            _initialized = false;
            if(_components!=null)
            {
                foreach(var component in _components)
                {
                    component.Dispose();
                }
            }
        }
    }
}
