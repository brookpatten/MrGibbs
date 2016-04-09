using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MrGibbs.Contracts.Infrastructure;

namespace MrGibbs.MPU6050
{
    public class Imu6050
    {
        VectorFloat accel_calibration = new VectorFloat();
        VectorFloat gyro_calibration = new VectorFloat();

        bool dmpReady = false; // set true if DMP init was successful
        byte mpuIntStatus; // holds actual interrupt status byte from MPU
        byte devStatus; // return status after each device operation (0 = success, !0 = error)
        ushort packetSize; // expected DMP packet size (default is 42 bytes)
        ushort fifoCount; // count of all bytes currently in FIFO
        byte[] fifoBuffer = new byte[64]; // FIFO storage buffer

        private ILogger _logger;

        Mpu6050 mpu;
		public Imu6050(Mpu6050 mpu,ILogger logger)
        {
            _logger = logger;
            this.mpu = mpu;
        }

        bool use_dmp;

        public void Init(bool dmp)
        {
            use_dmp = dmp;
			_logger.Debug("Initializing MPU-6050");
            mpu.Initialize();


            // verify connection
            if (mpu.TestConnection())
            {
				_logger.Debug("MPU6050 connection successful");
            }
            else
            {
				_logger.Warn("MPU6050 connection failed");
            }



            if (dmp)
            {
                // load and configure the DMP
                devStatus = mpu.DmpInitialize();
                // make sure it worked (returns 0 if so)
                if (devStatus == 0)
                {
                    // turn on the DMP, now that it's ready
                    _logger.Debug("DMP Initialized");
                    mpu.SetDMPEnabled(true);
					_logger.Debug("DMP Enabled");

                    // enable Arduino interrupt detection
                    //Serial.println(F("Enabling interrupt detection (Arduino external interrupt 0)..."));
                    //attachInterrupt(0, dmpDataReady, RISING);
                    mpuIntStatus = mpu.GetIntStatus();

                    // set our DMP Ready flag so the main loop() function knows it's okay to use it
					_logger.Debug("DMP Ready");
                    dmpReady = true;

                    // get expected DMP packet size for later comparison
                    packetSize = mpu.DmpGetFIFOPacketSize();
                }
                else
                {
                    // ERROR!
                    // 1 = initial memory load failed
                    // 2 = DMP configuration updates failed
                    // (if it's going to break, usually the code will be 1)
                    _logger.Error(string.Format("DMP Initialization failed (code {0})", devStatus));
                    throw new Exception("Failed to initialize DMP");
                }
            }

			_logger.Debug("Full Accel Range: " + mpu.GetFullScaleAccelRange());
			_logger.Debug("Full Gyro Range: " + mpu.GetFullScaleGyroRange());
        }

        public void Calibrate()
        {
            gyro_calibration = GetUncalibratedGyro() * -1;
            accel_calibration = new VectorFloat(0, 0, 1) - GetUncalibratedAccel();
        }

        Mpu6050.Motion6 motion;
        Quaternion q;
        VectorFloat gravity;
        float[] ypr = new float[3];

        public void Update(float dtime)
        {
            motion = mpu.GetMotion6();
            // these methods (and a few others) are also available
            //accelgyro.getAcceleration(&ax, &ay, &az);
            //accelgyro.getRotation(&gx, &gy, &gz);

            // display accel/gyro x/y/z values
            //Console.WriteLine("a/g: {0} {1} {2} {3} {4} {5}", m.ax, m.ay, m.az, m.gx, m.gy, m.gz);


            if (use_dmp && dmpReady)
            {
                ushort fifoCount = mpu.GetFIFOCount();

                if (fifoCount == 1024)
                {
                    // reset so we can continue cleanly
                    mpu.ResetFIFO();
                    _logger.Debug("MPU-6050 IMU FIFO overflow");

                    // otherwise, check for DMP data ready interrupt (this should happen frequently)
                }
                else if (fifoCount >= 42)
                {
                    // read a packet from FIFO
                    mpu.GetFIFOBytes(fifoBuffer, (byte)packetSize);

                    q = mpu.DmpGetQuaternion(fifoBuffer);
                    gravity = mpu.DmpGetGravity(q);

                    mpu.DmpGetYawPitchRoll(ypr, q, gravity);
                    //Console.WriteLine("quat {0} {1} {2} {3} ", q.w, q.x, q.y, q.z);
                    //Console.WriteLine("ypr {0} {1} {2} ", ypr[0] * 180 / Math.PI, ypr[1] * 180 / Math.PI, ypr[2] * 180 / Math.PI);


                }
            }
        }

        VectorFloat GetUncalibratedAccel()
        {
            const float accel_factor = 1.0f / 16384.0f;
            return new VectorFloat(motion.ax, motion.ay, -motion.az) * accel_factor;
        }

        public VectorFloat GetAccel()
        {
            return GetUncalibratedAccel() + accel_calibration;
        }

        VectorFloat GetUncalibratedGyro()
        {
            const float gyro_factor = -(1.0f / 32768.0f * 2000.0f / 180.0f * (float)Math.PI);
            return new VectorFloat(motion.gx, motion.gy, -motion.gz) * gyro_factor;
        }

        public VectorFloat GetGyro()
        {
            return GetUncalibratedGyro() + gyro_calibration;
        }

        public Quaternion GetQuaternion()
        {
            if (!(use_dmp && dmpReady))
                throw new Exception("dmp not ready!");
            //TODO: fix orientation
            return q;
        }

        public VectorFloat GetRollYawPitch()
        {
            if (!(use_dmp && dmpReady))
                throw new Exception("dmp not ready!");
            //TODO: fix orientation
            return new VectorFloat(ypr[2], ypr[0], ypr[1]);
        }
    }
}
