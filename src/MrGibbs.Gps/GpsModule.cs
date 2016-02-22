﻿using System;
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
				.WithConstructorArgument("simulated",ConfigurationHelper.ReadBoolAppSetting("simulatedGps",false))
				.WithConstructorArgument ("gpsPort", AppConfig.GpsPort)
				.WithConstructorArgument ("gpsBaud", AppConfig.GpsBaud);
		}
	}
}