using System;
using MrGibbs.Contracts;
using Ninject.Modules;

namespace MrGibbs.HMC5883
{
    /// <summary>
    /// configures types releavant to this plugin
    /// </summary>
	public class Hmc5883Module:NinjectModule
	{
		public override void Load()
		{
			Kernel.Bind<IPlugin> ()
				.To<Hmc5883Plugin> ()
				.InSingletonScope ();
		}
	}
}