using System;
using MrGibbs.Contracts;
using MrGibbs.Configuration;
using Ninject.Modules;

namespace MrGibbs.Gps
{
	public class GpsModule:NinjectModule
	{
		public override void Load()
		{
			Kernel.Bind<IPlugin> ().To<GpsPlugin> ()
				.InSingletonScope ()
				.WithConstructorArgument ("gpsPort", AppConfig.GpsPort)
				.WithConstructorArgument ("gpsBaud", AppConfig.GpsBaud);
		}
	}
}