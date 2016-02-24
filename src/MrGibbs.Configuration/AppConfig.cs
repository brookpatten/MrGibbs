namespace MrGibbs.Configuration
{
    /// <summary>
    /// configuration values for the "core" of mr gibbs
    /// TODO: move plugin specific values to plugins
    /// </summary>
    public static class AppConfig
    {
        public static string DatabaseConnectionString
        {
            get { return ConfigurationHelper.ReadStringAppSetting("Database", @"URI=file:gibbs.db"); }
        }
        public static int SleepTime
        {
            get { return ConfigurationHelper.ReadIntAppSetting("SleepTime", 1000); }
        }
        public static string GpsPort
        {
            get { return ConfigurationHelper.ReadStringAppSetting("GpsPort", @"/dev/ttyAMA0"); }
        }
        public static int GpsBaud
        {
            get { return ConfigurationHelper.ReadIntAppSetting("GpsBaud", 9600); }
        }

        public static double AutoRoundMarkDistanceMeters
        {
            get { return ConfigurationHelper.ReadDoubleAppSetting("AutoRoundMarkDistanceMeters", 30); }
        }

		public static int I2CAddress
		{
			get { return ConfigurationHelper.ReadIntAppSetting("I2CAddress", 1); }
		}
    }
}
