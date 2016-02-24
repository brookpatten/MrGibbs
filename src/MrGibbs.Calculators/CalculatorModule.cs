using System;
using MrGibbs.Contracts;
using Ninject.Modules;

namespace MrGibbs.Calculators
{
    /// <summary>
    /// configures types releavant to this plugin
    /// </summary>
	public class CalculatorModule:NinjectModule
	{
		public override void Load()
		{
			Kernel.Bind<IPlugin> ()
				.To<CalculatorPlugin> ()
				.InSingletonScope ();
		}
	}
}