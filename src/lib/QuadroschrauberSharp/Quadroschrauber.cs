using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using QuadroschrauberSharp.Hardware;

namespace QuadroschrauberSharp
{
    public class Quadroschrauber
    {
        public static Quadroschrauber Instance;

        public Motor MotorFront;
        public Motor MotorBack;
        public Motor MotorLeft;
        public Motor MotorRight;

        public I2C I2C;
        public MPU6050 mpu;
        public IMU_MPU6050 imu;
        public Controller Controller = new Controller();

        public Spektrum Remote;
        public Mbed Mbed;

        public Quadroschrauber()
        {
            Instance = this;
        }

        public void Init()
        {
            if (false)
            {
                I2C = new I2C(1);

                MotorFront = new MotorServoBlaster(0);
                MotorBack = new MotorServoBlaster(3);
                MotorLeft = new MotorServoBlaster(1);
                MotorRight = new MotorServoBlaster(2);

                Remote = new Spektrum();
                Mbed = new Hardware.Mbed();

                mpu = new MPU6050(I2C, 0x69,null);
                imu = new IMU_MPU6050(mpu,null);
                imu.Init(false);
            }

            try
            {
                //ControllerConfig.Load("quadroschrauber_controller.json").Set(this);
            }
            catch
            {
            }
        }
        

        public int Hz;
        int minframetime = int.MaxValue;
        int maxframetime = int.MinValue;
        bool exit;
        public void Run()
        {
            int lastticks = Environment.TickCount;
            int frames = 0;
            int lastframes = lastticks;
            while (!exit)
            {
                int ticks = Environment.TickCount;
                int delta = ticks - lastticks;
                if (delta > 0)
                {
                    minframetime = Math.Min(minframetime, delta);
                    maxframetime = Math.Max(maxframetime, delta);
                    Tick(delta * 1000);
                    frames++;
                    if (ticks - lastframes > 1000)
                    {
                        lastframes = ticks;
                        Console.WriteLine("FPS: " + frames);
                        Hz = frames;
                        frames = 0;
                    }
                }
                lastticks = ticks;
                //GC.Collect();
                Thread.Sleep(2);
            }

            Console.WriteLine("exit");
        }

        public float GetSystemLoad()
        {
            const string loadavg = "/proc/loadavg";
            if (!File.Exists(loadavg))
                return -1;

            using (StreamReader rs = new StreamReader(loadavg))
            {
                string s = rs.ReadToEnd();
                return float.Parse(s.Split(' ')[0]);
            }
        }


        //public Queue<Telemetry> SensorQueue = new Queue<Telemetry>(101);
        int queuecounter = 0;
        long framecounter = 0;

        public void Tick(int microseconds)
        {
            float dtime = (float)microseconds / 1000000.0f;
            if(imu != null)
                imu.Update(dtime);
            GetSensorData(dtime, SensorInput);
            if (framecounter++ == 100 && imu != null)
                imu.Calibrate();


            if (Remote != null && Remote.Input.active)
            {
                // RC has higher priority than web-interface
                Controller.Update(dtime, Remote.Input, SensorInput, MotorOutput);
            }
            else
            {
                //Controller.Update(dtime, control, SensorInput, MotorOutput);
            }
            SetMotors(MotorOutput);

            queuecounter += microseconds;



            if (queuecounter >= 50000)
            {
                queuecounter %= 50000;


                //var t = new Telemetry()
                //{
                //    Ticks = Environment.TickCount,
                //    AccelX = SensorInput.accel.x,
                //    AccelY = SensorInput.accel.y,
                //    AccelZ = SensorInput.accel.z,
                //    GyroX = SensorInput.gyro.x,
                //    GyroY = SensorInput.gyro.y,
                //    GyroZ = SensorInput.gyro.z,
                //    Hz = Hz,
                //    MotorFront = MotorOutput.motor_front,
                //    MotorBack = MotorOutput.motor_back,
                //    MotorLeft = MotorOutput.motor_left,
                //    MotorRight = MotorOutput.motor_right,
                //    Load = GetSystemLoad(),
                //    RemoteActive = Remote != null ? Remote.Input.active : false,
                //    RemotePitch = Remote != null ? Remote.Input.pitch : 0,
                //    RemoteRoll = Remote != null ? Remote.Input.roll : 0,
                //    RemoteThrottle = Remote != null ? Remote.Input.throttle : 0,
                //    RemoteYaw = Remote != null ? Remote.Input.yaw : 0,
                //    MinFrameTime = minframetime,
                //    MaxFrameTime = maxframetime,
                //    GC0 = GC.CollectionCount(0),
                //    GC1 = GC.CollectionCount(1)
                //};
                minframetime = int.MaxValue;
                maxframetime = int.MinValue;

                //var sessions = service.WebSocket.GetAllSessions();
                //if (sessions.Any())
                //{
                //    string data = service.WebSocket.JsonSerialize(t);

                //    foreach (var s in sessions)
                //    {
                //        s.Send(data);
                //        string mbed = Mbed.ReceivedJsonString;
                //        if (mbed != null)
                //        {
                //            s.Send(mbed);
                //        }
                //    }
                //}


                //lock (SensorQueue)
                //{
                //    SensorQueue.Enqueue(t);

                //    if (SensorQueue.Count > 101)
                //        SensorQueue.Dequeue();
                //}
            }
        }

        //ControlRequest control = new ControlRequest();
        MotorOutput MotorOutput = new MotorOutput();
        SensorInput SensorInput = new SensorInput();

        void SetMotors(MotorOutput output)
        {
            if (MotorBack != null)
            {
                MotorBack.Set(MotorOutput.motor_back);
                MotorFront.Set(MotorOutput.motor_front);
                MotorLeft.Set(MotorOutput.motor_left);
                MotorRight.Set(MotorOutput.motor_right);
            }
        }

        void GetSensorData(float dtime, SensorInput output)
        {
            if (imu != null)
            {
                output.accel = imu.GetAccel();
                output.gyro = imu.GetGyro();
            }
        }

        //public void Control(ControlRequest request)
        //{
        //    request.active = true;
        //    control = request;
        //}

        public void Calibrate()
        {
            imu.Calibrate();
        }

        public void Shutdown()
        {
            exit = true;
        }
    }
}
