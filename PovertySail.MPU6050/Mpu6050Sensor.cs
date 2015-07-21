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

        public I2C I2C;
        public QuadroschrauberSharp.Hardware.MPU6050 mpu;
        public QuadroschrauberSharp.IMU_MPU6050 imu;

        public Mpu6050Sensor(ILogger logger, Mpu6050Plugin plugin)
        {
            _logger = logger;
            _plugin = plugin;

            I2C = new I2C(1);
            mpu = new QuadroschrauberSharp.Hardware.MPU6050(I2C, 0x69);
            imu = new IMU_MPU6050(mpu);
            
            imu.Init(false);
            _logger.Info("Calibrating MPU-6050");
            imu.Calibrate();
        }

        public void Update(State state)
        {
            var accel= imu.GetAccel();
            var gyro = imu.GetGyro();

            _logger.Info("MPU-6050: Acceleration(" + string.Format("{0:0.00}", accel.x) + "," + string.Format("{0:0.00}", accel.y) + "," + string.Format("{0:0.00}", accel.z) + ") Gyro(" + string.Format("{0:0.00}", gyro.x) + "," + string.Format("{0:0.00}", gyro.y) + "," + string.Format("{0:0.00}", gyro.z) + ")");

        }

        public IPlugin Plugin
        {
            get { return _plugin; }
        }

        public void Dispose()
        {
            I2C.Close();
        }
    }
}
