using System;
using MrGibbs.Contracts;
using MrGibbs.Configuration;
using Ninject.Modules;

namespace MrGibbs.HMC5883
{
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