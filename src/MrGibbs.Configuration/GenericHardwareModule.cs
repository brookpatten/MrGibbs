using System;
using QuadroschrauberSharp.Hardware;
using Ninject.Modules;

namespace MrGibbs.Configuration
{
	public class GenericHardwareModule:NinjectModule
	{
		public override void Load()
		{
			Kernel.Bind<I2C> ()
				.ToSelf()
				.WithConstructorArgument("index", AppConfig.I2CAddress);
		}
	}
}

