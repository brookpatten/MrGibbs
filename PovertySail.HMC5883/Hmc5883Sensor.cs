using System;
using System.Runtime.InteropServices;
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
	    private Hmc5883 _hmc5883;

		private DateTime? _lastTime;

		public Hmc5883Sensor(ILogger logger, Hmc5883Plugin plugin)
		{
			_logger = logger;
			_plugin = plugin;

			//original pi is 0, pi rev 2 is 1
			//this probably DOES need to be configurable
			_hmc5883 = new Hmc5883(1);
            _hmc5883.Initialize();

		    if (!_hmc5883.TestConnection())
		    {
		        throw new ExternalException("Failed to connect to HMC5883L");
		    }
		}

		public void Update(State state)
		{
		    short x=0, y=0, z=0;
            _hmc5883.GetHeading(ref x,ref y,ref z);
            
            double heading = Math.Atan2(y,x);
		    if (heading < 0)
		    {
		        heading += 2*Math.PI;
		    }

            double finalHeading = heading * 180.0 / Math.PI;

            _logger.Info("HMC5883L Heading(" + x + "," + y + "," + z + ") ("+finalHeading+")");

		    state.MagneticHeading = finalHeading;
		}

		public IPlugin Plugin
		{
			get { return _plugin; }
		}

		public void Dispose()
		{
		    if (_hmc5883 != null)
		    {
                _hmc5883.Dispose();
		    }
		}
	}
}

