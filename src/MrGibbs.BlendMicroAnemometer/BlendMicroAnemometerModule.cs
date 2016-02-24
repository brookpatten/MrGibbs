using System;
using MrGibbs.Contracts;
using Ninject.Modules;

namespace MrGibbs.BlendMicroAnemometer
{
    /// <summary>
    /// configures types releavant to this plugin
    /// </summary>
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