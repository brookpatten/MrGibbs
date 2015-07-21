using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PovertySail.Contracts;
using PovertySail.Models;
using PovertySail.Contracts.Infrastructure;

using QuadroschrauberSharp;
using QuadroschrauberSharp.Hardware;

namespace PovertySail.MPU6050
{
    public class Mpu6050Sensor:ISensor
    {
        private ILogger _logger;
        private Mpu6050Plugin _plugin;

        public I2C i2c;
        public QuadroschrauberSharp.Hardware.MPU6050 _mpu;
        public QuadroschrauberSharp.IMU_MPU6050 _imu;

        public Mpu6050Sensor(ILogger logger, Mpu6050Plugin plugin)
        {
            _logger = logger;
            _plugin = plugin;

            i2c = new I2C(1);
            _mpu = new QuadroschrauberSharp.Hardware.MPU6050(i2c, 0x69);
            _imu = new IMU_MPU6050(_mpu);
            
            _imu.Init(false);
            _logger.Info("Calibrating MPU-6050");
            _imu.Calibrate();
        }

        public void Update(State state)
        {
            var accel= _imu.GetAccel();
            var gyro = _imu.GetGyro();

            _logger.Info("MPU-6050: Acceleration(" + string.Format("{0:0.00}", accel.x) + "," + string.Format("{0:0.00}", accel.y) + "," + string.Format("{0:0.00}", accel.z) + ") Gyro(" + string.Format("{0:0.00}", gyro.x) + "," + string.Format("{0:0.00}", gyro.y) + "," + string.Format("{0:0.00}", gyro.z) + ")");

        }

        public IPlugin Plugin
        {
            get { return _plugin; }
        }

        public void Dispose()
        {
            i2c.Close();
        }
    }
}
