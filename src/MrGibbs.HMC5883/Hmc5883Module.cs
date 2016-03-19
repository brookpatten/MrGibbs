using System;

using Ninject;
using Ninject.Modules;
using Mono.Linux.I2C;

using MrGibbs.Contracts;
using MrGibbs.Configuration;

namespace MrGibbs.HMC5883
{
    /// <summary>
    /// configures types releavant to this plugin
    /// </summary>
	public class Hmc5883Module:NinjectModule
	{
		public override void Load()
		{
			//loads the bus
			Kernel.LoadIfNotLoaded<I2CModule> ();

			Kernel.Bind<I2CDevice> ()
				  .ToSelf ()
				  .InSingletonScope ()
				  .Named ("hmc5883l")
				  .WithConstructorArgument ("deviceAddress", Hmc5883.Hmc5883l_Default_Address);
			
			Kernel.Bind<Hmc5883> ()
				  .ToSelf ()
				  .InSingletonScope ()
			      .WithConstructorArgument ("device", c => c.Kernel.Get<I2CDevice> ("hmc5883l"));
			
			Kernel.Bind<IPlugin> ()
				.To<Hmc5883Plugin> ()
				.InSingletonScope ();
		}
	}
}