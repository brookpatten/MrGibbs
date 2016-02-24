using System;
using MrGibbs.Contracts;
using Ninject.Modules;

namespace MrGibbs.MPU6050
{
    /// <summary>
    /// configures types releavant to this plugin
    /// </summary>
	public class Mpu6050Module:NinjectModule
	{
		public override void Load()
		{
			Kernel.Bind<IPlugin> ()
				.To<Mpu6050Plugin> ()
				.InSingletonScope ();
		}
	}
}