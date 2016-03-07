using System;

using Ninject;
using Ninject.Modules;

using MrGibbs.Contracts;
using MrGibbs.Configuration;

namespace MrGibbs.Pebble
{
    /// <summary>
    /// configures types releavant to this plugin
    /// </summary>
	public class PebbleModule:NinjectModule
	{
		public override void Load()
		{
			Kernel.LoadIfNotLoaded<BluetoothModule> ();

			Kernel.Bind<IPlugin> ().To<PebblePlugin> ()
				.InSingletonScope ()
				.WithConstructorArgument ("pbwPath", ConfigurationHelper.ReadStringAppSetting("PbwPath"
					,ConfigurationHelper.FindNewestFileWithExtension("pbw")))
				.WithConstructorArgument ("btAdapterName", ConfigurationHelper.ReadStringAppSetting("BtAdapterName","hci0"));

		}
	}
}

