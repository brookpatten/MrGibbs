using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MrGibbs.Contracts;
using MrGibbs.Contracts.Infrastructure;
using QuadroschrauberSharp.Hardware;

namespace MrGibbs.MPU6050
{
    public class Mpu6050Plugin:IPlugin
    {
        private bool _initialized = false;
        private ILogger _logger;
        private IList<IPluginComponent> _components;
		private I2C _i2c;
        
        public Mpu6050Plugin(ILogger logger,I2C i2c)
        {
            _logger = logger;
			_i2c = i2c;
        }

        public void Initialize(PluginConfiguration configuration, Action<Action<ISystemController, IRaceController>> queueCommand)
        {
            _components = new List<IPluginComponent>();
            _initialized = false;
            var sensor = new Mpu6050Sensor(_i2c,_logger,this,true);
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
