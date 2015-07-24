using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PovertySail.Configuration
{
    public static class AppConfig
    {
        public static string DatabaseConnectionString
        {
            get { return ConfigurationHelper.ReadStringAppSetting("Database", @"URI=file:povertysail.db"); }
        }
        public static int SleepTime
        {
            get { return ConfigurationHelper.ReadIntAppSetting("SleepTime", 1000); }
        }
        public static string GpsPort
        {
            get { return ConfigurationHelper.ReadStringAppSetting("GpsPort", @"/dev/ttyUSB0"); }
        }
        public static int GpsBaud
        {
            get { return ConfigurationHelper.ReadIntAppSetting("GpsBaud", 9600); }
        }
    }
}
