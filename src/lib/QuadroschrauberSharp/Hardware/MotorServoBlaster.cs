using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuadroschrauberSharp.Hardware
{
    public static class ServoBlaster
    {
        static FileStream servoblaster;

        static ServoBlaster()
        {
            servoblaster = new FileStream("/dev/servoblaster", FileMode.Open);
        }

        public static void Set(int index, int value)
        {
            if (index < 0 || index > 7)
                throw new Exception("BUG");
            if (value < 0 || value > 499)
                throw new Exception("BUG");

            byte[] buffer = Encoding.ASCII.GetBytes(string.Format("{0}={1}\n", index, value));
            servoblaster.Write(buffer, 0, buffer.Length);
            servoblaster.Flush();
        }
    }

    public class MotorServoBlaster : Motor
    {
        int index;

        public MotorServoBlaster(int index)
        {
            this.index = index;
        }

        public void SetPWM(int microseconds)
        {
            ServoBlaster.Set(index, Math.Min(microseconds / 5, 499));
        }

        public override void SetMilli(int permille)
        {
            if (permille < 0)
                permille = 0;
            else if (permille > 1000)
                permille = 1000;
            SetPWM(1000 + permille);
        }

        public override void Set(float power)
        {
            if (power < 0)
                power = 0;
            else if (power > 1)
                power = 1;
            SetPWM(1000 + (int)(power * 1000));
        }
    }
}
