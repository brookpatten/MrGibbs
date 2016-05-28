using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.IO;

namespace MrGibbs.Configuration
{
    /// <summary>
    /// configuration helper functions
    /// </summary>
    public static class ConfigurationHelper
    {
		public class ConfigurationSetting
		{
			public string Key{get;set;}
			public Type Type{get;set;}
			public Object DefaultValue{ get; set; }
			public Object ConfiguredValue{get;set;}
		}

		private static IList<ConfigurationSetting> Settings;

        /// <summary>
        /// record that the application requested a value, what value was defaulted, and what value was used
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <param name="configuredValue"></param>
        /// <returns></returns>
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

        /// <summary>
        /// generate a sample appSettings section based on what is currently configured
        /// </summary>
        /// <returns></returns>
		public static string GenerateDefaultConfiguration()
		{
			StringBuilder s = new StringBuilder ();
			s.Append ("<appSettings>");
			s.Append (Environment.NewLine);
			foreach(var setting in Settings)
			{
				s.Append("\t"+@"<add key="""+setting.Key+@""" value="""+setting.DefaultValue+@"""/>");
				s.Append(Environment.NewLine);
			}
			s.Append("</appSettings>");
			s.Append(Environment.NewLine);
			return s.ToString();
		}

        /// <summary>
        /// attempt to read a bool appsetting, if it doesn't exist or can't be read, return a default
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
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

        /// <summary>
        /// attempt to read a int appsetting, if it doesn't exist or can't be read, return a default
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
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

        /// <summary>
        /// attempt to read a double appsetting, if it doesn't exist or can't be read, return a default
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
		public static double ReadDoubleAppSetting(string key, double defaultValue)
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

        /// <summary>
        /// attempt to read a string appsetting, if it doesn't exist or can't be read, return a default
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
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

        /// <summary>
        /// find the newest file in the application folder with a given extension
        /// useful for loading the most recent version of a data file
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
		public static string FindNewestFileWithExtension(string extension)
		{
			var dir = new DirectoryInfo (GetExecutingAssemblyFolder());
			var matchingFiles = dir.GetFiles ("*."+extension);
			var newest = matchingFiles.OrderByDescending (x => x.CreationTimeUtc).FirstOrDefault();
			if(newest!=null)
			{
				return newest.FullName;
			}
			else
			{
				return null;
			}
		}

		public static string GetExecutingAssemblyFolder ()
		{
			string exePath = System.Reflection.Assembly.GetExecutingAssembly ().CodeBase;
			if (exePath.StartsWith ("file:"))
			{
				exePath = exePath.Substring (5);
			}
			string exeDir = Path.GetDirectoryName (exePath);
			return exeDir;
		}
    }
}
