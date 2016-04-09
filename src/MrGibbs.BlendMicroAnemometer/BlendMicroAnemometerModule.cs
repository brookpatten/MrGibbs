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
			      .WithConstructorArgument("simulated",AppConfig.SimulateSensorData)
				  .WithConstructorArgument ("maximumDataAge", new TimeSpan (0, 0, 3))
			      .WithConstructorArgument ("btAdapterName", ConfigurationHelper.ReadStringAppSetting("BtAdapterName","hci0"))
			      .WithConstructorArgument ("deviceAddress", ConfigurationHelper.ReadStringAppSetting("BlendMicroAnemometerAddress",""));
		}

		public override void Unload ()
		{
			var connection = Kernel.Get<BlendMicroAnemometerPlugin> ();
			connection.Dispose ();
			base.Unload ();
		}
	}
}