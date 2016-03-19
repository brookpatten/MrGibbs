using System;
using System.Collections.Generic;

using MrGibbs.Contracts;
using MrGibbs.Contracts.Infrastructure;

namespace MrGibbs.MPU6050
{
    /// <summary>
    /// plugin to read accel/gyro data from an mpu6050 chip via i2c
    /// </summary>
    public class Mpu6050Plugin:IPlugin
    {
        private bool _initialized = false;
        private ILogger _logger;
        private IList<IPluginComponent> _components;
		private Mpu6050 _mpu;
		private Imu6050 _imu;

		public Mpu6050Plugin(ILogger logger,Mpu6050 mpu, Imu6050 imu)
        {
            _logger = logger;
			_mpu = mpu;
			_imu = imu;
        }

        /// <inheritdoc />
        public void Initialize(PluginConfiguration configuration, Action<Action<ISystemController, IRaceController>> queueCommand)
        {
            _components = new List<IPluginComponent>();
            _initialized = false;
			var sensor = new Mpu6050Sensor(_mpu,_imu,_logger,this,true);
            configuration.Sensors.Add(sensor);
            _components.Add(sensor);
            _initialized = true;
        }

        /// <inheritdoc />
        public bool Initialized
        {
            get { return _initialized; }
        }

        /// <inheritdoc />
        public IList<IPluginComponent> Components
        {
            get { return _components; }
        }

        /// <inheritdoc />
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
