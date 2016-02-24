using System;
using MrGibbs.Contracts;
using MrGibbs.Configuration;
using Ninject.Modules;

namespace MrGibbs.Pebble
{
    /// <summary>
    /// configures types releavant to this plugin
    /// </summary>
	public class PebbleModule:NinjectModule
	{
		public override void Load()
		{
			Kernel.Bind<IPlugin> ().To<PebblePlugin> ()
				.InSingletonScope ()
				.WithConstructorArgument ("pbwPath", ConfigurationHelper.ReadStringAppSetting("PbwPath"
					,ConfigurationHelper.FindNewestFileWithExtension("pbw")))
				.WithConstructorArgument ("btAdapterName", ConfigurationHelper.ReadStringAppSetting("BtAdapterName",""));

		}
	}
}

