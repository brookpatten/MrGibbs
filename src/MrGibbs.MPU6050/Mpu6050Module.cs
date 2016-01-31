using System;
using MrGibbs.Contracts;
using MrGibbs.Configuration;
using Ninject.Modules;

namespace MrGibbs.MPU6050
{
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