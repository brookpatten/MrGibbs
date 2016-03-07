using System;

using Ninject;
using Ninject.Modules;

using MrGibbs.Contracts;
using MrGibbs.Configuration;

namespace MrGibbs.BlendMicroAnemometer
{
    /// <summary>
    /// configures types releavant to this plugin
    /// </summary>
	public class BlendMicroAnemometerModule:NinjectModule
	{
		public override void Load()
		{
			Kernel.LoadIfNotLoaded<BluetoothModule> ();

			Kernel.Bind<IPlugin> ()
				.To<BlendMicroAnemometerPlugin> ()
				  .InSingletonScope ()
			      .WithConstructorArgument ("btAdapterName", ConfigurationHelper.ReadStringAppSetting("BtAdapterName","hci0"))
			      .WithConstructorArgument ("deviceAddress", ConfigurationHelper.ReadStringAppSetting("BlendMicroAnemometerAddress",""));
		}
	}
}