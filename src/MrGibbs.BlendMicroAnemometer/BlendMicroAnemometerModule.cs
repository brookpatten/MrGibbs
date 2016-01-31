using System;
using MrGibbs.Contracts;
using MrGibbs.Configuration;
using Ninject.Modules;

namespace MrGibbs.BlendMicroAnemometer
{
	public class BlendMicroAnemometerModule:NinjectModule
	{
		public override void Load()
		{
			Kernel.Bind<IPlugin> ()
				.To<BlendMicroAnemometerPlugin> ()
				.InSingletonScope ();
		}
	}
}