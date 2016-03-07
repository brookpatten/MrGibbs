using System;

using Ninject;
using Ninject.Modules;

using MrGibbs.Contracts;
using MrGibbs.Configuration;

namespace MrGibbs.MPU6050
{
    /// <summary>
    /// configures types releavant to this plugin
    /// </summary>
	public class Mpu6050Module:NinjectModule
	{
		public override void Load()
		{
			Kernel.LoadIfNotLoaded<I2CModule> ();

			Kernel.Bind<IPlugin> ()
				.To<Mpu6050Plugin> ()
				.InSingletonScope ();
		}
	}
}