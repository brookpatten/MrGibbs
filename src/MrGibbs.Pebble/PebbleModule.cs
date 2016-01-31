using System;
using MrGibbs.Contracts;
using MrGibbs.Configuration;
using Ninject.Modules;

namespace MrGibbs.Pebble
{
	public class PebbleModule:NinjectModule
	{
		public override void Load()
		{
			Kernel.Bind<IPlugin> ().To<PebblePlugin> ()
				.InSingletonScope ()
				.WithConstructorArgument ("pbwPath", ConfigurationHelper.ReadStringAppSetting("PbwPath","Mr._Gibbs.pbw"))
				.WithConstructorArgument ("btAdapterName", ConfigurationHelper.ReadStringAppSetting("BtAdapterName",""));

		}
	}
}

