using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MrGibbs.Configuration
{
    public static class ConfigurationHelper
    {
		public class ConfigurationSetting
		{
			public string Key{get;set;}
			public Type Type{get;set;}
			public Object DefaultValue{ get; set; }
			public Object ConfiguredValue{get;set;}
		}

		public static IList<ConfigurationSetting> Settings{ get; private set; }

		private static T RecordAndReturnSetting<T> (string key, T defaultValue, T configuredValue)
		{
			if (Settings == null)
			{
				Settings = new List<ConfigurationSetting> ();
			}
			var existing = Settings.SingleOrDefault (x => x.Key == key);
			if (existing == null)
			{
				var setting = new ConfigurationSetting () {
					Key = key,
					Type = typeof(T),
					DefaultValue = defaultValue,
					ConfiguredValue = configuredValue
				};
				Settings.Add (setting);
			}

			return configuredValue;
		}

		public static bool ReadBoolAppSetting(string key, bool defaultValue)
        {
            bool val;
            if (bool.TryParse(ConfigurationManager.AppSettings[key], out val))
            {
				return RecordAndReturnSetting(key,defaultValue,val);
            }
            else
            {
				return RecordAndReturnSetting(key,defaultValue,defaultValue);
            }
        }

		public static int ReadIntAppSetting(string key, int defaultValue)
        {
            int val;
            if (int.TryParse(ConfigurationManager.AppSettings[key], out val))
            {
				return RecordAndReturnSetting(key,defaultValue,val);
            }
            else
            {
				return RecordAndReturnSetting(key,defaultValue,defaultValue);
            }
        }

		public static double ReadDoubleAppSetting(string key, int defaultValue)
        {
            double val;
            if (double.TryParse(ConfigurationManager.AppSettings[key], out val))
            {
				return RecordAndReturnSetting(key,defaultValue,val);
            }
            else
            {
				return RecordAndReturnSetting(key,defaultValue,defaultValue);
            }
        }

		public static string ReadStringAppSetting(string key, string defaultValue)
        {
            string configuredValue = ConfigurationManager.AppSettings[key];
            if (!string.IsNullOrEmpty(configuredValue))
            {
				return RecordAndReturnSetting(key,defaultValue,configuredValue);
            }
            else
            {
				return RecordAndReturnSetting(key,defaultValue,defaultValue);
            }
        }
    }
}
