using System;

using PovertySail.Contracts;
using PovertySail.Models;
using PovertySail.Contracts.Infrastructure;

using QuadroschrauberSharp;
using QuadroschrauberSharp.Hardware;

namespace PovertySail.HMC5883
{
	public class Hmc5883Sensor:ISensor
	{
		private ILogger _logger;
		private Hmc5883Plugin _plugin;

		private I2C _i2c;

		private DateTime? _lastTime;

		public Hmc5883Sensor(ILogger logger, Hmc5883Plugin plugin)
		{
			_logger = logger;
			_plugin = plugin;

			//original pi is 0, pi rev 2 is 1
			//this probably DOES need to be configurable
			_i2c = new I2C(1);


			//the i2c address is 0x1e
		}

		public void Update(State state)
		{
			if (_lastTime != null) {
				var difference = state.Time - _lastTime.Value;

				float dtime = (float)difference.TotalMilliseconds / 1000000.0f;


			}

			_lastTime = state.Time;
		}

		public IPlugin Plugin
		{
			get { return _plugin; }
		}

		public void Dispose()
		{
			_i2c.Close();
		}
	}
}

