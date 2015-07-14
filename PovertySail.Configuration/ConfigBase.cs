using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PovertySail.Configuration
{
    internal static class ConfigurationHelper
    {
        internal static bool ReadBoolAppSetting(string key, bool defaultValue)
        {
            bool val;
            if (bool.TryParse(ConfigurationManager.AppSettings[key], out val))
            {
                return val;
            }
            else
            {
                return defaultValue;
            }
        }

        internal static int ReadIntAppSetting(string key, int defaultValue)
        {
            int val;
            if (int.TryParse(ConfigurationManager.AppSettings[key], out val))
            {
                return val;
            }
            else
            {
                return defaultValue;
            }
        }

        internal static string ReadStringAppSetting(string key, string defaultValue)
        {
            string configuredValue = ConfigurationManager.AppSettings[key];
            if (!string.IsNullOrEmpty(configuredValue))
            {
                return configuredValue;
            }
            else
            {
                return defaultValue;
            }
        }
    }
}
