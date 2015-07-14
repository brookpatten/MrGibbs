using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PovertySail.Contracts;
using PovertySail.Contracts.Infrastructure;

namespace PovertySail.Gps
{
    public class GpsSensor:ISensor
    {
        private ILogger _logger;
        private GpsPlugin _plugin;

        public GpsSensor(ILogger logger, GpsPlugin plugin)
        {
            _plugin = plugin;
            _logger = logger;
        }

        public IPlugin Plugin
        {
            get { return _plugin; }
        }

        /*using (SerialPort port = new SerialPort("/dev/ttyUSB0", 4800, Parity.None))
                    {
                        port.StopBits = StopBits.One;
                        port.DataBits = 8;
                        port.Open();

                        logger.WriteLine("Logging data...");
                        while(true)
                        {
                            var line = port.ReadLine();
                            command.CommandText = "insert into test(line) values ('" + line + "');";
                            command.ExecuteNonQuery();
                            Console.WriteLine(line);
                            logger.WriteLine(line);
                            logger.Flush();
                        }

                        port.Close();
                    }*/
    }
}
