using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuadroschrauberSharp
{
    public class IMU_MPU6050
    {
        VectorFloat accel_calibration = new VectorFloat();
        VectorFloat gyro_calibration = new VectorFloat();

        bool dmpReady = false; // set true if DMP init was successful
        byte mpuIntStatus; // holds actual interrupt status byte from MPU
        byte devStatus; // return status after each device operation (0 = success, !0 = error)
        ushort packetSize; // expected DMP packet size (default is 42 bytes)
        ushort fifoCount; // count of all bytes currently in FIFO
        byte[] fifoBuffer = new byte[64]; // FIFO storage buffer

        Hardware.MPU6050 mpu;
        public IMU_MPU6050(Hardware.MPU6050 mpu)
        {
            this.mpu = mpu;
        }

        bool use_dmp;
        public void Init(bool dmp)
        {
            use_dmp = dmp;
            Console.WriteLine("Initializing I2C devices...\n");
            mpu.initialize();


            // verify connection
            Console.WriteLine("Testing device connections...\n");
            Console.WriteLine(mpu.testConnection() ? "MPU6050 connection successful\n" : "MPU6050 connection failed\n");


            if (true)
            {
                // load and configure the DMP
                Console.WriteLine("Initializing DMP...");
                devStatus = mpu.dmpInitialize();
                // make sure it worked (returns 0 if so)
                if (devStatus == 0)
                {
                    // turn on the DMP, now that it's ready
                    Console.WriteLine("Enabling DMP...");
                    mpu.setDMPEnabled(true);

                    // enable Arduino interrupt detection
                    //Serial.println(F("Enabling interrupt detection (Arduino external interrupt 0)..."));
                    //attachInterrupt(0, dmpDataReady, RISING);
                    mpuIntStatus = mpu.getIntStatus();

                    // set our DMP Ready flag so the main loop() function knows it's okay to use it
                    Console.WriteLine("DMP ready!");
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
                    Console.WriteLine("DMP Initialization failed (code %d)", devStatus);
                }
            }

            Console.WriteLine("Full Accel Range: " + mpu.getFullScaleAccelRange());
            Console.WriteLine("Full Gyro Range: " + mpu.getFullScaleGyroRange());
        }

        public void Calibrate()
        {
            gyro_calibration = GetUncalibratedGyro() * -1;
            accel_calibration = new VectorFloat(0, 0, 1) - GetUncalibratedAccel();
        }

        Hardware.MPU6050.Motion6 motion;
        Quaternion q;
        VectorFloat gravity;
        float[] ypr = new float[3];

        public void Update(float dtime)
        {
            motion = mpu.getMotion6();
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
                    Console.WriteLine("FIFO overflow!");

                    // otherwise, check for DMP data ready interrupt (this should happen frequently)
                }
                else if (fifoCount >= 42)
                {
                    // read a packet from FIFO
                    //Console.WriteLine("fifo read: " + packetSize + "/" + fifoCount);
                    mpu.getFIFOBytes(fifoBuffer, (byte)packetSize);

                    q = mpu.dmpGetQuaternion(fifoBuffer);
                    gravity = mpu.dmpGetGravity(q);

                    mpu.dmpGetYawPitchRoll(ypr, q, gravity);
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
