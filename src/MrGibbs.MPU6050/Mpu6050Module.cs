using System;

using Ninject;
using Ninject.Modules;
using Mono.Linux.I2C;

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

			Kernel.Bind<I2CDevice> ()
			      .ToSelf ()
			      .InSingletonScope ()
			      .Named ("mpu6050")
			      .WithConstructorArgument ("deviceAddress", Mpu6050.MPU6050_ADDRESS_AD0_HIGH);

			Kernel.Bind<Mpu6050> ()
			      .ToSelf ()
			      .InSingletonScope ()
			      .WithConstructorArgument ("device", c => c.Kernel.Get<I2CDevice> ("mpu6050"));

			Kernel.Bind<Imu6050> ()
				  .ToSelf ()
				  .InSingletonScope ();

			Kernel.Bind<IPlugin> ()
				.To<Mpu6050Plugin> ()
				.InSingletonScope ();
		}
	}
}