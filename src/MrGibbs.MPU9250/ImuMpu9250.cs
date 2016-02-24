using System;

using MrGibbs.Models;
using MrGibbs.Contracts.Infrastructure;
using QuadroschrauberSharp;

namespace MrGibbs.MPU9250
{
    /// <summary>
    /// IMU implementation for the mpu9259
    /// </summary>
    public class ImuMpu9250
    {

        Vector3 accel_calibration = new Vector3();
        Vector3 gyro_calibration = new Vector3();

        bool dmpReady = false; // set true if DMP init was successful
        byte mpuIntStatus; // holds actual interrupt status byte from MPU
        byte devStatus; // return status after each device operation (0 = success, !0 = error)
        ushort packetSize; // expected DMP packet size (default is 42 bytes)
        ushort fifoCount; // count of all bytes currently in FIFO
        byte[] fifoBuffer = new byte[64]; // FIFO storage buffer

        private ILogger _logger;

        Mpu9250 mpu;
        public ImuMpu9250(Mpu9250 mpu, ILogger logger)
        {
            _logger = logger;
            this.mpu = mpu;
        }

        bool use_dmp;

        public void Init(bool dmp)
        {
            use_dmp = dmp;
            _logger.Info("Initializing MPU-9250");
            mpu.initialize();


            // verify connection
            if (mpu.testConnection())
            {
                _logger.Info("MPU-9250 connection successful");
            }
            else
            {
                _logger.Info("MPU-9250 connection failed");
            }



            if (dmp)
            {
                // load and configure the DMP
                devStatus = mpu.dmpInitialize();
                // make sure it worked (returns 0 if so)
                if (devStatus == 0)
                {
                    // turn on the DMP, now that it's ready
                    _logger.Info("DMP Initialized");
                    mpu.setDMPEnabled(true);
                    _logger.Info("DMP Enabled");

                    // enable Arduino interrupt detection
                    //Serial.println(F("Enabling interrupt detection (Arduino external interrupt 0)..."));
                    //attachInterrupt(0, dmpDataReady, RISING);
                    mpuIntStatus = mpu.getIntStatus();

                    // set our DMP Ready flag so the main loop() function knows it's okay to use it
                    _logger.Info("DMP Ready");
                    dmpReady = true;

                    // get expected DMP packet size for later comparison
                    packetSize = mpu.dmpGetFIFOPacketSize();
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

            _logger.Info("Full Accel Range: " + mpu.getFullScaleAccelRange());
            _logger.Info("Full Gyro Range: " + mpu.getFullScaleGyroRange());
        }

        public void Calibrate()
        {
            gyro_calibration = GetUncalibratedGyro() * -1;
            accel_calibration = new Vector3(0, 0, 1) - GetUncalibratedAccel();
        }

        Mpu9250.Motion9 motion = new Mpu9250.Motion9();
        Quaternion q;
        Vector3 gravity;
        float[] ypr = new float[3];

        public void Update(float dtime)
        {
            motion = mpu.getMotion9();
            // these methods (and a few others) are also available
            //accelgyro.getAcceleration(&ax, &ay, &az);
            //accelgyro.getRotation(&gx, &gy, &gz);

            // display accel/gyro x/y/z values
            //Console.WriteLine("a/g: {0} {1} {2} {3} {4} {5}", m.ax, m.ay, m.az, m.gx, m.gy, m.gz);


            if (use_dmp && dmpReady)
            {
                ushort fifoCount = mpu.getFIFOCount();

                if (fifoCount == 1024)
                {
                    // reset so we can continue cleanly
                    mpu.resetFIFO();
                    _logger.Debug("MPU-9250 IMU FIFO overflow");

                    // otherwise, check for DMP data ready interrupt (this should happen frequently)
                }
                else if (fifoCount >= 42)
                {
                    // read a packet from FIFO
                    mpu.getFIFOBytes(fifoBuffer, (byte)packetSize);

                    q = mpu.dmpGetQuaternion(fifoBuffer);
                    gravity = mpu.dmpGetGravity(q);

                    mpu.dmpGetYawPitchRoll(ypr, q, gravity);
                    //Console.WriteLine("quat {0} {1} {2} {3} ", q.w, q.x, q.y, q.z);
                    //Console.WriteLine("ypr {0} {1} {2} ", ypr[0] * 180 / Math.PI, ypr[1] * 180 / Math.PI, ypr[2] * 180 / Math.PI);


                }
            }
        }

        Vector3 GetUncalibratedAccel()
        {
            const float accel_factor = 1.0f / 16384.0f;
            return new Vector3(motion.ax, motion.ay, -motion.az) * accel_factor;
        }

        public Vector3 GetAccel()
        {
            return GetUncalibratedAccel() + accel_calibration;
        }

        Vector3 GetUncalibratedGyro()
        {
            const float gyro_factor = -(1.0f / 32768.0f * 2000.0f / 180.0f * (float)Math.PI);
            return new Vector3(motion.gx, motion.gy, -motion.gz) * gyro_factor;
        }

        public Vector3 GetGyro()
        {
            return GetUncalibratedGyro() + gyro_calibration;
        }

        public Vector3 GetMagneto()
        {
            return new Vector3(motion.mx, motion.my, motion.mz);
        }

        public Quaternion GetQuaternion()
        {
            if (!(use_dmp && dmpReady))
                throw new Exception("dmp not ready!");
            //TODO: fix orientation
            return q;
        }

        public Vector3 GetRollYawPitch()
        {
            if (!(use_dmp && dmpReady))
                throw new Exception("dmp not ready!");
            //TODO: fix orientation
            return new Vector3(ypr[2], ypr[0], ypr[1]);
        }
    }
}
