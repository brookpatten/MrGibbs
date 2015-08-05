using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PovertySail.Contracts;
using PovertySail.Contracts.Infrastructure;

namespace PovertySail.MPU9250
{
    public class Mpu9250Plugin:IPlugin
    {
        private bool _initialized = false;
        private ILogger _logger;
        private IList<IPluginComponent> _components;
        
        public Mpu9250Plugin(ILogger logger)
        {
            _logger = logger;
        }

        public void Initialize(PluginConfiguration configuration, Action<Action<ISystemController, IRaceController>> queueCommand)
        {
            _components = new List<IPluginComponent>();
            _initialized = false;
            var sensor = new Mpu9250Sensor(_logger,this,false);
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
