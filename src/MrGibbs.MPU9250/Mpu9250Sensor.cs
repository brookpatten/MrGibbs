using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MrGibbs.Contracts;
using MrGibbs.Models;
using MrGibbs.Contracts.Infrastructure;

using QuadroschrauberSharp;
using QuadroschrauberSharp.Hardware;

namespace MrGibbs.MPU9250
{
    public class Mpu9250Sensor:ISensor
    {
        private ILogger _logger;
        private Mpu9250Plugin _plugin;

        private I2C _i2c;
        private Mpu9250 _mpu;
        private ImuMpu9250 _imu;

		private DateTime? _lastTime;
        private bool _enableDmp;

        public Mpu9250Sensor(ILogger logger, Mpu9250Plugin plugin, bool dmp)
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
			_mpu = new Mpu9250(_i2c, 0x69,_logger);
            _imu = new ImuMpu9250(_mpu, _logger);

            Calibrate();
        }

        public void Update(State state)
        {
			if (_lastTime != null) {
				var difference = state.BestTime - _lastTime.Value;

				float dtime = (float)difference.TotalMilliseconds / 1000000.0f;
				_imu.Update (dtime);

                

                //these probably need to be normalized to some known scale
                state.Accel = _imu.GetAccel();
                state.Gyro = _imu.GetGyro();
                state.Magneto = _imu.GetMagneto();
                
			    //var rpy = _imu.GetRollYawPitch ();

                _logger.Debug("MPU-9250: Acceleration(" + string.Format("{0:0.00}", state.Accel.X) + "," + string.Format("{0:0.00}", state.Accel.Y) + "," + string.Format("{0:0.00}", state.Accel.Z) + ") Gyro(" + string.Format("{0:0.00}", state.Gyro.X) + "," + string.Format("{0:0.00}", state.Gyro.Y) + "," + string.Format("{0:0.00}", state.Gyro.Z) + ")");
			    //_logger.Debug ("MPU-9250: Roll/Pitch/Yaw(" + string.Format ("{0:0.00}", rpy.x*360.0) + "," + string.Format ("{0:0.00}", gyro.y*360.0) + "," + string.Format ("{0:0.00}", gyro.z*360.0) + ")");

                state.Heel = state.Accel.Y * (360.0 / 4.0);
                state.Pitch = state.Accel.X * (360.0 / 4.0);
                _logger.Info("Heel:" +state.Heel+", Pitch:"+state.Pitch);



                double heading = Math.Atan2(state.Magneto.X, state.Magneto.Y);
                if (heading < 0)
                {
                    heading += 2.0 * Math.PI;
                }

                //convert to degrees
                heading = heading * (180.0 / Math.PI);
                state.MagneticHeading = 360 - heading;
                
                
                //float N = 256;
                //state.MagneticHeading = state.Magneto.X * state.Magneto.X / N + state.Magneto.Y * state.Magneto.Y / N + state.Magneto.Z * state.Magneto.Z / N;
                
                _logger.Info("MPU-9250 Heading(" + state.Magneto.X + "," + state.Magneto.Y + "," + state.Magneto.Z + ") (" + state.MagneticHeading + ")");


			   

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
            _logger.Info("Calibrating MPU-9250");
            _imu.Calibrate();
        }
    }
}
