using System;
using MrGibbs.Contracts;
using MrGibbs.Configuration;
using Ninject.Modules;

namespace MrGibbs.Calculators
{
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