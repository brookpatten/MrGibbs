using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Ports;
//using ServiceStack.Text;

namespace QuadroschrauberSharp.Hardware
{
    public class Mbed
    {
        SerialPort port;
        Task task;

        bool Open(string portname)
        {
            port = new SerialPort(portname);
            try
            {
                port.Open();
            }
            catch
            {
                port = null;
                return false;
            }

            return true;
        }

        public Mbed()
        {
            return;
            if (!Open("/dev/ttyACM0"))
            {
                if (!Open("/dev/ttyACM1"))
                {
                    return;
                }
            }
            task = Task.Factory.StartNew(ReadTask, TaskCreationOptions.LongRunning);
        }

        void ReadTask()
        {
            string line;
            while ((line = port.ReadLine()) != null)
            {
                //JsonObject json = JsonObject.Parse(line);
                /*foreach (var x in json)
                {
                    Console.WriteLine(x.Key + ": " + x.Value);
                }*/
                Received = null;
                ReceivedJsonString = line;
            }
        }

        public object Received;
        public string ReceivedJsonString;
    }
}
