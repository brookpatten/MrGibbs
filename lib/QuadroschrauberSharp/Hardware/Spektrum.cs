using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuadroschrauberSharp.Hardware
{
    public class Spektrum
    {
        const int RC_THROTTLE_CHANNEL = 0;
        const int RC_PITCH_CHANNEL = 2;
        const int RC_ROLL_CHANNEL = 3;
        const int RC_YAW_CHANNEL = 1;
        const int RC_SWITCH1_CHANNEL = 4;
        const int RC_SWITCH2_CHANNEL = 5;
        const int RC_MAX = 1710;
        const int RC_MIN = 330;
        const int RC_MID = ((RC_MAX + RC_MIN) / 2);
        const int RC_THROTTLE_INVERT = 1;
        const int RC_PITCH_INVERT = -1;
        const int RC_ROLL_INVERT = -1;
        const int RC_YAW_INVERT = -1;

        SerialPort port;
        Task task;
        public Spektrum()
        {
            port = new SerialPort("/dev/ttyAMA0", 115200);
            port.ReadTimeout = 40;
            port.Open();
            task = Task.Factory.StartNew(ReadTask, TaskCreationOptions.LongRunning);
        }

        void ReadPacket(byte[] buffer)
        {
            int fill = 0;
            while (fill != 16)
            {
                fill += port.Read(buffer, fill, 16 - fill);
            }
        }

        void ReadTask()
        {
            while (true)
            {
                int count = port.BytesToRead;
                if (count <= 16)
                {
                    try
                    {
                        ReadPacket(buffer);
                        ParsePacket(buffer);
                    }
                    catch (TimeoutException)
                    {
                        Input.active = false;
                        Input.throttle = Input.yaw = Input.roll = Input.pitch = 0.0f;
                    }
                }
                else
                    port.ReadExisting();
            }
        }

        public RemoteInput Input = new RemoteInput();

        void ParsePacket(byte[] packet)
        {
            for (int i = 1; i < 8; ++i)
            {
                int tmp = (packet[i * 2] * 256 + packet[i * 2 + 1]);
                int channel = (tmp >> 11) & 7;
                if (channel < 6)
                    channels[channel] = tmp & ((1 << 11) - 1);
            }

            Input.pitch = (((float)channels[RC_PITCH_CHANNEL]) - RC_MID) / (RC_MAX - RC_MID) * RC_PITCH_INVERT;
            Input.roll = (((float)channels[RC_ROLL_CHANNEL]) - RC_MID) / (RC_MAX - RC_MID) * RC_ROLL_INVERT;
            Input.yaw = (((float)channels[RC_YAW_CHANNEL]) - RC_MID) / (RC_MAX - RC_MID) * RC_YAW_INVERT;
            Input.throttle = (((float)channels[RC_THROTTLE_CHANNEL]) - RC_MIN) / (RC_MAX - RC_MIN) * RC_THROTTLE_INVERT;

            if (Math.Abs(Input.pitch) <= 2 && Math.Abs(Input.roll) <= 2 && Math.Abs(Input.yaw) <= 2 && Math.Abs(Input.throttle) <= 2)
                Input.active = true;
            else
                Input.active = false;
        }

        int[] channels = new int[8];
        /*public float Throttle { get; private set; }
        public float Yaw { get; private set; }
        public float Roll { get; private set; }
        public float Pitch { get; private set; }
        public bool Active { get; private set; }*/


        byte[] buffer = new byte[1024];
    }
}
