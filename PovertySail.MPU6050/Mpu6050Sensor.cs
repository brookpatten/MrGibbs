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

        private I2C _i2c;
        private QuadroschrauberSharp.Hardware.MPU6050 _mpu;
        private IMU_MPU6050 _imu;

		private DateTime? _lastTime;
        private bool _enableDmp;

        public Mpu6050Sensor(ILogger logger, Mpu6050Plugin plugin, bool dmp)
        {
            _enableDmp = dmp;
            _logger = logger;
            _plugin = plugin;

			//original pi is 0, pi rev 2 is 1
            //this probably DOES need to be configurable
			_i2c = new I2C(1);


			//address is dependent upon the voltage to the ADO pin
			//low=0x68 for the raw data
			//hi=0x69 for the vologic
			//this probably does NOT need to be configurable since it won't change
			_mpu = new QuadroschrauberSharp.Hardware.MPU6050(_i2c, 0x69,_logger);
            _imu = new IMU_MPU6050(_mpu,_logger);

            Calibrate();
        }

        public void Update(State state)
        {
			if (_lastTime != null) {
				var difference = state.BestTime - _lastTime.Value;

				float dtime = (float)difference.TotalMilliseconds / 1000000.0f;
				_imu.Update (dtime);

                var accel = _imu.GetAccel ();
				var gyro = _imu.GetGyro ();

                //these probably need to be normalized to some known scale
			    state.Accel = new Vector3() {X = accel.x, Y = accel.y, Z = accel.z};
			    state.Gyro = new Vector3() {X = gyro.x, Y = gyro.y, Z = gyro.z};

			    //var rpy = _imu.GetRollYawPitch ();

			    _logger.Debug ("MPU-6050: Acceleration(" + string.Format ("{0:0.00}", accel.x) + "," + string.Format ("{0:0.00}", accel.y) + "," + string.Format ("{0:0.00}", accel.z) + ") Gyro(" + string.Format ("{0:0.00}", gyro.x) + "," + string.Format ("{0:0.00}", gyro.y) + "," + string.Format ("{0:0.00}", gyro.z) + ")");
			    //_logger.Debug ("MPU-6050: Roll/Pitch/Yaw(" + string.Format ("{0:0.00}", rpy.x*360.0) + "," + string.Format ("{0:0.00}", gyro.y*360.0) + "," + string.Format ("{0:0.00}", gyro.z*360.0) + ")");


			    //_logger.Info ("Heel:" + (accel.x * 360.0)); 
			    state.Heel = accel.x*(360.0/4.0);//((double)accel.x).ToDegrees();
                state.Pitch = accel.y * (360.0 / 4.0);//((double)accel.y).ToDegrees();

			    //if (framecounter++ == 100 && imu != null)
			    //_imu.Calibrate ();

			}

            _lastTime = state.BestTime;
        }

        public IPlugin Plugin
        {
            get { return _plugin; }
        }

        public void Dispose()
        {
			_i2c.Close();
        }


        public void Calibrate()
        {
            _imu.Init(_enableDmp);
            _logger.Info("Calibrating MPU-6050");
            _imu.Calibrate();
        }
    }
}
