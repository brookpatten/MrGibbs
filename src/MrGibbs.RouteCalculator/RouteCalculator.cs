using System;

using MrGibbs.Models;
using MrGibbs.Contracts;
using MrGibbs.Contracts.Infrastructure;

namespace MrGibbs.RouteCalculator
{
	public class RouteCalculator:ICalculator
	{
		private ILogger _logger;
		private IPlugin _plugin;


		public RouteCalculator(ILogger logger, IPlugin plugin)
		{
			_plugin = plugin;
			_logger = logger;
		}

		public IPlugin Plugin {
			get {
				return _plugin;
			}
		}

		public void Calculate (State state)
		{
			//inputs
			//	true wind or absolute wind
			//	course over ground
			//	speed
			//	current mark
			//	lat/lon

			//outputs
			//	favored tack
			//	distance to mark
			//	distance to tack
			//	eta to mark
			//	eta to tack

			//if there's wind data
				//find the angle to the wind
				//calculate vmc/vmg
				//calculate vmc/vmg for the opposite tack based on reversing the angle to the wind
			//else
				//find the vmc/vmg
				//find the previous tack, vmc/vmg for that tack
				//add/subtract the current tack delta

		}

		public void Dispose ()
		{
			
		}
	}
}

